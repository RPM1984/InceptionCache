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

        public InMemoryCacheProvider(ObjectCache cache,
                                     ILoggingService loggingService)
        {
            cache.ShouldNotBe(null);
            loggingService.ShouldNotBe(null);

            _cache = cache;
            _loggingService = loggingService;

            _loggingService.Info("Created In Memory Cache");
        }

        private void LogDebug<T>(string operation,
                                 string key)
        {
            _loggingService.Debug($"In-Memory Cache|{operation}|Type:{typeof(T).Name}|Key:{key}");
        }


        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return Get<T>(key);
        }

        public T Get<T>(string key) where T : class
        {
            LogDebug<T>("GET", key);

            return (T) _cache.Get(key);
        }

        public T[] Get<T>(string[] keys) where T : class
        {
            LogDebug<T>("BATCH GET", $"(multiple) ({keys.Length}) keys");

            return keys.Select(Get<T>).ToArray();
        }

        public async Task<T[]> GetAsync<T>(string[] keys) where T : class
        {
            return Get<T>(keys);
        }

        public async Task AddAsync<T>(string key,
                                      T value,
                                      TimeSpan expiry) where T : class
        {
            await Task.Run(() => Add(key, value, expiry)).ConfigureAwait(false);
        }

        public void Add<T>(string key,
                           T value,
                           TimeSpan expiry) where T : class
        {
            LogDebug<T>("SET", key);

            var existingItem = _cache.Get(key);
            if (existingItem != null)
            {
                Delete(key);
            }

            _cache.Add(key,
                       value,
                       new CacheItemPolicy
                       {
                           AbsoluteExpiration = DateTimeOffset.UtcNow.AddTicks(expiry.Ticks)
                       });
        }

        public async Task AddAsync<T>(Dictionary<string, T> values,
                                      TimeSpan expiry) where T : class
        {
            await Task.Run(() => Add(values, expiry)).ConfigureAwait(false);
        }

        public void Add<T>(Dictionary<string, T> values,
                           TimeSpan expiry) where T : class
        {
            foreach (var kv in values)
            {
                Add(kv.Key, kv.Value, expiry);
            }
        }

        public async Task DeleteAsync(string key)
        {
            await Task.Run(() => Delete(key)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string[] keys)
        {
            await Task.Run(() => Delete(keys)).ConfigureAwait(false);
        }

        public void Delete(string[] keys)
        {
            foreach (var key in keys)
            {
                Delete(key);
            }
        }

        public void Delete(string key)
        {
            LogDebug<string>("DELETE", key);

            _cache.Remove(key);
        }

        public string Name => "In-Memory Cache Provider";
    }
}