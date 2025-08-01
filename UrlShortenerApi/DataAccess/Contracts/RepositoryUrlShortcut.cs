using System.Text.Json.Serialization;

namespace UrlShortenerApi.DataAccess.Contracts
{
    public class RepositoryUrlShortcut
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("partitionKey")]
        public required string PartitionKey { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
