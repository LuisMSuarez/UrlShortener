namespace UrlShortenerApi.Services.Contracts
{
    using System.Text.Json.Serialization;

    public class UrlShortcut
    {
        [JsonPropertyName("shortcut")]
        public string? Shortcut { get; set; }
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
