using System.Text.Json.Serialization;

namespace Wanin_Test.Dto.SRSController
{
    public class GetUrlResponse
    {
        [JsonPropertyName("userid")]
        public string? UserId { get; set; }
        [JsonPropertyName("pullerid")]
        public string? PullerId { get; set; }
        [JsonPropertyName("publisherid")]
        public string? PublisherId { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("pusherneedtopush")]
        public bool PusherNeedToPush { get; set; }
    }

}
