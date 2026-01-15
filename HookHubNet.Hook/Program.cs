using HookHubNet.Hook.Services;
using Microsoft.Extensions.Logging;

// Configure logging with console output and minimum level Information
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

// Create a logger for the HookHubClient
var logger = loggerFactory.CreateLogger<HookHubClient>();

// Initialize configuration service
var configService = new ConfigService();

try
{
    // Get configuration values
    var hubUrl = configService.GetHubUrl();
    var hooks = configService.GetHooks();

    // Create and run the startup service
    var startup = new Startup(logger, hubUrl, hooks);
    return await startup.RunAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to start application");
    return -1;
}
