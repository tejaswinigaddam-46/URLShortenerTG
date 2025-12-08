namespace URLShortener.Application.Interfaces {
    public interface IURLShortenerCacheRepository
    {
        Task StoreUrlMappingAsync(string shortCode, string longUrl, TimeSpan? expiration = null);
        Task<string?> GetLongUrlAsync(string shortCode);
    }
}