using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BaGet.Core;
using BaGet.Protocol.Models;
using NuGet.Versioning;
using SearchDocumentOptions = global::Azure.Search.Documents.SearchOptions;

namespace BaGet.Azure
{
    public class AzureSearchService : ISearchService
    {
        private readonly SearchClient _searchClient;
        private readonly IUrlGenerator _url;
        private readonly IFrameworkCompatibilityService _frameworks;

        public AzureSearchService(
            SearchClient searchClient,
            IUrlGenerator url,
            IFrameworkCompatibilityService frameworks)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _frameworks = frameworks ?? throw new ArgumentNullException(nameof(frameworks));
        }

        public async Task<SearchResponse> SearchAsync(
            SearchRequest request,
            CancellationToken cancellationToken)
        {
            var searchText = BuildSearchQuery(request.Query, request.PackageType, request.Framework);
            var filter = BuildSearchFilter(request.IncludePrerelease, request.IncludeSemVer2);
            var options = new SearchDocumentOptions
            {
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                Skip = request.Skip,
                Size = request.Take,
                Filter = filter
            };

            var response = await _searchClient.SearchAsync<PackageDocument>(
                searchText,
                options,
                cancellationToken);

            var results = new List<SearchResult>();

            await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
            {
                var document = result.Document;
                var versions = new List<SearchResultVersion>();

                if (document.Versions?.Length != document.VersionDownloads?.Length)
                {
                    throw new InvalidOperationException($"Invalid document {document.Key} with mismatched versions");
                }

                for (var i = 0; i < (document.Versions?.Length ?? 0); i++)
                {
                    var version = NuGetVersion.Parse(document.Versions![i]);

                    versions.Add(new SearchResultVersion
                    {
                        RegistrationLeafUrl = _url.GetRegistrationLeafUrl(document.Id!, version),
                        Version = document.Versions[i],
                        Downloads = long.Parse(document.VersionDownloads![i]),
                    });
                }

                var iconUrl = document.HasEmbeddedIcon
                    ? _url.GetPackageIconDownloadUrl(document.Id!, NuGetVersion.Parse(document.Version!))
                    : document.IconUrl;

                results.Add(new SearchResult
                {
                    PackageId = document.Id!,
                    Version = document.Version!,
                    Description = document.Description,
                    Authors = document.Authors,
                    IconUrl = iconUrl,
                    LicenseUrl = document.LicenseUrl,
                    ProjectUrl = document.ProjectUrl,
                    RegistrationIndexUrl = _url.GetRegistrationIndexUrl(document.Id!),
                    Summary = document.Summary,
                    Tags = document.Tags,
                    Title = document.Title,
                    TotalDownloads = document.TotalDownloads,
                    Versions = versions
                });
            }

            return new SearchResponse
            {
                TotalHits = response.Value.TotalCount ?? 0,
                Data = results,
                Context = SearchContext.Default(_url.GetPackageMetadataResourceUrl())
            };
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(
            AutocompleteRequest request,
            CancellationToken cancellationToken)
        {
            // TODO: Do a prefix search on the package id field.
            // TODO: Support prerelease, semver2, and package type filters.
            // See: https://github.com/loic-sharma/BaGet/issues/291
            var options = new SearchDocumentOptions
            {
                IncludeTotalCount = true,
                Skip = request.Skip,
                Size = request.Take,
            };

            var response = await _searchClient.SearchAsync<PackageDocument>(
                request.Query,
                options,
                cancellationToken);

            var results = new List<string>();
            await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
            {
                if (result.Document.Id != null)
                {
                    results.Add(result.Document.Id);
                }
            }

            return new AutocompleteResponse
            {
                TotalHits = response.Value.TotalCount ?? 0,
                Data = results.AsReadOnly(),
                Context = AutocompleteContext.Default
            };
        }

        public Task<AutocompleteResponse> ListPackageVersionsAsync(
            VersionsRequest request,
            CancellationToken cancellationToken)
        {
            // TODO: Support versions autocomplete.
            // See: https://github.com/loic-sharma/BaGet/issues/291
            throw new NotImplementedException();
        }

        public async Task<DependentsResponse> FindDependentsAsync(
            string packageId,
            CancellationToken cancellationToken)
        {
            // TODO: Escape packageId.
            var query = $"dependencies:{packageId.ToLowerInvariant()}";
            var options = new SearchDocumentOptions
            {
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                Skip = 0,
                Size = 20,
            };

            var response = await _searchClient.SearchAsync<PackageDocument>(query, options, cancellationToken);
            var results = new List<PackageDependent>();

            await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
            {
                results.Add(new PackageDependent
                {
                    Id = result.Document.Id,
                    Description = result.Document.Description,
                    TotalDownloads = result.Document.TotalDownloads
                });
            }

            return new DependentsResponse
            {
                TotalHits = response.Value.TotalCount ?? 0,
                Data = results.AsReadOnly()
            };
        }

        private string BuildSearchQuery(string? query, string? packageType, string? framework)
        {
            var queryBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(query))
            {
                queryBuilder.Append(query.TrimEnd().TrimEnd('*'));
                queryBuilder.Append('*');
            }

            if (!string.IsNullOrEmpty(packageType))
            {
                queryBuilder.Append(" +packageTypes:");
                queryBuilder.Append(packageType);
            }

            if (!string.IsNullOrEmpty(framework))
            {
                var frameworks = _frameworks.FindAllCompatibleFrameworks(framework);

                queryBuilder.Append(" +frameworks:(");
                queryBuilder.Append(string.Join(" ", frameworks));
                queryBuilder.Append(')');
            }

            return queryBuilder.ToString();
        }

        private string BuildSearchFilter(bool includePrerelease, bool includeSemVer2)
        {
            var searchFilters = SearchFilters.Default;

            if (includePrerelease)
            {
                searchFilters |= SearchFilters.IncludePrerelease;
            }

            if (includeSemVer2)
            {
                searchFilters |= SearchFilters.IncludeSemVer2;
            }

            return $"searchFilters eq '{searchFilters}'";
        }
    }
}
