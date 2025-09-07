namespace UrlShortenerApi.Services;

using UrlShortenerApi.Services.Contracts;

/// <summary>
/// Defines a service for generating unique shortcut identifiers for URLs.
/// </summary>
public interface IUrlShortcutGenerationService
{
    /// <summary>
    /// Generates a shortcut ID for the specified URL.
    /// </summary>
    /// <param name="urlShortcut">The URL shortcut object containing the full URL and optional existing shortcut.</param>
    /// <returns>A unique string identifier to be used as the shortcut.</returns>
    string GenerateUrlShortcutId(UrlShortcut urlShortcut);
}
