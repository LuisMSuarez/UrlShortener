using UrlShortenerApi.Services.Contracts;

namespace UrlShortenerApi.Services;

/// <summary>
/// Defines operations for managing URL shortcuts, including creation and retrieval.
/// </summary>
public interface IUrlShortcutService
{
    /// <summary>
    /// Retrieves a shortcut by its identifier.
    /// </summary>
    /// <param name="shortcut">The shortcut ID to look up.</param>
    /// <returns>The matching <see cref="UrlShortcut"/>, or null if not found.</returns>
    Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut);

    /// <summary>
    /// Creates a new shortcut for the specified URL.
    /// </summary>
    /// <param name="shortcut">The shortcut request containing the original URL.</param>
    /// <returns>The created <see cref="UrlShortcut"/>.</returns>
    Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut);

    /// <summary>
    /// Retrieves all shortcuts associated with a given URL.
    /// </summary>
    /// <param name="url">The original URL to search for.</param>
    /// <returns>A collection of matching <see cref="UrlShortcut"/> instances.</returns>
    Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
}
