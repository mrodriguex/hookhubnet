using HookHubNet.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HookHubNet.Hub.Controllers;

[ApiController]
[Route("hook")]
public class HookController : ControllerBase
{
    private readonly TunnelRegistry _registry;
    public HookController(TunnelRegistry registry)
    {
        _registry = registry;
    }

    [HttpGet("getall")]
    public IActionResult GetAllHooks()
    {
        var hooks = _registry.Hooks.Select(kvp => new { hookId = kvp.Key, port = kvp.Value.Port }).ToList();
        return Ok(hooks);
    }

    [HttpGet("get/{hookId}")]
    public IActionResult GetHookInfo(string hookId)
    {
        if (_registry.Hooks.TryGetValue(hookId, out var hookInfo))
        {
            return Ok(new { hookId, port = hookInfo.Port });
        }
        return NotFound(new { message = $"Hook '{hookId}' not found." });
    }

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