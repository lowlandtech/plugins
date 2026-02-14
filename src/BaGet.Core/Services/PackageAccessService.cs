using BaGet.Core.Models;
using BaGet.Core.Models.Graph;
using LowlandTech.Graph.Abstractions.Client;

namespace BaGet.Core.Services;

/// <summary>
/// Package access service using GraphClient for data access.
/// Combines subscription and license checks for comprehensive access control.
/// </summary>
public class PackageAccessService : IPackageAccessService
{
    private readonly IGraphClient _client;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILicenseService _licenseService;

    public PackageAccessService(
        IGraphClient client,
        ISubscriptionService subscriptionService,
        ILicenseService licenseService)
    {
        _client = client;
        _subscriptionService = subscriptionService;
        _licenseService = licenseService;
    }

    public async Task<bool> CanUserAccessPackageAsync(string userId, Guid packageNodeId, CancellationToken cancellationToken = default)
    {
        // Check 1: Does user have an individual license?
        if (await _licenseService.ValidateLicenseAsync(userId, packageNodeId, cancellationToken))
            return true;

        // Check 2: Does user's subscription allow access?
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId, cancellationToken);
        if (subscription?.Plan == null)
            return false;

        // Check download limits
        if (subscription.Plan.MaxDownloadsPerMonth > 0 &&
            subscription.DownloadsThisMonth >= subscription.Plan.MaxDownloadsPerMonth)
            return false;

        // Check tier requirement
        var package = await _client.GetAsync<GraphPackage>(packageNodeId, cancellationToken);
        if (package == null)
            return false;

        if (subscription.Plan.AccessLevel < package.RequiredTierLevel)
            return false;

        return true;
    }

    public async Task<IReadOnlyList<Package>> GetAccessiblePackagesAsync(
        string userId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var graphPackages = await _client.QueryAsync<GraphPackage>(skip, take, cancellationToken);
        var accessiblePackages = new List<Package>();

        foreach (var graphPackage in graphPackages)
        {
            if (await CanUserAccessPackageAsync(userId, graphPackage.Id, cancellationToken))
            {
                accessiblePackages.Add(PackageGraphMapper.FromGraph(graphPackage));
            }
        }

        return accessiblePackages;
    }

    public async Task<PackageTier?> GetPackageTierAsync(Guid packageNodeId, CancellationToken cancellationToken = default)
    {
        var package = await _client.GetAsync<GraphPackage>(packageNodeId, cancellationToken);
        if (package == null)
            return null;

        // Return a tier based on the package's RequiredTierLevel
        var tiers = await GetTiersAsync(cancellationToken);
        return tiers.FirstOrDefault(t => t.Level == package.RequiredTierLevel);
    }

    public async Task<bool> SetPackageTierAsync(Guid packageNodeId, Guid tierId, CancellationToken cancellationToken = default)
    {
        var package = await _client.GetAsync<GraphPackage>(packageNodeId, cancellationToken);
        if (package == null)
            return false;

        var tier = await _client.GetAsync<PackageTier>(tierId, cancellationToken);
        if (tier == null)
            return false;

        // Update package's RequiredTierLevel
        package.RequiredTierLevel = tier.Level;
        await _client.UpdateAsync(package, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<PackageTier>> GetTiersAsync(CancellationToken cancellationToken = default)
    {
        var tiers = await _client.QueryAsync<PackageTier>(0, 100, cancellationToken);
        return tiers.OrderBy(t => t.Level).ToList();
    }

    public async Task<PackageTier> CreateTierAsync(string name, int level, CancellationToken cancellationToken = default)
    {
        var tier = new PackageTier
        {
            Name = name,
            Level = level
        };

        return await _client.CreateAsync(tier, cancellationToken);
    }

    public async Task<int> GetUserAccessLevelAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId, cancellationToken);
        return subscription?.Plan?.AccessLevel ?? 0;
    }

    public async Task<bool> CanUserAccessTierAsync(string userId, int tierLevel, CancellationToken cancellationToken = default)
    {
        var userLevel = await GetUserAccessLevelAsync(userId, cancellationToken);
        return userLevel >= tierLevel;
    }
}
