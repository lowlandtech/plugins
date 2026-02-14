using BaGet.Core.Models;

namespace BaGet.Core;

/// <summary>
/// Service for managing package access control.
/// </summary>
public interface IPackageAccessService
{
    /// <summary>
    /// Checks if a user can access a specific package.
    /// Considers subscription tier, individual licenses, and package requirements.
    /// </summary>
    Task<bool> CanUserAccessPackageAsync(string userId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all packages accessible to a user based on their subscription and licenses.
    /// </summary>
    Task<IReadOnlyList<Package>> GetAccessiblePackagesAsync(
        string userId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the access tier required for a package.
    /// </summary>
    Task<PackageTier?> GetPackageTierAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the access tier requirement for a package.
    /// </summary>
    Task<bool> SetPackageTierAsync(Guid packageId, Guid tierId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available access tiers.
    /// </summary>
    Task<IReadOnlyList<PackageTier>> GetTiersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new access tier.
    /// </summary>
    Task<PackageTier> CreateTierAsync(string name, int level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's effective access level based on their subscription.
    /// </summary>
    Task<int> GetUserAccessLevelAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user can access packages at a specific tier level.
    /// </summary>
    Task<bool> CanUserAccessTierAsync(string userId, int tierLevel, CancellationToken cancellationToken = default);
}
