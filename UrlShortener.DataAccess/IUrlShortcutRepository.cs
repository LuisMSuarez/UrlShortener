using UrlShortenerApi.DataAccess.Contracts;

namespace UrlShortenerApi.DataAccess;

/// <summary>
/// Defines the contract for accessing and manipulating URL shortcut data in the repository.
/// </summary>
public interface IUrlShortcutRepository
{
    /// <summary>
    /// Retrieves a URL shortcut by its unique identifier.
    /// </summary>
    /// <param name="shortcutId">The unique ID of the shortcut to retrieve.</param>
    /// <returns>
    /// A task that resolves to the <see cref="RepositoryUrlShortcut"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<RepositoryUrlShortcut?> GetShortcutAsync(string shortcutId);

    /// <summary>
    /// Creates a new URL shortcut in the repository.
    /// </summary>
    /// <param name="shortcut">The shortcut entity to create.</param>
    /// <returns>
    /// A task that resolves to the created <see cref="RepositoryUrlShortcut"/> with any generated fields populated.
    /// </returns>
    Task<RepositoryUrlShortcut> CreateShortcutAsync(RepositoryUrlShortcut shortcut);

    /// <summary>
    /// Retrieves all shortcuts that match the specified original URL.
    /// </summary>
    /// <param name="url">The original URL to search for.</param>
    /// <returns>
    /// A task that resolves to a collection of <see cref="RepositoryUrlShortcut"/> instances associated with the given URL.
    /// </returns>
    Task<IEnumerable<RepositoryUrlShortcut>> GetUrlShortcutsByUrlAsync(string url);
}
