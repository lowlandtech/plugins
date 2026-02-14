using LowlandTech.Graph.Abstractions.Attributes;
using LowlandTech.Graph.Abstractions.Enums;

namespace BaGet.Core.Models;

/// <summary>
/// Defines an access tier that packages can require.
/// </summary>
[GraphNode("plugin.tier", Name = "Access Tier", Icon = "lock")]
public class PackageTier
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name: Free, Premium, Premium+, Business, Enterprise
    /// </summary>
    [GraphProperty(Required = true)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Access level requirement: 0=Free, 1=Premium, 2=Premium+, 3=Business, 4=Enterprise
    /// Packages with this tier require users to have at least this access level.
    /// </summary>
    [GraphProperty(PropertyTypes.Int)]
    public int Level { get; set; }
}
