using HookHubNet.Common.Models;
using Microsoft.Extensions.Configuration;

namespace HookHubNet.Hook.Services;

public class ConfigService
{
    private readonly IConfiguration _config;

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

    public string GetHubUrl() => _config["HubUrl"] ?? "ws://localhost:5201/hook";

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