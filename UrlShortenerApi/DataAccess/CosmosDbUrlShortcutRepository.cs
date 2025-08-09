namespace UrlShortenerApi.DataAccess
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using System.Net;
    using UrlShortenerApi.DataAccess.Contracts;

    public class CosmosDbUrlShortcutRepository : IUrlShortcutRepository
    {
        private readonly CosmosClient client;
        private readonly Configuration configuration;
        private readonly Container container;

        public CosmosDbUrlShortcutRepository(
            CosmosClient client,
            IOptions<Configuration> configurationOptions)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.configuration = configurationOptions?.Value ?? throw new ArgumentNullException(nameof(configurationOptions));
            var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
            this.container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);

        }

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
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut.", ex);
            }
        }

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

        public async Task<IEnumerable<RepositoryUrlShortcut>> GetUrlShortcutsByUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new DataAccessException(DataAccessResultCode.BadRequest, "Url is null or empty.");
            }

            // Note: Below is a cross-partition query that can be optimized
            // by creating a separate collection for the reverse lookup where url is the partition key
            // for now this is mitigated by indexing the url field.
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

        private static RepositoryUrlShortcut ToRepositoryUrlShortcut(CosmosDbUrlShortcut cosmosDbUrlShortcut)
        {
            return new RepositoryUrlShortcut
            {
                Id = cosmosDbUrlShortcut.Id,
                Url = cosmosDbUrlShortcut.Url
            };
        }
    }
}