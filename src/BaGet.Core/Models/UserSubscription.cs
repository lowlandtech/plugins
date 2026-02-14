using LowlandTech.Graph.Abstractions.Attributes;
using LowlandTech.Graph.Abstractions.Enums;

namespace BaGet.Core.Models;

/// <summary>
/// Represents a user's active subscription to a plan.
/// </summary>
[GraphNode("plugin.subscription", Name = "User Subscription", Icon = "subscriptions")]
public class UserSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Links to ASP.NET Identity user ID (not stored in graph).
    /// </summary>
    [GraphIgnore]
    public string UserId { get; set; } = string.Empty;

    [GraphProperty(PropertyTypes.DateTime)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [GraphProperty(PropertyTypes.DateTime)]
    public DateTime? EndDate { get; set; }

    [GraphProperty(PropertyTypes.Enum)]
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    /// <summary>
    /// Download counter for the current billing period.
    /// </summary>
    [GraphProperty(PropertyTypes.Int)]
    public int DownloadsThisMonth { get; set; }

    /// <summary>
    /// The subscription plan this user subscribes to.
    /// </summary>
    [GraphEdge("subscribes.to")]
    public SubscriptionPlan? Plan { get; set; }
}

public enum SubscriptionStatus
{
    Active,
    Cancelled,
    Expired,
    Trial
}
