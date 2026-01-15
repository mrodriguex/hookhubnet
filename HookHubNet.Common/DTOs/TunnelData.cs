namespace HookHubNet.Common.DTOs
{
    public class TunnelData
    {
        public string TunnelId { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public bool Close { get; set; } = false;
    }
}