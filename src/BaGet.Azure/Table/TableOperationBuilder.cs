using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Azure;
using BaGet.Core;
using NuGet.Versioning;

namespace BaGet.Azure
{
    public class TableOperationBuilder
    {
        public PackageEntity BuildPackageEntity(Package package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            var version = package.Version;
            var normalizedVersion = version.ToNormalizedString();

            return new PackageEntity
            {
                PartitionKey = package.Id.ToLowerInvariant(),
                RowKey = normalizedVersion.ToLowerInvariant(),

                Id = package.Id,
                NormalizedVersion = normalizedVersion,
                OriginalVersion = version.ToFullString(),
                Authors = JsonSerializer.Serialize(package.Authors),
                Description = package.Description,
                Downloads = package.Downloads,
                HasReadme = package.HasReadme,
                HasEmbeddedIcon = package.HasEmbeddedIcon,
                IsPrerelease = package.IsPrerelease,
                Language = package.Language,
                Listed = package.Listed,
                MinClientVersion = package.MinClientVersion,
                Published = package.Published,
                RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                SemVerLevel = (int)package.SemVerLevel,
                Summary = package.Summary,
                Title = package.Title,
                IconUrl = package.IconUrlString,
                LicenseUrl = package.LicenseUrlString,
                ReleaseNotes = package.ReleaseNotes,
                ProjectUrl = package.ProjectUrlString,
                RepositoryUrl = package.RepositoryUrlString,
                RepositoryType = package.RepositoryType,
                Tags = JsonSerializer.Serialize(package.Tags),
                Dependencies = SerializeList(package.Dependencies, AsDependencyModel),
                PackageTypes = SerializeList(package.PackageTypes, AsPackageTypeModel),
                TargetFrameworks = SerializeList(package.TargetFrameworks, f => f.Moniker)
            };
        }

        public PackageDownloadsEntity BuildDownloadsEntity(string packageId, NuGetVersion packageVersion, long downloads)
        {
            return new PackageDownloadsEntity
            {
                PartitionKey = packageId.ToLowerInvariant(),
                RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
                Downloads = downloads,
                ETag = ETag.All
            };
        }

        public (string partitionKey, string rowKey) GetEntityKeys(string packageId, NuGetVersion packageVersion)
        {
            return (packageId.ToLowerInvariant(), packageVersion.ToNormalizedString().ToLowerInvariant());
        }

        public PackageListingEntity BuildListingEntity(string packageId, NuGetVersion packageVersion, bool listed)
        {
            return new PackageListingEntity
            {
                PartitionKey = packageId.ToLowerInvariant(),
                RowKey = packageVersion.ToNormalizedString().ToLowerInvariant(),
                Listed = listed,
                ETag = ETag.All
            };
        }

        private static string SerializeList<TIn, TOut>(IReadOnlyList<TIn> objects, Func<TIn, TOut> map)
        {
            var data = objects.Select(map).ToList();
            return JsonSerializer.Serialize(data);
        }

        public static DependencyModel AsDependencyModel(PackageDependency dependency)
        {
            return new DependencyModel
            {
                Id = dependency.Id,
                VersionRange = dependency.VersionRange,
                TargetFramework = dependency.TargetFramework
            };
        }

        public static PackageTypeModel AsPackageTypeModel(PackageType packageType)
        {
            return new PackageTypeModel
            {
                Name = packageType.Name,
                Version = packageType.Version
            };
        }
    }
}
