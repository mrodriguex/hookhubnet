using HookHubNet.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HookHubNet.Hub.Controllers;

/// <summary>
/// Controller for managing hooks, providing endpoints to query and remove hooks.
/// </summary>
[ApiController]
[Route("hook")]
public class HookController : ControllerBase
{
    private readonly TunnelRegistry _registry;

    /// <summary>
    /// Initializes a new instance of the HookController.
    /// </summary>
    /// <param name="registry">The tunnel registry for managing hooks.</param>
    public HookController(TunnelRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Gets a list of all connected hooks with their IDs and assigned ports.
    /// </summary>
    /// <returns>An IActionResult containing the list of hooks.</returns>
    [HttpGet("getall")]
    public IActionResult GetAllHooks()
    {
        var hooks = _registry.Hooks.Select(kvp => new { hookId = kvp.Key, port = kvp.Value.Port }).ToList();
        return Ok(hooks);
    }

    /// <summary>
    /// Gets information about a specific hook by its ID.
    /// </summary>
    /// <param name="hookId">The ID of the hook to retrieve.</param>
    /// <returns>An IActionResult with hook information or NotFound if the hook doesn't exist.</returns>
    [HttpGet("get/{hookId}")]
    public IActionResult GetHookInfo(string hookId)
    {
        if (_registry.Hooks.TryGetValue(hookId, out var hookInfo))
        {
            return Ok(new { hookId, port = hookInfo.Port });
        }
        return NotFound(new { message = $"Hook '{hookId}' not found." });
    }

    /// <summary>
    /// Removes a hook by its ID, stopping its listener and cancelling its operations.
    /// </summary>
    /// <param name="hookId">The ID of the hook to remove.</param>
    /// <returns>An IActionResult indicating success or NotFound if the hook doesn't exist.</returns>
    [HttpGet("remove/{hookId}")]
    public IActionResult RemoveHook(string hookId)
    {
        if (_registry.Hooks.TryRemove(hookId, out var hookInfo))
        {
            if (hookInfo is not null)
            {
                hookInfo.CancellationTokenSource?.Cancel();
                hookInfo.Listener?.Stop();
            }
            return Ok(new { message = $"Hook '{hookId}' removed." });
        }
        return NotFound(new { message = $"Hook '{hookId}' not found." });
    }
}