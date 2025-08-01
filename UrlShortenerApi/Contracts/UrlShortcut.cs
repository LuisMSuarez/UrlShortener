using System.Text.Json.Serialization;

namespace UrlShortenerApi.Contracts
{
    public class UrlShortcut
    {
        [JsonPropertyName("shortcut")]
        public required string Shortcut { get; set; }
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
