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
    public class InMemoryCacheProvider
        : ICacheProvider,
          IInMemoryCacheProvider
    {
        private readonly ObjectCache _cache;
        private readonly ILoggingService _loggingService;

        public InMemoryCacheProvider(ObjectCache cache,
                                     ILoggingService loggingService)
        {
            cache.ShouldNotBeNull();
            loggingService.ShouldNotBeNull();

            _cache = cache;
            _loggingService = loggingService;

            _loggingService.Info("Created In Memory Cache");
        }

        public string Name => "In-Memory Cache Provider";

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            key.ShouldNotBeNullOrWhiteSpace();
            return Get<T>(key);
        }

        public T Get<T>(string key) where T : class
        {
            key.ShouldNotBeNullOrWhiteSpace();

            LogDebug<T>("GET", key);

            return (T) _cache.Get(key);
        }

        public T[] Get<T>(string[] keys) where T : class
        {
            keys.ShouldNotBeNull();

            LogDebug<T>("BATCH GET", $"(multiple) ({keys.Length}) keys");

            return keys.Select(Get<T>).ToArray();
        }

        public async Task<T[]> GetAsync<T>(string[] keys) where T : class
        {
            keys.ShouldNotBeNull();
            return Get<T>(keys);
        }

        public async Task AddAsync<T>(string key,
                                      T value,
                                      TimeSpan expiry) where T : class
        {
            key.ShouldNotBeNullOrWhiteSpace();
            value.ShouldNotBeNull();

            await Task.Run(() => Add(key, value, expiry))
                      .ConfigureAwait(false);
        }

        public void Add<T>(string key,
                           T value,
                           TimeSpan expiry) where T : class
        {
            key.ShouldNotBeNullOrWhiteSpace();
            value.ShouldNotBeNull();

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
            values.ShouldNotBeNull();

            await Task.Run(() => Add(values, expiry)).ConfigureAwait(false);
        }

        public void Add<T>(Dictionary<string, T> values,
                           TimeSpan expiry) where T : class
        {
            values.ShouldNotBeNull()
                ;
            foreach (var kv in values)
            {
                Add(kv.Key, kv.Value, expiry);
            }
        }

        public async Task DeleteAsync(string key)
        {
            key.ShouldNotBeNullOrWhiteSpace();
            await Task.Run(() => Delete(key)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string[] keys)
        {
            keys.ShouldNotBeNull();
            await Task.Run(() => Delete(keys)).ConfigureAwait(false);
        }

        public void Delete(string[] keys)
        {
            keys.ShouldNotBeNull();
            foreach (var key in keys)
            {
                Delete(key);
            }
        }

        public void Delete(string key)
        {
            key.ShouldNotBeNullOrWhiteSpace();
            LogDebug<string>("DELETE", key);

            _cache.Remove(key);
        }

        public long TotalCount()
        {
            return _cache.GetCount();
        }

        private void LogDebug<T>(string operation,
                                 string key)
        {
            operation.ShouldBeNullOrWhiteSpace();
            key.ShouldNotBeNullOrWhiteSpace();

            _loggingService.Debug($"In-Memory Cache|{operation}|Type:{typeof(T).Name}|Key:{key}");
        }
    }
}