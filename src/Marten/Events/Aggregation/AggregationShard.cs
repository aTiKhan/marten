using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using Marten.Linq.SqlGeneration;
using Marten.Storage;
using Microsoft.Extensions.Logging;

namespace Marten.Events.Aggregation
{

    public class AggregationShard<TDoc, TId> : IAsyncProjectionShard
    {
        private readonly AggregationRuntime<TDoc, TId> _runtime;
        private readonly ITenancy _tenancy;
        private TransformBlock<EventRange, AggregateRange> _slicing;
        private ActionBlock<AggregateRange> _building;
        private IProjectionUpdater _updater;
        private ILogger<IProjection> _logger;

        public AggregationShard(string projectionOrShardName, ISqlFragment[] eventFilters,
            AggregationRuntime<TDoc, TId> runtime, ITenancy tenancy)
        {
            _runtime = runtime;
            _tenancy = tenancy;
            EventFilters = eventFilters;
            ProjectionOrShardName = projectionOrShardName;
        }

        public ISqlFragment[] EventFilters { get; }

        public string ProjectionOrShardName { get; }
        public AsyncOptions Options { get; }

        public ITargetBlock<EventRange> Start(IProjectionUpdater updater, ILogger<IProjection> logger)
        {
            _updater = updater;
            _logger = logger;

            var singleFileOptions = new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            };

            _slicing = new TransformBlock<EventRange, AggregateRange>(x => slice(x), singleFileOptions);


            _building = new ActionBlock<AggregateRange>(configureBatch, singleFileOptions);

            _slicing.LinkTo(_building);

            return _slicing;
        }

        private async Task configureBatch(AggregateRange aggregateRange)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Shard '{ProjectionOrShardName}': Starting to build an update batch for {aggregateRange}");
            }

            var batch = _updater.StartNewBatch(aggregateRange.Range);
            await _runtime.Configure(batch.Queue, aggregateRange.Slices);
            batch.Queue.Complete();
            await batch.Queue.Completion;

            await _updater.ExecuteBatch(batch);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Shard '{ProjectionOrShardName}': Configured batch {aggregateRange}");
            }
        }

        internal class AggregateRange
        {
            public AggregateRange(EventRange range, IReadOnlyList<EventSlice<TDoc, TId>> slices)
            {
                Range = range;
                Slices = slices;
            }

            public EventRange Range { get; }
            public IReadOnlyList<EventSlice<TDoc, TId>> Slices { get; }

            public override string ToString()
            {
                return $"Aggregate for {Range}, {Slices.Count} slices";
            }
        }

        private AggregateRange slice(EventRange range)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Shard '{ProjectionOrShardName}': Starting to slice {range}");
            }

            var slices = _runtime.Slicer.Slice(range.Events, _tenancy);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Shard '{ProjectionOrShardName}': successfully sliced {range}");
            }

            return new AggregateRange(range, slices);
        }

        public async Task Stop()
        {
            await _slicing.Completion;
            await _building.Completion;

            _slicing.Complete();
            _building.Complete();

            _logger.LogInformation($"Shard '{ProjectionOrShardName}': Stopped");
        }
    }
}
