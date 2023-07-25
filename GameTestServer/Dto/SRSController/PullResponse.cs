using System.Text.Json.Serialization;

namespace GameTestServer.Dto.SRSController
{
    public class PullResponse
    {
        public string? RTCUrl { get; set; }
        public string? RTMPUrl { get; set; }
        public string? SRSManagerConnectionUrl { get; set; }
        public string? Token { get; set; }
    }
}
