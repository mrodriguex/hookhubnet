using HookHubNet.Common.Models;
using Microsoft.Extensions.Configuration;

namespace HookHubNet.Hook.Services;

/// <summary>
/// Service for loading and providing configuration settings for the HookHub Hook application.
/// </summary>
public class ConfigService
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the ConfigService, building the configuration from JSON files and environment variables.
    /// </summary>
    public ConfigService()
    {
        // Determine the current environment (defaults to Production if not set)
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "Production";

        // Build the configuration from JSON files and environment variables
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Gets the URL of the HookHub hub from configuration.
    /// </summary>
    /// <returns>The hub URL, defaulting to "ws://localhost:5201/hook" if not configured.</returns>
    public string GetHubUrl() => _config["HubUrl"] ?? "ws://localhost:5201/hookhubnet";

    /// <summary>
    /// Gets the list of hook configurations from the configuration.
    /// </summary>
    /// <returns>A list of HookConfig objects.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no hooks are configured.</exception>
    public List<HookConfig> GetHooks()
    {
        var hooks = _config.GetSection("Hooks").Get<List<HookConfig>>();
        if (hooks == null || !hooks.Any())
        {
            throw new InvalidOperationException("No hooks configured in appsettings.json");
        }
        return hooks;
    }
}