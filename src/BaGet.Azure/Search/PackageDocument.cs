using System;
using System.Text.Json.Serialization;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace BaGet.Azure
{
    // See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages
    public class PackageDocument
    {
        public const string IndexName = "packages";

        [SimpleField(IsKey = true)]
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [SearchableField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The package's full versions after normalization, including any SemVer 2.0.0 build metadata.
        /// </summary>
        [SearchableField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [SearchableField]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("authors")]
        public string[]? Authors { get; set; }

        [JsonPropertyName("hasEmbeddedIcon")]
        public bool HasEmbeddedIcon { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }

        [JsonPropertyName("licenseUrl")]
        public string? LicenseUrl { get; set; }

        [JsonPropertyName("projectUrl")]
        public string? ProjectUrl { get; set; }

        [JsonPropertyName("published")]
        public DateTimeOffset Published { get; set; }

        [SearchableField]
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [JsonPropertyName("tags")]
        public string[]? Tags { get; set; }

        [SearchableField]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("totalDownloads")]
        public long TotalDownloads { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("downloadsMagnitude")]
        public int DownloadsMagnitude { get; set; }

        /// <summary>
        /// The package's full versions after normalization, including any SemVer 2.0.0 build metadata.
        /// </summary>
        [JsonPropertyName("versions")]
        public string[]? Versions { get; set; }

        [JsonPropertyName("versionDownloads")]
        public string[]? VersionDownloads { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.Keyword)]
        [JsonPropertyName("dependencies")]
        public string[]? Dependencies { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.Keyword)]
        [JsonPropertyName("packageTypes")]
        public string[]? PackageTypes { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.Keyword)]
        [JsonPropertyName("frameworks")]
        public string[]? Frameworks { get; set; }

        [SimpleField(IsFilterable = true)]
        [JsonPropertyName("searchFilters")]
        public string? SearchFilters { get; set; }
    }
}
