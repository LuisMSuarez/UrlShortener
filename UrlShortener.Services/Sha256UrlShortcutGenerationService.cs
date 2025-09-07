using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UrlShortenerApi.Services.Contracts;

namespace UrlShortenerApi.Services
{
    /// <summary>
    /// Generates a short URL identifier using SHA256 hashing and Base62 encoding.
    /// </summary>
    public class Sha256UrlShortcutGenerationService : IUrlShortcutGenerationService
    {
        /// <summary>
        /// Character set used for Base62 encoding.
        /// </summary>
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Maximum length of the generated shortcut identifier.
        /// </summary>
        private const int MaxShortcutLength = 6;

        /// <summary>
        /// Generates a short, unique identifier for the given URL using SHA256 and Base62.
        /// </summary>
        /// <param name="urlShortcut">The URL shortcut object containing the original URL.</param>
        /// <returns>A Base62-encoded string of fixed length representing the shortcut.</returns>
        public string GenerateUrlShortcutId(UrlShortcut urlShortcut)
        {
            // SHA256 is not thread-safe; instantiate per request
            using SHA256 sha256 = SHA256.Create();

            // Convert URL string to byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(urlShortcut.Url);

            // Compute SHA256 hash
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert hash to Base62 string
            return ToBase62(hashBytes);
        }

        /// <summary>
        /// Converts a byte array to a Base62-encoded string and truncates it to the maximum shortcut length.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <returns>A Base62-encoded string representing the input bytes.</returns>
        private static string ToBase62(byte[] bytes)
        {
            // Ensure positive BigInteger by appending a zero byte
            // Appending a 0 byte at the end ensures the number is always interpreted as positive, since BigInteger treats the highest bit as the sign bit.
            var value = new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
            var sb = new StringBuilder();

            // Perform Base62 encoding
            while (value > 0)
            {
                value = BigInteger.DivRem(value, 62, out var remainder);
                sb.Insert(0, Base62Chars[(int)remainder]);
            }

            // Truncate to maximum shortcut length
            return sb.ToString().Substring(0, Math.Min(sb.Length, MaxShortcutLength));
        }
    }
}
