namespace UrlShortenerApi.Contracts
{
    public class UrlShortcut
    {
        public required string Shortcut { get; set; }
        public required string Url { get; set; }
    }
}
