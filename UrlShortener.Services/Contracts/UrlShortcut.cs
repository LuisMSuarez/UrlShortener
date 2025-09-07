namespace UrlShortenerApi.Services.Contracts;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a mapping between a shortcut identifier and a full URL.
/// Used for serializing and deserializing URL shortening data.
/// </summary>
public class UrlShortcut
{
    /// <summary>
    /// Gets or sets the shortcut identifier for the URL.
    /// This value is optional and may be null.
    /// </summary>
    [JsonPropertyName("shortcut")]
    public string? Shortcut { get; set; }

    /// <summary>
    /// Gets or sets the full URL that the shortcut resolves to.
    /// This value is required.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
