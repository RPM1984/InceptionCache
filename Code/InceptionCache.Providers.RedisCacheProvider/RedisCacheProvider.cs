using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public RedisCacheProvider(Lazy<ConnectionMultiplexer> connectionMultiplexer,
            ILoggingService loggingService,
            ISerializer serializer = null)
        {
            connectionMultiplexer.ShouldNotBe(null);
            loggingService.ShouldNotBe(null);

            _lazyConnection = connectionMultiplexer;
            _loggingService = loggingService;
            _serializer = serializer ?? new ProtoBufSerializer();
        }

        public RedisCacheProvider(string endpoint, 
            ILoggingService loggingService, 
            ISerializer serializer = null)
            : this(_lazyConnection, loggingService, serializer)
        {
            endpoint.ShouldNotBe(null);

            _endpoint = endpoint;
            _loggingService.Info("Created Redis Cache at endpoint: {0}", endpoint);
        }

        private static IDatabase Cache
        {
            get { return _lazyConnection.Value.GetDatabase(); }
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING", key);
                return await Cache.GetStringAsync<T>(_serializer, key);
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
                LogDebug<T>("Get STRING (Batch)", string.Format("(multiple) ({0}) keys", keys.Length));
                return await Cache.GetStringAsync<T>(_serializer, keys);
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
                LogDebug<T>("Get STRING (BATCH)", string.Format("(multiple) ({0}) keys", keys.Length));
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

        public async Task AddAsync<T>(string key, T value, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING", key);
                await Cache.AddStringAsync(_serializer, key, value, expiry);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public void Add<T>(string key, T value, TimeSpan expiry) where T : class
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

        public async Task AddAsync<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING (BATCH)", string.Format("(multiple) ({0}) keys", values.Keys.Count));
                await Cache.AddStringAsync(_serializer, values, expiry);
            }
            catch (Exception exc)
            {
                foreach (var key in values.Keys)
                {
                    LogError(key, exc);
                }
            }
        }

        public void Add<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING (BATCH)", string.Format("(multiple) ({0}) keys", values.Keys.Count));
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
                await Cache.KeyDeleteAsync(key);
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

        public string Name
        {
            get { return "Redis Cache Provider"; }
        }

        public async Task<T[]> GetSetAsync<T>(string key) where T : class
        {
            try
            {
                LogDebug<T>("Get SET", key);
                return await Cache.GetSetAsync<T>(_serializer, key);
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
                LogDebug<T>("Get SET (BATCH)", string.Format("(multiple) ({0}) keys", keys.Length));
                return await Cache.GetSetsAsync<T>(_serializer, keys);
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

        public async Task AddToSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add SET", key);
                await Cache.AddManyToSetAsync(_serializer, key, values, expiry);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task AddToSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add SET", key);
                await Cache.AddSingleToSetAsync(_serializer, key, value, expiry);
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
                LogDebug<T>("Add SET (BATCH)", string.Format("(multiple) ({0}) keys", keysAndValues.Count));
                await Cache.AddManyToSetsAsync(_serializer, keysAndValues, expiries);
            }
            catch (Exception exc)
            {
                foreach (var key in keysAndValues.Keys)
                {
                    LogError(key, exc);
                }
            }
        }

        public async Task DeleteFromSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Delete items from SET", key);
                await Cache.RemoveManyFromSetAsync(_serializer, key, values, expiry);
            }
            catch (Exception exc)
            {
                LogError(key, exc);
            }
        }

        public async Task DeleteFromSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class
        {
            try
            {
                LogDebug<T>("Delete item SET", key);
                await Cache.RemoveSingleFromSetAsync(_serializer, key, value, expiry);
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
                await Cache.KeyDeleteAsync(keys.Select(x => (RedisKey)x).ToArray());
            }
            catch (Exception exc)
            {
                LogError(string.Join(",", keys), exc);
            }
        }

        private void LogDebug<T>(string operation, string key)
        {
            _loggingService.Debug(string.Format("Redis Cache|{0}|Type:{1}|Key:{2}", operation, typeof (T).Name, key));
        }

        private void LogError(string key, Exception exc)
        {
            _loggingService.Error(new Exception(string.Format("Key: {0}", key), exc));
        }
    }
}