using URLShortener.Application.Exceptions;
using URLShortener.Application.Interfaces;
using URLShortener.Infrastructure.Database.DbContext;
using URLShortener.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace URLShortener.Infrastructure.Database.Repositories {
    public class URLShortenerDBRepository : IURLShortenerDBRepository{
        private readonly ApplicationDbContext _dbContext;

        public URLShortenerDBRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task InsertUrlMappingAsync(long snowflakeId, string shortCode, string longUrl)
        {
            var urlMapping = new UrlMappingDataModel
            {
                SnowflakeId = snowflakeId,
                ShortCode = shortCode,
                LongUrl = longUrl,
                Clicks = 0,
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(3)
            };
            _dbContext.UrlMappings.Add(urlMapping);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetLongUrlAsync(string shortCode)
        {
            var urlMapping = await _dbContext.UrlMappings
                .FirstOrDefaultAsync(um => um.ShortCode == shortCode);
            
            if (urlMapping == null)
            {
                throw new ShortCodeNotFoundException($"Short code '{shortCode}' is not registered in this service.");
            }

            return urlMapping.LongUrl;
        }

        public async Task<string?> GetShortCodeAsync(string longUrl)
        {
            var urlMapping  = await _dbContext.UrlMappings.FirstOrDefaultAsync(um => um.LongUrl == longUrl);

            if (urlMapping == null)
            {
                throw new LongURLNotFoundException($"Long URL '{longUrl}' is not registered in this service.");
            }

            return urlMapping.ShortCode;
        }

        public async Task<int> GetUrlAccessCountAsync(string shortCode)
        {
            var urlMapping = await _dbContext.UrlMappings
                .FirstOrDefaultAsync(um => um.ShortCode == shortCode);
    
            if (urlMapping == null)
            {
                throw new ShortCodeNotFoundException($"Short code '{shortCode}' is not registered in this service.");
            }

            return urlMapping.Clicks;
        }

        public async Task IncrementClickCountAsync(string shortCode)
        {
            var urlMapping = await _dbContext.UrlMappings
                .FirstOrDefaultAsync(um => um.ShortCode == shortCode);
    
            if (urlMapping == null)
            {
                throw new ShortCodeNotFoundException($"Short code '{shortCode}' is not registered in this service.");
            }

            urlMapping.Clicks += 1;
            await _dbContext.SaveChangesAsync();
        }
    }
}
