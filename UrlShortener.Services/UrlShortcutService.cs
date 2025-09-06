using Microsoft.Extensions.Logging;
using UrlShortenerApi.DataAccess;
using UrlShortenerApi.DataAccess.Contracts;
using UrlShortenerApi.Services.Contracts;

namespace UrlShortenerApi.Services;

/// <summary>
/// Provides operations for creating and retrieving URL shortcuts.
/// Handles conflict resolution and retry logic during shortcut creation.
/// </summary>
public class UrlShortcutService : IUrlShortcutService
{
    private readonly IUrlShortcutRepository urlShortcutRepository;
    private readonly IUrlShortcutGenerationService urlShortcutGenerationService;
    private readonly ILogger<UrlShortcutService> logger;
    private const int MaxRetriesConflictResolution = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlShortcutService"/> class.
    /// </summary>
    /// <param name="urlShortcutRepository">Repository for shortcut persistence.</param>
    /// <param name="urlShortcutGenerationService">Service for generating shortcut identifiers.</param>
    /// <param name="logger">Logger for diagnostics and warnings.</param>
    public UrlShortcutService(
        IUrlShortcutRepository urlShortcutRepository,
        IUrlShortcutGenerationService urlShortcutGenerationService,
        ILogger<UrlShortcutService> logger
        )
    {
        this.urlShortcutRepository = urlShortcutRepository ?? throw new ArgumentNullException(nameof(urlShortcutRepository));
        this.urlShortcutGenerationService = urlShortcutGenerationService ?? throw new ArgumentNullException(nameof(urlShortcutGenerationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new shortcut for the specified URL.
    /// Applies retry logic to resolve potential ID conflicts.
    /// </summary>
    /// <param name="shortcut">The shortcut request containing the original URL.</param>
    /// <returns>The created <see cref="UrlShortcut"/>.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid or creation fails.</exception>
    public async Task<UrlShortcut> CreateUrlShortcutAsync(UrlShortcut shortcut)
    {
        if (shortcut == null || string.IsNullOrWhiteSpace(shortcut.Url))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
        }

        return await this.RetriableShortcutCreation(MaxRetriesConflictResolution, string.Empty, shortcut);
    }

    /// <summary>
    /// Retrieves a shortcut by its identifier.
    /// </summary>
    /// <param name="shortcut">The shortcut ID.</param>
    /// <returns>The corresponding <see cref="UrlShortcut"/>, or null if not found.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid or retrieval fails.</exception>
    public async Task<UrlShortcut?> GetUrlShortcutAsync(string shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Shortcut cannot be null or empty.");
        }

        try
        {
            var repositoryShortcut = await this.urlShortcutRepository.GetShortcutAsync(shortcut);
            if (repositoryShortcut == null)
            {
                return null;
            }

            return ToServiceUrlShortcut(repositoryShortcut);
        }
        catch (Exception ex)
        {
            throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while fetching the URL shortcut {shortcut}.", ex);
        }
    }

    /// <summary>
    /// Retrieves all shortcuts associated with a given URL.
    /// </summary>
    /// <param name="url">The original URL.</param>
    /// <returns>A collection of <see cref="UrlShortcut"/> instances.</returns>
    /// <exception cref="ServiceException">Thrown when input is invalid or retrieval fails.</exception>
    public async Task<IEnumerable<UrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ServiceException(ServiceResultCode.BadRequest, "Url cannot be null or empty.");
        }

        var normalizedUrl = url.ToLowerInvariant();
        try
        {
            var shortcuts = await this.urlShortcutRepository.GetUrlShortcutsByUrlAsync(normalizedUrl);
            if (shortcuts == null)
            {
                return [];
            }

            return shortcuts.Select(repoShortcut => ToServiceUrlShortcut(repoShortcut));
        }
        catch (Exception ex)
        {
            throw new ServiceException(ServiceResultCode.InternalServerError, $"An unexpected error occurred while fetching the URL shortcut by Url {url}.", ex);
        }
    }

    /// <summary>
    /// Converts a repository-layer shortcut to a service-layer shortcut.
    /// </summary>
    /// <param name="repositoryUrlShortcut">The repository model.</param>
    /// <returns>The converted <see cref="UrlShortcut"/>.</returns>
    private static UrlShortcut ToServiceUrlShortcut(RepositoryUrlShortcut repositoryUrlShortcut)
    {
        return new UrlShortcut
        {
            Url = repositoryUrlShortcut.Url,
            Shortcut = repositoryUrlShortcut.Id
        };
    }

    /// <summary>
    /// Attempts to create a shortcut with retry logic for ID collisions.
    /// </summary>
    /// <param name="retriesLeft">Number of retries remaining.</param>
    /// <param name="previousHash">Hash from previous attempt used for salting.</param>
    /// <param name="shortcut">The original shortcut request.</param>
    /// <returns>A successfully created <see cref="UrlShortcut"/>.</returns>
    /// <exception cref="ServiceException">Thrown when conflict cannot be resolved or an error occurs.</exception>
    private async Task<UrlShortcut> RetriableShortcutCreation(int retriesLeft, string previousHash, UrlShortcut shortcut)
    {
        if (retriesLeft == 0)
        {
            throw new ServiceException(ServiceResultCode.Conflict, $"Could not resolve conflict in creation of shortcut with URL {shortcut.Url}.");
        }

        // Salt the URL with part of the previous hash to reduce collision probability
        var salt = retriesLeft == MaxRetriesConflictResolution ?
            string.Empty :
            previousHash.Substring(0, Math.Min(3, previousHash.Length));

        var newShortcutId = this.urlShortcutGenerationService.GenerateUrlShortcutId(
            new UrlShortcut
            {
                Url = string.Concat(salt, shortcut.Url)
            });

        try
        {
            var createdShortcut = await this.urlShortcutRepository.CreateShortcutAsync(
                new RepositoryUrlShortcut
                {
                    Id = newShortcutId,
                    Url = shortcut.Url
                });

            return ToServiceUrlShortcut(createdShortcut);
        }
        catch (DataAccessException ex) when (ex.ResultCode == DataAccessResultCode.Conflict)
        {
            logger.Log(LogLevel.Warning, $"RetryShortcutCreationConflict: A shortcut with the same key {newShortcutId} already exists. Retries left {retriesLeft}");

            // TODO: handle case where GetShortcutAsync throws not found exception
            var conflictingShortcut = await this.urlShortcutRepository.GetShortcutAsync(newShortcutId);
            if (conflictingShortcut != null && string.Equals(conflictingShortcut.Url, shortcut.Url, StringComparison.OrdinalIgnoreCase))
            {
                return ToServiceUrlShortcut(conflictingShortcut);
            }

            return await RetriableShortcutCreation(retriesLeft - 1, newShortcutId, shortcut);
        }
        catch (Exception ex)
        {
            throw new ServiceException(ServiceResultCode.InternalServerError, $"RetryShortcutCreationConflict: An unexpected error occurred while creating the URL shortcut {shortcut}.", ex);
        }
    }
}
