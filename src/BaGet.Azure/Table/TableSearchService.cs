using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using BaGet.Core;
using BaGet.Protocol.Models;

namespace BaGet.Azure
{
    public class TableSearchService : ISearchService
    {
        private const string TableName = "Packages";

        private readonly TableClient _table;
        private readonly ISearchResponseBuilder _responseBuilder;

        public TableSearchService(
            TableServiceClient tableService,
            ISearchResponseBuilder responseBuilder)
        {
            _table = tableService?.GetTableClient(TableName) ?? throw new ArgumentNullException(nameof(tableService));
            _responseBuilder = responseBuilder ?? throw new ArgumentNullException(nameof(responseBuilder));
        }

        public async Task<SearchResponse> SearchAsync(
            SearchRequest request,
            CancellationToken cancellationToken)
        {
            var results = await SearchAsync(
                request.Query,
                request.Skip,
                request.Take,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                cancellationToken);

            return _responseBuilder.BuildSearch(results);
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(
            AutocompleteRequest request,
            CancellationToken cancellationToken)
        {
            var results = await SearchAsync(
                request.Query,
                request.Skip,
                request.Take,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                cancellationToken);

            var packageIds = results.Select(p => p.PackageId).ToList();

            return _responseBuilder.BuildAutocomplete(packageIds);
        }

        public Task<AutocompleteResponse> ListPackageVersionsAsync(
            VersionsRequest request,
            CancellationToken cancellationToken)
        {
            // TODO: Support versions autocomplete.
            // See: https://github.com/loic-sharma/BaGet/issues/291
            var response = _responseBuilder.BuildAutocomplete(new List<string>());

            return Task.FromResult(response);
        }

        public Task<DependentsResponse> FindDependentsAsync(string packageId, CancellationToken cancellationToken)
        {
            var response = _responseBuilder.BuildDependents(new List<PackageDependent>());

            return Task.FromResult(response);
        }

        private async Task<List<PackageRegistration>> SearchAsync(
            string? searchText,
            int skip,
            int take,
            bool includePrerelease,
            bool includeSemVer2,
            CancellationToken cancellationToken)
        {
            var filter = GenerateSearchFilter(searchText, includePrerelease, includeSemVer2);
            var query = _table.QueryAsync<PackageEntity>(filter: filter, maxPerPage: 500, cancellationToken: cancellationToken);

            var results = await LoadPackagesAsync(query, maxPartitions: skip + take, cancellationToken);

            return results
                .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PackageRegistration(group.Key, group.ToList()))
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        private async Task<IReadOnlyList<Package>> LoadPackagesAsync(
            AsyncPageable<PackageEntity> query,
            int maxPartitions,
            CancellationToken cancellationToken)
        {
            var results = new List<Package>();

            var partitions = 0;
            string? lastPartitionKey = null;

            await foreach (var result in query.WithCancellation(cancellationToken))
            {
                if (lastPartitionKey != result.PartitionKey)
                {
                    lastPartitionKey = result.PartitionKey;
                    partitions++;

                    if (partitions > maxPartitions)
                    {
                        break;
                    }
                }

                results.Add(result.AsPackage());
            }

            return results;
        }

        private string GenerateSearchFilter(string? searchText, bool includePrerelease, bool includeSemVer2)
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Filter to rows where the "searchText" prefix matches on the partition key.
                var prefix = searchText.TrimEnd().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Last();

                var prefixLower = prefix;
                var prefixUpper = prefix + "~";

                filters.Add($"PartitionKey ge '{prefixLower}'");
                filters.Add($"PartitionKey le '{prefixUpper}'");
            }

            // Filter to rows that are listed.
            filters.Add("Listed eq true");

            if (!includePrerelease)
            {
                filters.Add("IsPrerelease eq false");
            }

            if (!includeSemVer2)
            {
                filters.Add("SemVerLevel eq 0");
            }

            return string.Join(" and ", filters);
        }
    }
}
