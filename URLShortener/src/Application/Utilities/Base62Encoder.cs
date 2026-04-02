using System.Text;

namespace URLShortener.Application.Utilities
{
    public static class Base62Encoder
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encode(long longValue)
        {
            if (longValue == 0) return Alphabet[0].ToString();

            // Use ulong to handle potential negative long values if necessary, 
            // although Snowflake IDs should be positive.
            ulong value = (ulong)longValue;

            Span<char> buffer = stackalloc char[11]; // max length of shortcode for 64-bit ID
            int pos = buffer.Length;
            while (value > 0)
            {
                buffer[--pos] = Alphabet[(int)(value % 62)];
                value /= 62;
            }

            return new string(buffer[pos..]);
        }
    }
}
