using BaGet.Core.Models;
using BaGet.Core.Models.Graph;
using LowlandTech.Graph.Abstractions.Client;

namespace BaGet.Core.Services;

/// <summary>
/// Subscription service using GraphClient for data access.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IGraphClient _client;

    public SubscriptionService(IGraphClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<SubscriptionPlan>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _client.QueryAsync<SubscriptionPlan>(0, 100, cancellationToken);
        return plans.Where(p => p.IsActive).OrderBy(p => p.AccessLevel).ToList();
    }

    public async Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<SubscriptionPlan>(planId, cancellationToken);
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _client.QueryAsync<UserSubscription>(0, 1000, cancellationToken);
        var active = subscriptions
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();

        if (active != null)
        {
            // Load the related plan
            var plans = await _client.GetRelatedAsync<UserSubscription, SubscriptionPlan>(active.Id, "subscribes.to", cancellationToken);
            active.Plan = plans.FirstOrDefault();
        }

        return active;
    }

    public async Task<UserSubscription> CreateSubscriptionAsync(string userId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _client.GetAsync<SubscriptionPlan>(planId, cancellationToken);
        if (plan == null)
            throw new ArgumentException($"Plan {planId} not found", nameof(planId));

        // Cancel any existing active subscription
        var existing = await GetActiveSubscriptionAsync(userId, cancellationToken);
        if (existing != null)
        {
            existing.Status = SubscriptionStatus.Cancelled;
            existing.EndDate = DateTime.UtcNow;
            await _client.UpdateAsync(existing, cancellationToken);
        }

        // Create new subscription
        var subscription = new UserSubscription
        {
            UserId = userId,
            StartDate = DateTime.UtcNow,
            Status = SubscriptionStatus.Active,
            DownloadsThisMonth = 0
        };

        var created = await _client.CreateAsync(subscription, cancellationToken);

        // Create edge to plan
        await _client.CreateEdgeAsync<UserSubscription, SubscriptionPlan>(created.Id, planId, "subscribes.to", cancellationToken);

        created.Plan = plan;
        return created;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _client.GetAsync<UserSubscription>(subscriptionId, cancellationToken);
        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.EndDate = DateTime.UtcNow;
        await _client.UpdateAsync(subscription, cancellationToken);
        return true;
    }

    public async Task<UserSubscription> ChangePlanAsync(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken = default)
    {
        var subscription = await _client.GetAsync<UserSubscription>(subscriptionId, cancellationToken);
        if (subscription == null)
            throw new ArgumentException($"Subscription {subscriptionId} not found", nameof(subscriptionId));

        var newPlan = await _client.GetAsync<SubscriptionPlan>(newPlanId, cancellationToken);
        if (newPlan == null)
            throw new ArgumentException($"Plan {newPlanId} not found", nameof(newPlanId));

        // Delete old edge and create new one
        var oldPlans = await _client.GetRelatedAsync<UserSubscription, SubscriptionPlan>(subscriptionId, "subscribes.to", cancellationToken);
        if (oldPlans.Any())
        {
            await _client.DeleteEdgeAsync(subscriptionId, oldPlans.First().Id, "subscribes.to", cancellationToken);
        }

        await _client.CreateEdgeAsync<UserSubscription, SubscriptionPlan>(subscriptionId, newPlanId, "subscribes.to", cancellationToken);

        subscription.Plan = newPlan;
        return subscription;
    }

    public async Task<bool> CanDownloadPackageAsync(string userId, Guid packageNodeId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionAsync(userId, cancellationToken);
        if (subscription?.Plan == null)
            return false;

        // Check download limit
        if (subscription.Plan.MaxDownloadsPerMonth > 0 &&
            subscription.DownloadsThisMonth >= subscription.Plan.MaxDownloadsPerMonth)
            return false;

        // Check package tier requirement
        var package = await _client.GetAsync<GraphPackage>(packageNodeId, cancellationToken);
        if (package == null)
            return false;

        // Check if user's subscription level meets the package requirement
        if (subscription.Plan.AccessLevel < package.RequiredTierLevel)
            return false;

        return true;
    }

    public async Task IncrementDownloadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionAsync(userId, cancellationToken);
        if (subscription != null)
        {
            subscription.DownloadsThisMonth++;
            await _client.UpdateAsync(subscription, cancellationToken);
        }
    }

    public async Task ResetMonthlyDownloadCountsAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _client.QueryAsync<UserSubscription>(0, 10000, cancellationToken);
        foreach (var sub in subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
        {
            sub.DownloadsThisMonth = 0;
            await _client.UpdateAsync(sub, cancellationToken);
        }
    }
}
