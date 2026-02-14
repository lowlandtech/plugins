using BaGet.Core.Models;

namespace BaGet.Core;

/// <summary>
/// Service for managing user subscriptions.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Gets all available subscription plans.
    /// </summary>
    Task<IReadOnlyList<SubscriptionPlan>> GetPlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription plan by ID.
    /// </summary>
    Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active subscription for a user.
    /// </summary>
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new subscription for a user.
    /// </summary>
    Task<UserSubscription> CreateSubscriptionAsync(string userId, Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a user's subscription.
    /// </summary>
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's subscription to a different plan.
    /// </summary>
    Task<UserSubscription> ChangePlanAsync(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user can download a package based on their subscription.
    /// </summary>
    Task<bool> CanDownloadPackageAsync(string userId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the download count for a user's current billing period.
    /// </summary>
    Task IncrementDownloadCountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the monthly download counter for all subscriptions (called by scheduled job).
    /// </summary>
    Task ResetMonthlyDownloadCountsAsync(CancellationToken cancellationToken = default);
}
