using HookHubNet.Common.Models;
using Microsoft.Extensions.Logging;

namespace HookHubNet.Hook.Services;

/// <summary>
/// Handles the startup and execution of the HookHub Hook application, managing multiple hook clients.
/// </summary>
public class Startup
{
    private readonly ILogger<HookHubClient> _logger;
    private readonly string _hubUrl;
    private readonly List<HookConfig> _hooks;

    /// <summary>
    /// Initializes a new instance of the Startup class.
    /// </summary>
    /// <param name="logger">The logger for HookHubClient.</param>
    /// <param name="hubUrl">The URL of the HookHub hub.</param>
    /// <param name="hooks">The list of hook configurations.</param>
    public Startup(ILogger<HookHubClient> logger, string hubUrl, List<HookConfig> hooks)
    {
        _logger = logger;
        _hubUrl = hubUrl;
        _hooks = hooks;
    }

    /// <summary>
    /// Runs the application, starting all hook clients and handling shutdown.
    /// </summary>
    /// <returns>The exit code (0 for success, 1 for error).</returns>
    public async Task<int> RunAsync()
    {
        // Log startup information
        _logger.LogInformation("HookHub Client Starting...");
        _logger.LogInformation("Hub URL: {HubUrl}", _hubUrl);

        // Set up cancellation token for graceful shutdown on Ctrl+C
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            _logger.LogInformation("Shutting down...");
        };

        // Instantiate and run HookHubClients for each configured hook
        var tasks = _hooks.Select(hook => {
            var client = new HookHubClient(_logger, _hubUrl, hook.HookId, hook.TargetHost, hook.TargetPort);
            return client.RunAsync(cts.Token);
        }).ToList();

        try
        {
            // Wait for all clients to complete
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            // Log any fatal errors and exit with code 1
            _logger.LogError(ex, "Fatal error occurred");
            return 1;
        }

        // Log that the clients have stopped and exit successfully
        _logger.LogInformation("Clients stopped");
        return 0;
    }
}