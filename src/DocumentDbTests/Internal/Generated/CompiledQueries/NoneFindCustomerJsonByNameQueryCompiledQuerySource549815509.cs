// <auto-generated/>
#pragma warning disable
using DocumentDbTests.Reading.Json;
using Marten.Internal.CompiledQueries;
using Marten.Linq;
using Marten.Linq.QueryHandlers;
using System;

namespace Marten.Generated.CompiledQueries
{
    // START: NoneFindCustomerJsonByNameQueryCompiledQuery549815509
    public class NoneFindCustomerJsonByNameQueryCompiledQuery549815509 : Marten.Internal.CompiledQueries.ClonedCompiledQuery<System.Collections.Generic.IEnumerable<DocumentDbTests.Reading.Json.streaming_json_results.Customer>, DocumentDbTests.Reading.Json.streaming_json_results.FindCustomerJsonByNameQuery>
    {
        private readonly Marten.Linq.QueryHandlers.IMaybeStatefulHandler _inner;
        private readonly DocumentDbTests.Reading.Json.streaming_json_results.FindCustomerJsonByNameQuery _query;
        private readonly Marten.Linq.QueryStatistics _statistics;
        private readonly Marten.Internal.CompiledQueries.HardCodedParameters _hardcoded;

        public NoneFindCustomerJsonByNameQueryCompiledQuery549815509(Marten.Linq.QueryHandlers.IMaybeStatefulHandler inner, DocumentDbTests.Reading.Json.streaming_json_results.FindCustomerJsonByNameQuery query, Marten.Linq.QueryStatistics statistics, Marten.Internal.CompiledQueries.HardCodedParameters hardcoded) : base(inner, query, statistics, hardcoded)
        {
            _inner = inner;
            _query = query;
            _statistics = statistics;
            _hardcoded = hardcoded;
        }



        public override void ConfigureCommand(Weasel.Postgresql.CommandBuilder builder, Marten.Internal.IMartenSession session)
        {
            var parameters = builder.AppendWithParameters(@"select d.id, d.data from public.mt_doc_streaming_json_results_customer as d where d.data ->> 'LastName' LIKE ? order by d.data ->> 'LastName'");

            parameters[0].NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
            parameters[0].Value = EndsWith(_query.LastNamePrefix);
        }

    }

    // END: NoneFindCustomerJsonByNameQueryCompiledQuery549815509
    
    
    // START: NoneFindCustomerJsonByNameQueryCompiledQuerySource549815509
    public class NoneFindCustomerJsonByNameQueryCompiledQuerySource549815509 : Marten.Internal.CompiledQueries.CompiledQuerySource<System.Collections.Generic.IEnumerable<DocumentDbTests.Reading.Json.streaming_json_results.Customer>, DocumentDbTests.Reading.Json.streaming_json_results.FindCustomerJsonByNameQuery>
    {
        private readonly Marten.Internal.CompiledQueries.HardCodedParameters _hardcoded;
        private readonly Marten.Linq.QueryHandlers.IMaybeStatefulHandler _maybeStatefulHandler;

        public NoneFindCustomerJsonByNameQueryCompiledQuerySource549815509(Marten.Internal.CompiledQueries.HardCodedParameters hardcoded, Marten.Linq.QueryHandlers.IMaybeStatefulHandler maybeStatefulHandler)
        {
            _hardcoded = hardcoded;
            _maybeStatefulHandler = maybeStatefulHandler;
        }



        public override Marten.Linq.QueryHandlers.IQueryHandler<System.Collections.Generic.IEnumerable<DocumentDbTests.Reading.Json.streaming_json_results.Customer>> BuildHandler(DocumentDbTests.Reading.Json.streaming_json_results.FindCustomerJsonByNameQuery query, Marten.Internal.IMartenSession session)
        {
            return new Marten.Generated.CompiledQueries.NoneFindCustomerJsonByNameQueryCompiledQuery549815509(_maybeStatefulHandler, query, null, _hardcoded);
        }

    }

    // END: NoneFindCustomerJsonByNameQueryCompiledQuerySource549815509
    
    
}

