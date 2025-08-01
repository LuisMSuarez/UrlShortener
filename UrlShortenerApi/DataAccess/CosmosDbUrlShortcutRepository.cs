using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using UrlShortenerApi.Contracts;

namespace UrlShortenerApi.DataAccess
{
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

        public async Task<UrlShortcut> GetUrlShortcutAsync(string shortcut)
        {
            Database database = client.GetDatabase(configuration.AzureCosmosDB.DatabaseName);

            database = await database.ReadAsync();
            Console.WriteLine($"Database Id: {database.Id}");
            Container container = database.GetContainer(configuration.AzureCosmosDB.ContainerName);
            var response = await container.ReadItemAsync<UrlShortcut>("967157c1-53c1-4ca1-8d6c-eab8722f2a7e", new PartitionKey(shortcut));
            return response.Resource;
        }
    }
}
