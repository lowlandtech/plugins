using System.Security.Cryptography;
using BaGet.Core.Models;
using BaGet.Core.Models.Graph;
using LowlandTech.Graph.Abstractions.Client;

namespace BaGet.Core.Services;

/// <summary>
/// License service using GraphClient for data access.
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly IGraphClient _client;

    public LicenseService(IGraphClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<PackageLicense>> GetUserLicensesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var licenses = await _client.QueryAsync<PackageLicense>(0, 1000, cancellationToken);
        var userLicenses = licenses.Where(l => l.UserId == userId).ToList();

        // Load related packages
        foreach (var license in userLicenses)
        {
            var packages = await _client.GetRelatedAsync<PackageLicense, GraphPackage>(license.Id, "licenses", cancellationToken);
            license.Package = packages.FirstOrDefault();
        }

        return userLicenses;
    }

    public async Task<PackageLicense?> GetLicenseAsync(Guid licenseId, CancellationToken cancellationToken = default)
    {
        var license = await _client.GetAsync<PackageLicense>(licenseId, cancellationToken);
        if (license != null)
        {
            var packages = await _client.GetRelatedAsync<PackageLicense, GraphPackage>(licenseId, "licenses", cancellationToken);
            license.Package = packages.FirstOrDefault();
        }
        return license;
    }

    public async Task<PackageLicense> IssueLicenseAsync(
        string userId,
        Guid packageNodeId,
        LicenseType type,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var package = await _client.GetAsync<GraphPackage>(packageNodeId, cancellationToken);
        if (package == null)
            throw new ArgumentException($"Package {packageNodeId} not found", nameof(packageNodeId));

        var license = new PackageLicense
        {
            UserId = userId,
            PackageNodeId = packageNodeId,
            LicenseKey = GenerateLicenseKey(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            Type = type
        };

        var created = await _client.CreateAsync(license, cancellationToken);

        // Create edge to package
        await _client.CreateEdgeAsync<PackageLicense, GraphPackage>(created.Id, packageNodeId, "licenses", cancellationToken);

        created.Package = package;
        return created;
    }

    public async Task<bool> ValidateLicenseAsync(string userId, Guid packageNodeId, CancellationToken cancellationToken = default)
    {
        var licenses = await GetUserLicensesAsync(userId, cancellationToken);
        var license = licenses.FirstOrDefault(l =>
            l.PackageNodeId == packageNodeId &&
            (l.ExpiresAt == null || l.ExpiresAt > DateTime.UtcNow));

        return license != null;
    }

    public async Task<PackageLicense?> ValidateLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        var licenses = await _client.QueryAsync<PackageLicense>(0, 10000, cancellationToken);
        var license = licenses.FirstOrDefault(l =>
            l.LicenseKey == licenseKey &&
            (l.ExpiresAt == null || l.ExpiresAt > DateTime.UtcNow));

        if (license != null)
        {
            var packages = await _client.GetRelatedAsync<PackageLicense, GraphPackage>(license.Id, "licenses", cancellationToken);
            license.Package = packages.FirstOrDefault();
        }

        return license;
    }

    public async Task<bool> RevokeLicenseAsync(Guid licenseId, CancellationToken cancellationToken = default)
    {
        return await _client.DeleteAsync<PackageLicense>(licenseId, cancellationToken);
    }

    public async Task<PackageLicense?> ExtendLicenseAsync(Guid licenseId, DateTime newExpiresAt, CancellationToken cancellationToken = default)
    {
        var license = await _client.GetAsync<PackageLicense>(licenseId, cancellationToken);
        if (license == null)
            return null;

        license.ExpiresAt = newExpiresAt;
        return await _client.UpdateAsync(license, cancellationToken);
    }

    public async Task<IReadOnlyList<PackageLicense>> GetPackageLicensesAsync(Guid packageNodeId, CancellationToken cancellationToken = default)
    {
        var licenses = await _client.QueryAsync<PackageLicense>(0, 10000, cancellationToken);
        return licenses.Where(l => l.PackageNodeId == packageNodeId).ToList();
    }

    private static string GenerateLicenseKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
