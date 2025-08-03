namespace UrlShortenerApi.Services
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using UrlShortenerApi.Contracts;

    public class Sha256UrlShortcutGenerationService : IUrlShortcutGenerationService
    {
        private bool disposedValue;

        public string GenerateUrlShortcut(UrlShortcut urlShortcut)
        {
            // A shared SHA256 instance is not thread - safe. Better to create one per request
            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(urlShortcut.Url);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert to hex string
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}