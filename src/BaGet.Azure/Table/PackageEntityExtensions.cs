using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BaGet.Core;

namespace BaGet.Azure
{
    public static class PackageEntityExtensions
    {
        public static Package AsPackage(this PackageEntity entity)
        {
            return new Package
            {
                Id = entity.Id ?? string.Empty,
                NormalizedVersionString = entity.NormalizedVersion,
                OriginalVersionString = entity.OriginalVersion,

                Authors = Deserialize<string[]>(entity.Authors) ?? Array.Empty<string>(),
                Description = entity.Description,
                Downloads = entity.Downloads,
                HasReadme = entity.HasReadme,
                HasEmbeddedIcon = entity.HasEmbeddedIcon,
                IsPrerelease = entity.IsPrerelease,
                Language = entity.Language,
                Listed = entity.Listed,
                MinClientVersion = entity.MinClientVersion,
                Published = entity.Published,
                RequireLicenseAcceptance = entity.RequireLicenseAcceptance,
                SemVerLevel = (SemVerLevel)entity.SemVerLevel,
                Summary = entity.Summary,
                Title = entity.Title,
                ReleaseNotes = entity.ReleaseNotes,
                IconUrl = ParseUri(entity.IconUrl),
                LicenseUrl = ParseUri(entity.LicenseUrl),
                ProjectUrl = ParseUri(entity.ProjectUrl),
                RepositoryUrl = ParseUri(entity.RepositoryUrl),
                RepositoryType = entity.RepositoryType,
                Tags = Deserialize<string[]>(entity.Tags) ?? Array.Empty<string>(),
                Dependencies = ParseDependencies(entity.Dependencies),
                PackageTypes = ParsePackageTypes(entity.PackageTypes),
                TargetFrameworks = ParseTargetFrameworks(entity.TargetFrameworks),
            };
        }

        private static T? Deserialize<T>(string? input) where T : class
        {
            if (string.IsNullOrEmpty(input))
                return null;

            return JsonSerializer.Deserialize<T>(input);
        }

        private static Uri? ParseUri(string? input)
        {
            return string.IsNullOrEmpty(input) ? null : new Uri(input);
        }

        private static List<PackageDependency> ParseDependencies(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<PackageDependency>();

            var models = JsonSerializer.Deserialize<List<DependencyModel>>(input);
            if (models == null)
                return new List<PackageDependency>();

            return models
                .Select(e => new PackageDependency
                {
                    Id = e.Id,
                    VersionRange = e.VersionRange,
                    TargetFramework = e.TargetFramework,
                })
                .ToList();
        }

        private static List<PackageType> ParsePackageTypes(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<PackageType>();

            var models = JsonSerializer.Deserialize<List<PackageTypeModel>>(input);
            if (models == null)
                return new List<PackageType>();

            return models
                .Select(e => new PackageType
                {
                    Name = e.Name,
                    Version = e.Version
                })
                .ToList();
        }

        private static List<TargetFramework> ParseTargetFrameworks(string? targetFrameworks)
        {
            if (string.IsNullOrEmpty(targetFrameworks))
                return new List<TargetFramework>();

            var frameworks = JsonSerializer.Deserialize<List<string>>(targetFrameworks);
            if (frameworks == null)
                return new List<TargetFramework>();

            return frameworks
                .Select(f => new TargetFramework { Moniker = f })
                .ToList();
        }
    }
}
