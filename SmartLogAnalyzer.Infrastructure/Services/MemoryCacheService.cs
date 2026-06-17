using Microsoft.Extensions.Caching.Memory;
using SmartLogAnalyzer.Core.Interfaces;

namespace SmartLogAnalyzer.Infrastructure.Services
{
    public class MemoryCacheService : IRedisCacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<bool> KeyExistsAsync(string key)
        {
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }

        public Task SetKeyAsync(string key, string value, TimeSpan expiry)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };
            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}