namespace HookHubNet.Common.Models;

/// <summary>
/// Represents the configuration for a hook, including its unique identifier and the target backend service details.
/// </summary>
/// <param name="HookId">The unique identifier for the hook.</param>
/// <param name="TargetHost">The hostname or IP address of the backend service to forward traffic to.</param>
/// <param name="TargetPort">The port number of the backend service.</param>
public record HookConfig(string HookId, string TargetHost, int TargetPort);