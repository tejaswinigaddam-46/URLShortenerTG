using URLShortener.Application.Interfaces;
using Microsoft.Extensions.Logging;
using URLShortener.Application.Exceptions;
using URLShortener.Application.Utilities;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace URLShortener.Application.Services{
    public class URLShortenerService : IURLShortenerService
    {
        private readonly IURLShortenerCacheRepository _cacheRepository;
        private readonly IURLShortenerDBRepository _dbRepository;
         private readonly ILogger<URLShortenerService> _logger;
        private readonly SnowflakeIdGenerator _snowflakeIdGenerator;
        
        public URLShortenerService(IURLShortenerCacheRepository cacheRepository, IURLShortenerDBRepository dbRepository, ILogger<URLShortenerService> logger, SnowflakeIdGenerator snowflakeIdGenerator)
        {
            _cacheRepository = cacheRepository;
            _dbRepository = dbRepository;
            _logger = logger;
            _snowflakeIdGenerator = snowflakeIdGenerator;
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

            // Step 1 → Generate Snowflake ID
            long snowflakeId = _snowflakeIdGenerator.NextId();

            // Step 2 → Base62Encode the Snowflake ID
            string code = Base62Encoder.Encode(snowflakeId);

            // Step 3 → Insert into DB using new DB method
            await _dbRepository.InsertUrlMappingAsync(snowflakeId, code, longUrl);

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

        
    }
}
