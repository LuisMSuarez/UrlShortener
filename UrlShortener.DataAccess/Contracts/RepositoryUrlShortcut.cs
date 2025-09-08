namespace UrlShortenerApi.DataAccess.Contracts;

/// <summary>
/// Represents a URL shortcut entity used within the data access layer.
/// Encapsulates the mapping between a unique shortcut ID and its original URL.
/// </summary>
public class RepositoryUrlShortcut
{
    /// <summary>
    /// Gets or sets the unique identifier for the shortcut.
    /// This value serves as the primary key in the data store.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the original URL that the shortcut resolves to.
    /// </summary>
    public required string Url { get; set; }
}
