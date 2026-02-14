using BaGet.Core.Models;

namespace BaGet.Core;

/// <summary>
/// Service for managing package licenses.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Gets all licenses for a user.
    /// </summary>
    Task<IReadOnlyList<PackageLicense>> GetUserLicensesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a license by ID.
    /// </summary>
    Task<PackageLicense?> GetLicenseAsync(Guid licenseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a new license for a user to access a package.
    /// </summary>
    Task<PackageLicense> IssueLicenseAsync(
        string userId,
        Guid packageId,
        LicenseType type,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a user has a valid license for a package.
    /// </summary>
    Task<bool> ValidateLicenseAsync(string userId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a license key.
    /// </summary>
    Task<PackageLicense?> ValidateLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a license.
    /// </summary>
    Task<bool> RevokeLicenseAsync(Guid licenseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends a license expiration date.
    /// </summary>
    Task<PackageLicense?> ExtendLicenseAsync(Guid licenseId, DateTime newExpiresAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all licenses for a specific package.
    /// </summary>
    Task<IReadOnlyList<PackageLicense>> GetPackageLicensesAsync(Guid packageId, CancellationToken cancellationToken = default);
}
