using Microsoft.Extensions.Caching.Distributed;
using SmartLogAnalyzer.Core.Interfaces;

namespace SmartLogAnalyzer.Infrastructure.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return value != null;
        }

        public async Task SetKeyAsync(string key, string value, TimeSpan expiry)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };
            await _cache.SetStringAsync(key, value, options);
        }
    }
}