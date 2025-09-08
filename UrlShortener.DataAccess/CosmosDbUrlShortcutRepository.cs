using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using UrlShortenerApi.DataAccess.Contracts;

namespace UrlShortenerApi.DataAccess;

/// <summary>
/// Implements <see cref="IUrlShortcutRepository"/> using Azure Cosmos DB as the backing store.
/// </summary>
public class CosmosDbUrlShortcutRepository : IUrlShortcutRepository
{
    private readonly CosmosClient client;
    private readonly Configuration configuration;
    private readonly Container container;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDbUrlShortcutRepository"/> class.
    /// </summary>
    /// <param name="client">The Cosmos DB client instance.</param>
    /// <param name="configurationOptions">Configuration options containing database and container names.</param>
    public CosmosDbUrlShortcutRepository(
        CosmosClient client,
        IOptions<Configuration> configurationOptions)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.configuration = configurationOptions?.Value ?? throw new ArgumentNullException(nameof(configurationOptions));
        var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
        this.container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
    }

    /// <summary>
    /// Creates a new shortcut document in Cosmos DB.
    /// </summary>
    /// <param name="shortcut">The shortcut entity to persist.</param>
    /// <returns>The created <see cref="RepositoryUrlShortcut"/>.</returns>
    /// <exception cref="DataAccessException">
    /// Thrown when the input is invalid, a conflict occurs, or an unexpected error is encountered.
    /// </exception>
    public async Task<RepositoryUrlShortcut> CreateShortcutAsync(RepositoryUrlShortcut shortcut)
    {
        if (shortcut == null || string.IsNullOrWhiteSpace(shortcut.Url))
        {
            throw new DataAccessException(DataAccessResultCode.BadRequest, "Shortcut is null or contains empty url.");
        }

        try
        {
            var response = await this.container.CreateItemAsync<CosmosDbUrlShortcut>(
                new CosmosDbUrlShortcut
                {
                    Id = shortcut.Id,
                    Url = shortcut.Url,
                    PartitionKey = shortcut.Id
                },
                partitionKey: new PartitionKey(shortcut.Id));

            return ToRepositoryUrlShortcut(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new DataAccessException(DataAccessResultCode.Conflict, $"Shortcut with id {shortcut} already exists", ex);
        }
        catch (Exception ex)
        {
            throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error creating url shortcut.", ex);
        }
    }

    /// <summary>
    /// Retrieves a shortcut document by its unique identifier.
    /// </summary>
    /// <param name="shortcut">The shortcut ID.</param>
    /// <returns>The matching <see cref="RepositoryUrlShortcut"/>, or <c>null</c> if not found.</returns>
    /// <exception cref="DataAccessException">
    /// Thrown when the input is invalid or an unexpected error occurs during retrieval.
    /// </exception>
    public async Task<RepositoryUrlShortcut?> GetShortcutAsync(string shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut))
        {
            throw new DataAccessException(DataAccessResultCode.BadRequest, "Shortcut is null or empty url.");
        }

        try
        {
            var response = await this.container.ReadItemAsync<CosmosDbUrlShortcut>(shortcut, new PartitionKey(shortcut));
            return ToRepositoryUrlShortcut(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut.", ex);
        }
    }

    /// <summary>
    /// Retrieves all shortcut documents associated with a specific original URL.
    /// </summary>
    /// <param name="url">The original URL to search for.</param>
    /// <returns>A collection of <see cref="RepositoryUrlShortcut"/> instances.</returns>
    /// <exception cref="DataAccessException">
    /// Thrown when the input is invalid or an unexpected error occurs during query execution.
    /// </exception>
    /// <remarks>
    /// This method performs a cross-partition query. For optimal performance, consider restructuring
    /// the data model to use the URL as a partition key or maintain a reverse lookup collection.
    /// </remarks>
    public async Task<IEnumerable<RepositoryUrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new DataAccessException(DataAccessResultCode.BadRequest, "Url is null or empty.");
        }

        var query = new QueryDefinition("SELECT * FROM c WHERE c.url = @url")
            .WithParameter("@url", url);

        var results = new List<RepositoryUrlShortcut>();
        using var iterator = this.container.GetItemQueryIterator<CosmosDbUrlShortcut>(query);
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
            {
                results.Add(ToRepositoryUrlShortcut(item));
            }
        }
        return results;
    }

    /// <summary>
    /// Converts a Cosmos DB entity to a repository-layer shortcut model.
    /// </summary>
    /// <param name="cosmosDbUrlShortcut">The Cosmos DB entity.</param>
    /// <returns>The mapped <see cref="RepositoryUrlShortcut"/>.</returns>
    private static RepositoryUrlShortcut ToRepositoryUrlShortcut(CosmosDbUrlShortcut cosmosDbUrlShortcut)
    {
        return new RepositoryUrlShortcut
        {
            Id = cosmosDbUrlShortcut.Id,
            Url = cosmosDbUrlShortcut.Url
        };
    }
}
