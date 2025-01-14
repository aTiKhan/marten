using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Marten.Internal;
using Marten.Internal.Storage;
using Marten.Schema;
using Marten.Schema.Identity.Sequences;
using Weasel.Core;
using Weasel.Core.Migrations;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;

namespace Marten.Storage
{
    public partial class MartenDatabase: PostgresqlDatabase, IMartenDatabase
    {
        private readonly IConnectionFactory _factory;
        private readonly StorageFeatures _features;


        private readonly StoreOptions _options;

        private Lazy<SequenceFactory> _sequences;

        // TODO -- need to name the databases some how
        public MartenDatabase(StoreOptions options, IConnectionFactory factory, string tenantId)
            : base(options, options.AutoCreateSchemaObjects, options.Advanced.Migrator, "Marten", factory.Create)
        {
            TenantId = tenantId;
            _features = options.Storage;
            _options = options;
            _factory = factory;

            resetSequences();

            Providers = options.Providers;
        }


        public string TenantId { get; }

        public ISequences Sequences => _sequences.Value;

        public IProviderGraph Providers { get; private set; }

        /// <summary>
        ///     Set the minimum sequence number for a Hilo sequence for a specific document type
        ///     to the specified floor. Useful for migrating data between databases
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="floor"></param>
        public Task ResetHiloSequenceFloor<T>(long floor)
        {
            var sequence = Sequences.SequenceFor(typeof(T));
            return sequence.SetFloor(floor);
        }

        public override void ResetSchemaExistenceChecks()
        {
            base.ResetSchemaExistenceChecks();
            resetSequences();
        }

        public IDocumentStorage<T> StorageFor<T>()
        {
            var documentProvider = Providers.StorageFor<T>();
            return documentProvider.QueryOnly;
        }

        public async Task<IReadOnlyList<DbObjectName>> DocumentTables()
        {
            var tables = await this.SchemaTables().ConfigureAwait(false);
            return tables.Where(x => x.Name.StartsWith(SchemaConstants.TablePrefix)).ToList();
        }

        public async Task<IReadOnlyList<DbObjectName>> Functions()
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);

            var schemaNames = AllSchemaNames();

            return await conn.ExistingFunctions("mt_%", schemaNames).ConfigureAwait(false);
        }

        public async Task<Table> ExistingTableFor(Type type)
        {
            var mapping = _features.MappingFor(type).As<DocumentMapping>();
            var expected = mapping.Schema.Table;

            await using var conn = CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);

            return await expected.FetchExisting(conn).ConfigureAwait(false);
        }

        public override IFeatureSchema FindFeature(Type featureType)
        {
            return _features.FindFeature(featureType);
        }

        private void resetSequences()
        {
            _sequences = new Lazy<SequenceFactory>(() =>
            {
                var sequences = new SequenceFactory(_options, this);

#if NETSTANDARD2_0
                generateOrUpdateFeature(typeof(SequenceFactory), sequences, default).GetAwaiter().GetResult();
#else
                generateOrUpdateFeature(typeof(SequenceFactory), sequences, default).AsTask().GetAwaiter().GetResult();
#endif

                return sequences;
            });
        }


        public override IFeatureSchema[] BuildFeatureSchemas()
        {
            return _options.Storage.AllActiveFeatures(this).ToArray();
        }

    }
}
