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

        public CosmosDbUrlShortcutRepository(
            CosmosClient client,
            IOptions<Configuration> configurationOptions)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.configuration = configurationOptions?.Value ?? throw new ArgumentNullException(nameof(configurationOptions));
        }

        public async Task<RepositoryUrlShortcut> CreateShortcutAsync(RepositoryUrlShortcut shortcut)
        {
            try
            {
                var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
                var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
                var response = await container.CreateItemAsync<CosmosDbUrlShortcut>(
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
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut", ex);
            }
        }

        public async Task<RepositoryUrlShortcut?> GetShortcutAsync(string shortcut)
        {
            try
            {
                var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
                var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
                var response = await container.ReadItemAsync<CosmosDbUrlShortcut>(shortcut, new PartitionKey(shortcut));
                return ToRepositoryUrlShortcut(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(DataAccessResultCode.InternalServerError, "Error fetching url shortcut", ex);
            }
        }

        public async Task<IEnumerable<RepositoryUrlShortcut>> GetUrlShortcutByUrlAsync(string url)
        {
            var database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);
            var container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.url = @url")
                .WithParameter("@url", url);

            var results = new List<RepositoryUrlShortcut>();
            using var iterator = container.GetItemQueryIterator<CosmosDbUrlShortcut>(query);
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