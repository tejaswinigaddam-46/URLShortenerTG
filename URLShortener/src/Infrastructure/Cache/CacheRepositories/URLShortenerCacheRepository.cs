using Microsoft.Extensions.Caching.Distributed;
using URLShortener.Application.Exceptions;
using URLShortener.Application.Interfaces;
using URLShortener.Shared.Constants;

namespace URLShortener.Infrastructure.Cache.CacheRepositories {
    public class URLShortenerCacheRepository : IURLShortenerCacheRepository
    {
        private readonly IDistributedCache _cache;
        public URLShortenerCacheRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task StoreUrlMappingAsync(string shortCode, string longUrl, TimeSpan? expiration = null)
        {
            var existingLongUrl = await _cache.GetStringAsync(shortCode);
            if(existingLongUrl != null)
            {
                if (existingLongUrl == longUrl){
                    return;
                }
                else{
                    throw new ShortCodeStorageException("Short code already exists with a different long URL.");
                }
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeConstants.DefaultCacheExpiration
            };
            await _cache.SetStringAsync(shortCode, longUrl, options);
        }

        public async Task<string?> GetLongUrlAsync(string shortCode)
        {
            var longUrl = await _cache.GetStringAsync(shortCode);
            return string.IsNullOrEmpty(longUrl) ? null : longUrl;
        }
    }
}
