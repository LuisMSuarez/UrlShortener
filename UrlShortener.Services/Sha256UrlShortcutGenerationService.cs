namespace UrlShortenerApi.Services
{
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Text;
    using UrlShortenerApi.Contracts;

    public class Sha256UrlShortcutGenerationService : IUrlShortcutGenerationService
    {
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const int MaxShortcutLength = 6;

        public string GenerateUrlShortcutId(UrlShortcut urlShortcut)
        {
            // A shared SHA256 instance is not thread safe. Better to create one per request
            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(urlShortcut.Url);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert to hex string
            var sb = new StringBuilder(hashBytes.Length * 2);
            return ToBase62(hashBytes);
        }

        private static string ToBase62(byte[] bytes)
        {
            // Convert byte array to a BigInteger (positive)
            // Appending a 0 byte at the end ensures the number is always interpreted as positive, since BigInteger treats the highest bit as the sign bit.
            var value = new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
            var sb = new StringBuilder();

            // Base62 encoding
            while (value > 0)
            {
                value = BigInteger.DivRem(value, 62, out var remainder);
                sb.Insert(0, Base62Chars[(int)remainder]);
            }

            return sb.ToString().Substring(0, Math.Min(sb.Length, MaxShortcutLength));
        }
    }
}