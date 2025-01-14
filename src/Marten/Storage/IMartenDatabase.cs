using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten.Internal;
using Marten.Internal.Storage;
using Marten.Schema;
using Marten.Schema.Identity.Sequences;
using Npgsql;
using Weasel.Core;
using Weasel.Core.Migrations;
using Weasel.Postgresql.Tables;

namespace Marten.Storage
{
    /// <summary>
    /// Governs the database structure and migration path for a single Marten database
    /// </summary>
    public interface IMartenDatabase: IDatabase, IConnectionSource<NpgsqlConnection>, IDocumentCleaner
    {
        /// <summary>
        ///     Retrieves or generates the active IDocumentStorage object
        ///     for the given document type
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        IDocumentStorage<T> StorageFor<T>() where T : notnull;


        /// <summary>
        ///     Directs Marten to disregard any previous schema checks. Useful
        ///     if you change the underlying schema without shutting down the document store
        /// </summary>
        void ResetSchemaExistenceChecks();

        /// <summary>
        ///     Rewinds the feature tracking at development time
        /// </summary>
        void MarkAllFeaturesAsChecked();

        /// <summary>
        ///     Ensures that the IDocumentStorage object for a document type is ready
        ///     and also attempts to update the database schema for any detected changes
        /// </summary>
        /// <param name="documentType"></param>
        void EnsureStorageExists(Type documentType);

#if NETSTANDARD2_0
        /// <summary>
        ///     Ensures that the IDocumentStorage object for a document type is ready
        ///     and also attempts to update the database schema for any detected changes
        /// </summary>
        /// <param name="featureType"></param>
        /// <param name="???"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task EnsureStorageExistsAsync(Type featureType, CancellationToken token = default);
        #else
        /// <summary>
        ///     Ensures that the IDocumentStorage object for a document type is ready
        ///     and also attempts to update the database schema for any detected changes
        /// </summary>
        /// <param name="featureType"></param>
        /// <param name="???"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        ValueTask EnsureStorageExistsAsync(Type featureType, CancellationToken token = default);
#endif

        /// <summary>
        ///     Set the minimum sequence number for a Hilo sequence for a specific document type
        ///     to the specified floor. Useful for migrating data between databases
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="floor"></param>
        Task ResetHiloSequenceFloor<T>(long floor);


        /// <summary>
        ///     Used to create new Hilo sequences
        /// </summary>
        ISequences Sequences { get; }


        IProviderGraph Providers { get; }


        Task<IReadOnlyList<DbObjectName>> DocumentTables();
        Task<IReadOnlyList<DbObjectName>> Functions();
        Task<Table> ExistingTableFor(Type type);

        /// <summary>
        /// Fetch a list of the existing tables in the database
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public Task<IReadOnlyList<DbObjectName>> SchemaTables();
    }
}
