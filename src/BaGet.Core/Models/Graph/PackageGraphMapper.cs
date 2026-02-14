namespace BaGet.Core.Models.Graph;

/// <summary>
/// Maps between frontend Package entities and GraphClient DTOs.
/// </summary>
public static class PackageGraphMapper
{
    /// <summary>
    /// Converts a frontend Package to a GraphPackage for storage.
    /// </summary>
    public static GraphPackage ToGraph(Package package, Guid? existingNodeId = null)
    {
        return new GraphPackage
        {
            Id = existingNodeId ?? Guid.NewGuid(),
            PackageId = package.Id,
            NormalizedVersionString = package.NormalizedVersionString,
            OriginalVersionString = package.OriginalVersionString,
            Authors = package.Authors,
            Description = package.Description,
            Downloads = package.Downloads,
            HasReadme = package.HasReadme,
            HasEmbeddedIcon = package.HasEmbeddedIcon,
            IsPrerelease = package.IsPrerelease,
            ReleaseNotes = package.ReleaseNotes,
            Language = package.Language,
            Listed = package.Listed,
            MinClientVersion = package.MinClientVersion,
            Published = package.Published,
            RequireLicenseAcceptance = package.RequireLicenseAcceptance,
            SemVerLevel = (int)package.SemVerLevel,
            Summary = package.Summary,
            Title = package.Title,
            IconUrl = package.IconUrl?.AbsoluteUri,
            LicenseUrl = package.LicenseUrl?.AbsoluteUri,
            ProjectUrl = package.ProjectUrl?.AbsoluteUri,
            RepositoryUrl = package.RepositoryUrl?.AbsoluteUri,
            RepositoryType = package.RepositoryType,
            Tags = package.Tags,
            RequiredTierLevel = 0 // Default to free tier
        };
    }

    /// <summary>
    /// Converts a GraphPackage back to a frontend Package.
    /// </summary>
    public static Package FromGraph(GraphPackage graph)
    {
        return new Package
        {
            Id = graph.PackageId,
            NormalizedVersionString = graph.NormalizedVersionString,
            OriginalVersionString = graph.OriginalVersionString,
            Authors = graph.Authors ?? Array.Empty<string>(),
            Description = graph.Description,
            Downloads = graph.Downloads,
            HasReadme = graph.HasReadme,
            HasEmbeddedIcon = graph.HasEmbeddedIcon,
            IsPrerelease = graph.IsPrerelease,
            ReleaseNotes = graph.ReleaseNotes,
            Language = graph.Language,
            Listed = graph.Listed,
            MinClientVersion = graph.MinClientVersion,
            Published = graph.Published,
            RequireLicenseAcceptance = graph.RequireLicenseAcceptance,
            SemVerLevel = (SemVerLevel)graph.SemVerLevel,
            Summary = graph.Summary,
            Title = graph.Title,
            IconUrl = string.IsNullOrEmpty(graph.IconUrl) ? null : new Uri(graph.IconUrl),
            LicenseUrl = string.IsNullOrEmpty(graph.LicenseUrl) ? null : new Uri(graph.LicenseUrl),
            ProjectUrl = string.IsNullOrEmpty(graph.ProjectUrl) ? null : new Uri(graph.ProjectUrl),
            RepositoryUrl = string.IsNullOrEmpty(graph.RepositoryUrl) ? null : new Uri(graph.RepositoryUrl),
            RepositoryType = graph.RepositoryType,
            Tags = graph.Tags ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Updates a GraphPackage from a frontend Package (preserves node ID).
    /// </summary>
    public static void UpdateGraph(GraphPackage target, Package source)
    {
        target.PackageId = source.Id;
        target.NormalizedVersionString = source.NormalizedVersionString;
        target.OriginalVersionString = source.OriginalVersionString;
        target.Authors = source.Authors;
        target.Description = source.Description;
        target.Downloads = source.Downloads;
        target.HasReadme = source.HasReadme;
        target.HasEmbeddedIcon = source.HasEmbeddedIcon;
        target.IsPrerelease = source.IsPrerelease;
        target.ReleaseNotes = source.ReleaseNotes;
        target.Language = source.Language;
        target.Listed = source.Listed;
        target.MinClientVersion = source.MinClientVersion;
        target.Published = source.Published;
        target.RequireLicenseAcceptance = source.RequireLicenseAcceptance;
        target.SemVerLevel = (int)source.SemVerLevel;
        target.Summary = source.Summary;
        target.Title = source.Title;
        target.IconUrl = source.IconUrl?.AbsoluteUri;
        target.LicenseUrl = source.LicenseUrl?.AbsoluteUri;
        target.ProjectUrl = source.ProjectUrl?.AbsoluteUri;
        target.RepositoryUrl = source.RepositoryUrl?.AbsoluteUri;
        target.RepositoryType = source.RepositoryType;
        target.Tags = source.Tags;
    }
}
