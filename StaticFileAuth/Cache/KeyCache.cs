using System;
using Microsoft.Extensions.Caching.Memory;

namespace StaticFileAuth.Cache
{
    public class KeyCache: IKeyCache
    {
        private const string CacheName = "KeyCache";
        private const int CacheDurationInMinutes = 10;
        private readonly IMemoryCache _cache;

        public KeyCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Guid GenerateKey()
        {
            var key = Guid.NewGuid();
            _cache.Set($"{CacheName}_{key}", key, TimeSpan.FromMinutes(CacheDurationInMinutes));
            return key;
        }

        public bool IsValid(string key)
        {
            var valid = _cache.TryGetValue($"{CacheName}_{key}", out _);
            return valid;
        }

        public void RefreshKey(string key)
        {
            if (IsValid(key))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationInMinutes));

                _cache.Set($"{CacheName}_{key}", key, cacheEntryOptions);
            }
        }
    }
}
