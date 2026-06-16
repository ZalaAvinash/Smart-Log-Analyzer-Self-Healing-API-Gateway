namespace SmartLogAnalyzer.Core.Interfaces
{
    public interface IRedisCacheService
    {
        Task<bool> KeyExistsAsync(string key);
        Task SetKeyAsync(string key, string value, TimeSpan expiry);
    }
}