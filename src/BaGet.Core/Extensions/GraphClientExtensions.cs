using BaGet.Core.Services;
using LowlandTech.Graph.Abstractions.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaGet.Core.Extensions;

/// <summary>
/// Extension methods for registering GraphClient and related services.
/// </summary>
public static class GraphClientExtensions
{
    /// <summary>
    /// Configuration section name for GraphApi settings.
    /// </summary>
    public const string SectionName = "GraphApi";

    /// <summary>
    /// Adds GraphClient and subscription/licensing services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGraphServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var baseUrl = section["BaseUrl"] ?? "http://localhost:5000";
        var apiKey = section["ApiKey"];
        var timeoutSeconds = int.TryParse(section["TimeoutSeconds"], out var t) ? t : 30;

        return services.AddGraphServices(opts =>
        {
            opts.BaseUrl = baseUrl;
            opts.ApiKey = apiKey;
            opts.TimeoutSeconds = timeoutSeconds;
        });
    }

    /// <summary>
    /// Adds GraphClient and subscription/licensing services to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGraphServices(this IServiceCollection services, Action<GraphApiOptions> configureOptions)
    {
        var options = new GraphApiOptions();
        configureOptions(options);

        // Register GraphClient
        services.AddHttpClient<IGraphClient, GraphClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
            }
        });

        // Register subscription and licensing services
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IPackageAccessService, PackageAccessService>();

        return services;
    }
}

/// <summary>
/// Configuration options for the Graph API client.
/// </summary>
public class GraphApiOptions
{
    /// <summary>
    /// The base URL of the Graph API.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// Optional API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Timeout in seconds for HTTP requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
