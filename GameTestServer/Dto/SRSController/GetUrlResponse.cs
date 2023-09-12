using System.Text.Json.Serialization;

namespace Wanin_Test.Dto.SRSController
{
    public class GetUrlResponse
    {
        [JsonPropertyName("pullerid")]
        public string? PullerId { get; set; }

        [JsonPropertyName("publisherid")]
        public string? PublisherId { get; set; }

        [JsonPropertyName("rtmpurl")]
        public string? RTMPUrl { get; set; }

        [JsonPropertyName("rtcurl")]
        public string? RTCUrl { get; set; }

        [JsonPropertyName("publisherneedtopush")]
        public bool PublisherNeedToPush { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

}
