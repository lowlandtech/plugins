using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using BaGet.Core;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace BaGet.Azure
{
    /// <summary>
    /// Stores the metadata of packages using Azure Table Storage.
    /// </summary>
    public class TablePackageDatabase : IPackageDatabase
    {
        private const string TableName = "Packages";
        private const int MaxPreconditionFailures = 5;

        private readonly TableOperationBuilder _operationBuilder;
        private readonly TableClient _table;
        private readonly ILogger<TablePackageDatabase> _logger;

        public TablePackageDatabase(
            TableOperationBuilder operationBuilder,
            TableServiceClient tableService,
            ILogger<TablePackageDatabase> logger)
        {
            _operationBuilder = operationBuilder ?? throw new ArgumentNullException(nameof(operationBuilder));
            _table = tableService?.GetTableClient(TableName) ?? throw new ArgumentNullException(nameof(tableService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PackageAddResult> AddAsync(Package package, CancellationToken cancellationToken)
        {
            try
            {
                var entity = _operationBuilder.BuildPackageEntity(package);
                await _table.AddEntityAsync(entity, cancellationToken);
            }
            catch (RequestFailedException e) when (e.IsAlreadyExistsException())
            {
                return PackageAddResult.PackageAlreadyExists;
            }

            return PackageAddResult.Success;
        }

        public async Task AddDownloadAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken)
        {
            var attempt = 0;
            var (partitionKey, rowKey) = _operationBuilder.GetEntityKeys(id, version);

            while (true)
            {
                try
                {
                    var response = await _table.GetEntityAsync<PackageDownloadsEntity>(
                        partitionKey,
                        rowKey,
                        cancellationToken: cancellationToken);

                    var entity = response.Value;
                    if (entity == null)
                    {
                        return;
                    }

                    entity.Downloads += 1;

                    await _table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Merge, cancellationToken);
                    return;
                }
                catch (RequestFailedException e) when (e.IsNotFoundException())
                {
                    return;
                }
                catch (RequestFailedException e)
                    when (attempt < MaxPreconditionFailures && e.IsPreconditionFailedException())
                {
                    attempt++;
                    _logger.LogWarning(
                        e,
                        $"Retrying due to precondition failure, attempt {{Attempt}} of {MaxPreconditionFailures}..",
                        attempt);
                }
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            var query = _table.QueryAsync<PackageEntity>(
                filter: $"PartitionKey eq '{id.ToLowerInvariant()}'",
                maxPerPage: 1,
                select: new[] { "PartitionKey" },
                cancellationToken: cancellationToken);

            await foreach (var _ in query)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ExistsAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken)
        {
            var (partitionKey, rowKey) = _operationBuilder.GetEntityKeys(id, version);

            try
            {
                await _table.GetEntityAsync<PackageEntity>(
                    partitionKey,
                    rowKey,
                    select: new[] { "PartitionKey" },
                    cancellationToken: cancellationToken);

                return true;
            }
            catch (RequestFailedException e) when (e.IsNotFoundException())
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<Package>> FindAsync(string id, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var filter = $"PartitionKey eq '{id.ToLowerInvariant()}'";
            if (!includeUnlisted)
            {
                filter = $"{filter} and Listed eq true";
            }

            var query = _table.QueryAsync<PackageEntity>(filter: filter, cancellationToken: cancellationToken);
            var results = new List<Package>();

            await foreach (var entity in query)
            {
                results.Add(entity.AsPackage());
            }

            return results.OrderBy(p => p.Version).ToList();
        }

        public async Task<Package?> FindOrNullAsync(
            string id,
            NuGetVersion version,
            bool includeUnlisted,
            CancellationToken cancellationToken)
        {
            var (partitionKey, rowKey) = _operationBuilder.GetEntityKeys(id, version);

            try
            {
                var response = await _table.GetEntityAsync<PackageEntity>(
                    partitionKey,
                    rowKey,
                    cancellationToken: cancellationToken);

                var entity = response.Value;

                // Filter out the package if it's unlisted.
                if (!includeUnlisted && !entity.Listed)
                {
                    return null;
                }

                return entity.AsPackage();
            }
            catch (RequestFailedException e) when (e.IsNotFoundException())
            {
                return null;
            }
        }

        public async Task<bool> HardDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            var (partitionKey, rowKey) = _operationBuilder.GetEntityKeys(id, version);

            try
            {
                await _table.DeleteEntityAsync(partitionKey, rowKey, ETag.All, cancellationToken);
                return true;
            }
            catch (RequestFailedException e) when (e.IsNotFoundException())
            {
                return false;
            }
        }

        public async Task<bool> RelistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await TryUpdateListingAsync(id, version, listed: true, cancellationToken);
        }

        public async Task<bool> UnlistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await TryUpdateListingAsync(id, version, listed: false, cancellationToken);
        }

        private async Task<bool> TryUpdateListingAsync(string id, NuGetVersion version, bool listed, CancellationToken cancellationToken)
        {
            var entity = _operationBuilder.BuildListingEntity(id, version, listed);

            try
            {
                await _table.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Merge, cancellationToken);
                return true;
            }
            catch (RequestFailedException e) when (e.IsNotFoundException())
            {
                return false;
            }
        }
    }
}
