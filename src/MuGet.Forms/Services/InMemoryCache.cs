﻿using Microsoft.Extensions.Caching.Memory;
using System;

namespace MuGet.Forms.Services
{
    public class InMemoryCache : ICacheProvider
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public InMemoryCache()
        {
        }

        public T Get<T>(string key)
        {
            var result = default(T);

            if (_cache.TryGetValue(key, out T value))
            {
                result = value;
            }
            
            return result;
        }

        public void Set<T>(string key, T value, TimeSpan expires)
        {
            _cache.Set(key, value, DateTimeOffset.UtcNow.Add(expires));
        }
    }
}
