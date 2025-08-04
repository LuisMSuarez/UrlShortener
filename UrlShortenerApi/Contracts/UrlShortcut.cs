namespace UrlShortenerApi.Contracts
{
    using System.Text.Json.Serialization;

    public class UrlShortcut
    {
        [JsonPropertyName("shortcut")]
        public required string Shortcut { get; set; }
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
