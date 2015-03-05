using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using InceptionCache.Core;
using Shouldly;
using SimpleLogging.Core;

namespace InceptionCache.Providers.InMemoryCacheProvider
{
    public class InMemoryCacheProvider : ICacheProvider
    {
        private readonly ObjectCache _cache;
        private readonly ILoggingService _loggingService;

        public InMemoryCacheProvider(ObjectCache cache, ILoggingService loggingService)
        {
            cache.ShouldNotBe(null);
            loggingService.ShouldNotBe(null);

            _cache = cache;
            _loggingService = loggingService;

            _loggingService.Info("Created In Memory Cache: {0}", cache.DefaultCacheCapabilities.ToString());
        }

        private void LogDebug<T>(string operation, string key)
        {
            _loggingService.Debug(string.Format("In-Memory Cache|{0}|Type:{1}|Key:{2}", operation, typeof(T).Name, key));
        }


        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return Get<T>(key);
        }

        public T Get<T>(string key) where T : class
        {
            LogDebug<T>("GET", key);

            return (T)_cache.Get(key);
        }

        public T[] Get<T>(string[] keys) where T : class
        {
            LogDebug<T>("BATCH GET", string.Format("(multiple) ({0}) keys", keys.Length));

            return keys.Select(Get<T>).ToArray();
        }

        public Task<T[]> GetAsync<T>(string[] keys) where T : class
        {
            return Task.FromResult(Get<T>(keys));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
        {
            Set(key, value, expiry);
        }

        public void Set<T>(string key, T value, TimeSpan expiry) where T : class
        {
            LogDebug<T>("SET", key);

            var existingItem = _cache.Get(key);
            if (existingItem != null)
            {
                Delete(key);
            }

            _cache.Add(key, value, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddTicks(expiry.Ticks)
            });
        }

        public async Task SetAsync<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            Set(values, expiry);
        }

        public void Set<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            foreach (var kv in values)
            {
                Set(kv.Key, kv.Value, expiry);
            }
        }

        public async Task DeleteAsync(string key)
        {
            Delete(key);
        }

        public void Delete(string key)
        {
            LogDebug<string>("DELETE", key);

            _cache.Remove(key);
        }
    }
}
