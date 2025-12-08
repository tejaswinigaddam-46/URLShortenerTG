namespace URLShortener.Application.Interfaces {
    public interface IURLShortenerDBRepository
    {
        public Task<long> InsertLongUrlAsync(string longUrl);
        public Task<string?> GetLongUrlAsync(string shortCode);
        public Task<string?> GetShortCodeAsync(string longUrl);
        public Task<int> GetUrlAccessCountAsync(string shortCode);
        public Task IncrementClickCountAsync(string shortCode);
        public Task UpdateShortCodeAsync(long id, string shortCode);
    }
}
