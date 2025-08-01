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
            return new UrlShortcut
            {
                Shortcut = shortcut,
                Url = $"https://{configuration.AzureCosmosDB.Endpoint}/{configuration.AzureCosmosDB.ContainerName}/{shortcut}"
            };
        }
    }
}
