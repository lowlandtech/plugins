using LowlandTech.Graph.Abstractions.Attributes;
using LowlandTech.Graph.Abstractions.Enums;

namespace BaGet.Core.Models.Graph;

/// <summary>
/// Graph representation of a package for GraphClient operations.
/// Maps to/from the frontend Package entity.
/// </summary>
[GraphNode("plugin.package", Name = "Package", Icon = "inventory_2")]
public class GraphPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The NuGet package ID (e.g., "Newtonsoft.Json").
    /// </summary>
    [GraphProperty(Required = true)]
    public string PackageId { get; set; } = string.Empty;

    [GraphProperty(Required = true)]
    public string NormalizedVersionString { get; set; } = string.Empty;

    [GraphProperty]
    public string? OriginalVersionString { get; set; }

    [GraphProperty(PropertyTypes.Json)]
    public string[]? Authors { get; set; }

    [GraphProperty]
    public string? Description { get; set; }

    [GraphProperty(PropertyTypes.Long)]
    public long Downloads { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool HasReadme { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool HasEmbeddedIcon { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool IsPrerelease { get; set; }

    [GraphProperty]
    public string? ReleaseNotes { get; set; }

    [GraphProperty]
    public string? Language { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool Listed { get; set; }

    [GraphProperty]
    public string? MinClientVersion { get; set; }

    [GraphProperty(PropertyTypes.DateTime)]
    public DateTime Published { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool RequireLicenseAcceptance { get; set; }

    [GraphProperty(PropertyTypes.Int)]
    public int SemVerLevel { get; set; }

    [GraphProperty]
    public string? Summary { get; set; }

    [GraphProperty]
    public string? Title { get; set; }

    [GraphProperty]
    public string? IconUrl { get; set; }

    [GraphProperty]
    public string? LicenseUrl { get; set; }

    [GraphProperty]
    public string? ProjectUrl { get; set; }

    [GraphProperty]
    public string? RepositoryUrl { get; set; }

    [GraphProperty]
    public string? RepositoryType { get; set; }

    [GraphProperty(PropertyTypes.Json)]
    public string[]? Tags { get; set; }

    /// <summary>
    /// Required tier level for access (0=Free, 1=Premium, etc.)
    /// </summary>
    [GraphProperty(PropertyTypes.Int)]
    public int RequiredTierLevel { get; set; }
}
