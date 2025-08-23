namespace UrlShortenerApi.DataAccess
{
    using Newtonsoft.Json;

    // Important note: CosmosDb uses "Newtonsoft.Json" as its serialization library,
    // so we need to use the same attributes for serialization/deserialization.
    internal class CosmosDbUrlShortcut
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("partitionKey")]
        public required string PartitionKey { get; set; }

        [JsonProperty("url")]
        public required string Url { get; set; }
    }
}
