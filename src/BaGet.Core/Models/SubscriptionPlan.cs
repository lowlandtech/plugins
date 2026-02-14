using LowlandTech.Graph.Abstractions.Attributes;
using LowlandTech.Graph.Abstractions.Enums;

namespace BaGet.Core.Models;

/// <summary>
/// Defines a subscription tier with pricing and access levels.
/// Tiers: Free (0), Premium (1), Premium+ (2), Business (3), Enterprise (4)
/// </summary>
[GraphNode("plugin.plan", Name = "Subscription Plan", Icon = "card_membership")]
public class SubscriptionPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name: Free, Premium, Premium+, Business, Enterprise
    /// </summary>
    [GraphProperty(Required = true)]
    public string Name { get; set; } = string.Empty;

    [GraphProperty]
    public string? Description { get; set; }

    [GraphProperty(PropertyTypes.Decimal)]
    public decimal MonthlyPrice { get; set; }

    [GraphProperty(PropertyTypes.Decimal)]
    public decimal AnnualPrice { get; set; }

    /// <summary>
    /// Maximum downloads per month. 0 = unlimited.
    /// </summary>
    [GraphProperty(PropertyTypes.Int)]
    public int MaxDownloadsPerMonth { get; set; }

    /// <summary>
    /// Access level: 0=Free, 1=Premium, 2=Premium+, 3=Business, 4=Enterprise
    /// </summary>
    [GraphProperty(PropertyTypes.Int)]
    public int AccessLevel { get; set; }

    [GraphProperty(PropertyTypes.Bool)]
    public bool IsActive { get; set; } = true;
}
