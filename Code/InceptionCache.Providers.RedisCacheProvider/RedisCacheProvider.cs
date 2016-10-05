using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using InceptionCache.Core;
using InceptionCache.Core.Serialization;
using Shouldly;
using SimpleLogging.Core;
using StackExchange.Redis;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public class RedisCacheProvider : IRedisCacheProvider
    {
        private static string _endpoint;

        private static Lazy<ConnectionMultiplexer> _lazyConnection =
            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_endpoint));

        private readonly ILoggingService _loggingService;
        private readonly ISerializer _serializer;
        private readonly LoggingOptions _loggingOptions;
        private readonly ObjectCache _inMemoryCache = MemoryCache.Default;

        public RedisCacheProvider(Lazy<ConnectionMultiplexer> connectionMultiplexer,
                                  ILoggingService loggingService,
                                  LoggingOptions loggingOptions,
                                  ISerializer serializer = null)
        {
            connectionMultiplexer.ShouldNotBeNull();
            loggingService.ShouldNotBeNull();
            loggingOptions.ShouldNotBeNull();

            _lazyConnection = connectionMultiplexer;
            _loggingService = loggingService;
            _loggingOptions = loggingOptions;
            _serializer = serializer ?? new ProtoBufSerializer();
        }

        public RedisCacheProvider(string endpoint,
                                  ILoggingService loggingService,
                                  LoggingOptions loggingOptions,
                                  ISerializer serializer = null)
            : this(_lazyConnection,
                   loggingService,
                   loggingOptions,
                   serializer)
        {
            endpoint.ShouldNotBeNull();

            _endpoint = endpoint;
            _loggingService.Info("Created Redis Cache at endpoint: {0}", endpoint);
        }

        private static IDatabase Cache => _lazyConnection.Value.GetDatabase();
        
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING", key);
                AddToCacheStats(key);
                return await Cache.GetStringAsync<T>(_loggingService, _loggingOptions, _serializer, key).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
                return null;
            }
        }

        public T Get<T>(string key) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING", key);
                AddToCacheStats(key);
                return Cache.GetString<T>(_serializer, key);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
                return null;
            }
        }

        public async Task<T[]> GetAsync<T>(string[] keys) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING (Batch)", $"(multiple) ({keys.Length}) keys");
                AddToCacheStats(keys);
                return await Cache.GetStringAsync<T>(_serializer, keys).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                foreach (var key in keys)
                {
                    LogError(key, exc);
                }
                return null;
            }
        }

        public T[] Get<T>(string[] keys) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING (BATCH)", $"(multiple) ({keys.Length}) keys");
                AddToCacheStats(keys);
                return Cache.GetString<T>(_serializer, keys);
            }
            catch (Exception exc)
            {
                foreach (var key in keys)
                {
                    LogError(key, exc);
                }
                return null;
            }
        }

        public async Task AddAsync<T>(string key,
                                      T value,
                                      TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING", key);
                await Cache.AddStringAsync(_serializer, key, value, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public void Add<T>(string key,
                           T value,
                           TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING", key);
                Cache.AddString(_serializer, key, value, expiry);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task AddAsync<T>(Dictionary<string, T> values,
                                      TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING (BATCH)", $"(multiple) ({values.Keys.Count}) keys");
                await Cache.AddStringAsync(_serializer, values, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                foreach (var key in values.Keys)
                {
                    LogError(key, exc);
                }
            }
        }

        public void Add<T>(Dictionary<string, T> values,
                           TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING (BATCH)", $"(multiple) ({values.Keys.Count}) keys");
                Cache.AddString(_serializer, values, expiry);
            }
            catch (Exception exc)
            {
                foreach (var key in values.Keys)
                {
                    LogError(key, exc);
                }
            }
        }

        public async Task DeleteAsync(string key)
        {
            try
            {
                LogDebug<string>("Delete STRING", key);
                await Cache.KeyDeleteAsync(key).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public void Delete(string key)
        {
            try
            {
                LogDebug<string>("Delete STRING", key);
                Cache.KeyDelete(key);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public void Delete(string[] keys)
        {
            try
            {
                LogDebug<string>("Delete STRING (many)", string.Join(",", keys));
                Cache.KeyDelete(keys.Select(x => (RedisKey)x).ToArray());
            }
            catch (Exception exc)
            {
                LogError(string.Join(",", keys), exc);
            }
        }

        public string Name => "Redis Cache Provider";

        public async Task<T[]> GetSetAsync<T>(string key) where T : class
        {
            try
            {
                LogDebug<T>("Get SET", key);
                AddToCacheStats(key);
                return await Cache.GetSetAsync<T>(_loggingService, _loggingOptions, _serializer, key).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
                return null;
            }
        }

        public async Task<Dictionary<string, T[]>> GetSetsAsync<T>(string[] keys) where T : class
        {
            try
            {
                LogDebug<T>("Get SET (BATCH)", $"(multiple) ({keys.Length}) keys");
                AddToCacheStats(keys);
                return await Cache.GetSetsAsync<T>(_serializer, keys).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                foreach (var key in keys)
                {
                    LogError(key, exc);
                }
                return null;
            }
        }

        public async Task AddToSetAsync<T>(string key,
                                           T[] values,
                                           TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add SET", key);
                await Cache.AddManyToSetAsync(_serializer, key, values, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task AddToSetAsync<T>(string key,
                                           T value,
                                           TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add SET", key);
                await Cache.AddSingleToSetAsync(_serializer, key, value, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task AddToSetsAsync<T>(Dictionary<string, T[]> keysAndValues,
                                            Dictionary<string, TimeSpan?> expiries) where T : class
        {
            try
            {
                LogDebug<T>("Add SET (BATCH)", $"(multiple) ({keysAndValues.Count}) keys");
                await Cache.AddManyToSetsAsync(_serializer, keysAndValues, expiries).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                foreach (var key in keysAndValues.Keys)
                {
                    LogError(key, exc);
                }
            }
        }

        public async Task DeleteFromSetAsync<T>(string key,
                                                T[] values,
                                                TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Delete items from SET", key);
                await Cache.RemoveManyFromSetAsync(_serializer, key, values, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task DeleteFromSetAsync<T>(string key,
                                                T value,
                                                TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Delete item SET", key);
                await Cache.RemoveSingleFromSetAsync(_serializer, key, value, expiry).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task DeleteAsync(string[] keys)
        {
            try
            {
                LogDebug<string>("Delete STRING (many)", string.Join(",", keys));
                await Cache.KeyDeleteAsync(keys.Select(x => (RedisKey) x).ToArray()).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError(string.Join(",", keys), exc);
            }
        }

        private void LogDebug<T>(string operation,
                                 string key)
        {
            _loggingService.Debug($"Redis Cache|{operation}|Type:{typeof(T).Name}|Key:{key}");
        }

        private void AddToCacheStats(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                AddToCacheStats(key);
            }
        }

        private void AddToCacheStats(string key)
        {
            if (!_loggingOptions.LogCacheStatsSummaryAfter.HasValue)
            {
                return;
            }
            
            if (_inMemoryCache.Contains(CacheStatsKey))
            {
                
                var stats = (CacheStatsItem) _inMemoryCache[CacheStatsKey];

                if (stats.Items.ContainsKey(key))
                {
                    stats.Items[key]++;
                }
                else
                {
                    stats.Items.TryAdd(key, 1);
                }
                
            }
            else
            {
                var stats = new CacheStatsItem
                {
                    Items = new ConcurrentDictionary<string, int>()
                };

                stats.Items.TryAdd(key, 1);
                _inMemoryCache.Set(CacheStatsKey,
                                   stats,
                                   new CacheItemPolicy
                                   {
                                       AbsoluteExpiration = DateTime.Now.Add(_loggingOptions.LogCacheStatsSummaryAfter.Value),
                                       RemovedCallback = cacheEntryRemovedArgs =>
                                       {
                                           LogCacheStats(
                                               _loggingService,
                                               _loggingOptions.LogCacheStatsSummaryAfter.Value,
                                               ((CacheStatsItem) cacheEntryRemovedArgs.CacheItem.Value).Items
                                               );
                                       }
                                   });
            }
        }

        private const string CacheStatsKey = "cache-stats";
        
        private static void LogCacheStats(
            ILoggingService loggingService,
            TimeSpan logCacheStatsSummaryAfter,
            ConcurrentDictionary<string, int> stats)
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Multiple GET's for same key after after {logCacheStatsSummaryAfter.Minutes} minutes:");
            foreach (var stat in stats
                .Where(stat => stat.Value > 1)
                .OrderByDescending(stat => stat.Value)
                .ThenBy(stat => stat.Key))
            {
                stringBuilder.AppendLine($"{stat.Key}|{stat.Value}");
            }
            loggingService.Info(stringBuilder.ToString());
        }

        private void LogError(string key,
                              Exception exc)
        {
            if (!_loggingOptions.LogErrors)
            {
                return;
            }

            var error = new StringBuilder();
            error.AppendLine($"Error occured. Key: {key}");
            error.AppendLine("Exception: ");
            error.AppendLine(exc.Message);
            if (exc.InnerException != null)
            {
                error.AppendLine("Inner Exception: ");
                error.AppendLine(exc.InnerException.Message);
            }
            if (exc.StackTrace != null)
            {
                error.AppendLine("StackTrace: ");
                error.AppendLine(exc.StackTrace);
            }

            _loggingService.Error(error.ToString());
        }
    }

    public class CacheStatsItem
    {

        public ConcurrentDictionary<string, int> Items { get; set; } = new ConcurrentDictionary<string, int>();
    }
}