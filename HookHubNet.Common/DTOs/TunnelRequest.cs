namespace HookHubNet.Common.DTOs
{
    public class TunnelRequest
    {
        public string TunnelId { get; set; } = ""; // unique ID for this tunnel
        public string TargetHost { get; set; } = ""; // e.g., 127.0.0.1
        public int TargetPort { get; set; } // e.g., 5000
    }
}