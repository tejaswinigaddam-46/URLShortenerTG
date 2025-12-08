using URLShortener.Application.Interfaces;
using Microsoft.Extensions.Logging;
using URLShortener.Application.Exceptions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace URLShortener.Application.Services{
    public class URLShortenerService : IURLShortenerService
    {
        private readonly IURLShortenerCacheRepository _cacheRepository;
        private readonly IURLShortenerDBRepository _dbRepository;
         private readonly ILogger<URLShortenerService> _logger;
        private const string Green = "\x1B[32m";
        private const string Reset = "\x1B[0m";

        public URLShortenerService(IURLShortenerCacheRepository cacheRepository, IURLShortenerDBRepository dbRepository, ILogger<URLShortenerService> logger)
        {
            _cacheRepository = cacheRepository;
            _dbRepository = dbRepository;
            _logger = logger;
        }

        public async Task<string?> GenerateShortCodeAsync(string longUrl)
        {
            if(string.IsNullOrEmpty(longUrl))
            {
                throw new ArgumentException("Long URL cannot be null or empty.", nameof(longUrl));
            }

            try
            {
                var existing = await _dbRepository.GetShortCodeAsync(longUrl);
                if (!string.IsNullOrEmpty(existing))
                {
                    return existing;
                }
            }
            catch (LongURLNotFoundException)
            {
            }

            // Step 1 → Insert URL, get sequential ID
            long id = await _dbRepository.InsertLongUrlAsync(longUrl);

            // Step 2 → XOR obfuscation
            var secretSaltStr = Environment.GetEnvironmentVariable("SECRET_SALT");
            if (!long.TryParse(secretSaltStr, out var secretSalt))
            {
                throw new InvalidOperationException("SECRET_SALT is not configured or invalid.");
            }
            // _logger.LogInformation($"{Green}ID from DB: {id}{Reset}");
            long obfuscated = (id*9853) ^ secretSalt;
            // _logger.LogInformation($"{Green}Obfuscated ID: {obfuscated}{Reset}");
            string code = Base62EncodeFixed(obfuscated, 7);
           //  _logger.LogInformation($"{Green}Generated short code after obfuscation: {code} for ID: {id}{Reset}");
            // Step 4 → Update DB with the final shortcode
            await _dbRepository.UpdateShortCodeAsync(id, code);

            return code;
        }

        public async Task<string?> GetLongUrlAsync(string shortCode)
        {
            if(string.IsNullOrEmpty(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.", nameof(shortCode));
            }

            var longUrl = await _cacheRepository.GetLongUrlAsync(shortCode);
            if (longUrl != null)
            {
                return longUrl;
            }

            longUrl = await _dbRepository.GetLongUrlAsync(shortCode);
            if (longUrl != null)
            {
                await _cacheRepository.StoreUrlMappingAsync(shortCode, longUrl);
            }
            else{
                throw new ShortCodeNotFoundException($"Short code '{shortCode}' not found in Database.");
            }

            return longUrl;
        }

        public async Task<string?> GetShortCodeAsync(string longUrl)
        {
            if(string.IsNullOrEmpty(longUrl))
            {
                throw new ArgumentException("Long URL cannot be null or empty.", nameof(longUrl));
            }

            return await _dbRepository.GetShortCodeAsync(longUrl);
        }

        public Task<int> GetUrlAccessCountAsync(string shortCode)
        {
            if(string.IsNullOrEmpty(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.", nameof(shortCode));
            }

            return _dbRepository.GetUrlAccessCountAsync(shortCode);
        }

        public Task IncrementClickCountAsync(string shortCode)
        {
            if(string.IsNullOrEmpty(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.", nameof(shortCode));
            }

            return _dbRepository.IncrementClickCountAsync(shortCode);
        }

        private string Base62EncodeFixed(long value, int length)
        {
            const string base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new char[length];
            var modulus = 1L;
            for (int i = 0; i < length; i++) modulus *= 62L;
            var v = ((value % modulus) + modulus) % modulus;
            // _logger.LogInformation($"{Green}Initial value before obfuscation: {v} {Reset}");
            
            for (int i = length - 1; i >= 0; i--)
            {
                var rem = (int)(v % 62L);
                result[i] = base62Chars[rem];
                v /= 62L;
            }
           // _logger.LogInformation($"{Green}Remaining value after obfuscation: {v} {Reset}");
            
            return new string(result);
        }
    }
}
