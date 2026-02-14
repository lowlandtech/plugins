using BaGet.Core.Models.Graph;
using LowlandTech.Graph.Abstractions.Attributes;
using LowlandTech.Graph.Abstractions.Enums;

namespace BaGet.Core.Models;

/// <summary>
/// Represents a license granting a user access to a specific package.
/// </summary>
[GraphNode("plugin.license", Name = "Package License", Icon = "verified")]
public class PackageLicense
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Links to ASP.NET Identity user ID (not stored in graph).
    /// </summary>
    [GraphIgnore]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Graph node ID of the licensed package.
    /// </summary>
    [GraphProperty]
    public Guid PackageNodeId { get; set; }

    /// <summary>
    /// Optional license key for validation.
    /// </summary>
    [GraphProperty]
    public string? LicenseKey { get; set; }

    [GraphProperty(PropertyTypes.DateTime)]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [GraphProperty(PropertyTypes.DateTime)]
    public DateTime? ExpiresAt { get; set; }

    [GraphProperty(PropertyTypes.Enum)]
    public LicenseType Type { get; set; } = LicenseType.Standard;

    /// <summary>
    /// The package this license grants access to (loaded via edge).
    /// </summary>
    [GraphEdge("licenses")]
    public GraphPackage? Package { get; set; }
}

public enum LicenseType
{
    Standard,
    Enterprise,
    Perpetual,
    Subscription
}
