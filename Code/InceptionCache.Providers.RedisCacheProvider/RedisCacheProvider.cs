using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            connectionMultiplexer.ShouldNotBeNull();
            loggingService.ShouldNotBeNull();

            _lazyConnection = connectionMultiplexer;
            _loggingService = loggingService;
            _serializer = serializer ?? new ProtoBufSerializer();
        }

        public RedisCacheProvider(string endpoint, 
            ILoggingService loggingService, 
            ISerializer serializer = null)
            : this(_lazyConnection, 
                  loggingService, 
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
                LogDebug<T>("Get STRING (Batch)", $"(multiple) ({keys.Length}) keys");
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
                LogDebug<T>("Get STRING (BATCH)", $"(multiple) ({keys.Length}) keys");
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
                await Cache.AddStringAsync(_serializer, key, value, expiry);
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

        public async Task AddAsync<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("Add STRING (BATCH)", $"(multiple) ({values.Keys.Count}) keys");
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

        public string Name => "Redis Cache Provider";

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
                LogDebug<T>("Get SET (BATCH)", $"(multiple) ({keys.Length}) keys");
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

        public async Task AddToSetAsync<T>(string key, 
            T[] values, 
            TimeSpan? expiry) where T : class
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

        public async Task AddToSetAsync<T>(string key, 
            T value, 
            TimeSpan? expiry) where T : class
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
                LogDebug<T>("Add SET (BATCH)", $"(multiple) ({keysAndValues.Count}) keys");
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

        public async Task DeleteFromSetAsync<T>(string key, 
            T[] values, 
            TimeSpan? expiry) where T : class
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

        public async Task DeleteFromSetAsync<T>(string key, 
            T value, 
            TimeSpan? expiry) where T : class
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
            _loggingService.Debug($"Redis Cache|{operation}|Type:{typeof(T).Name}|Key:{key}");
        }

        private void LogError(string key, Exception exc)
        {
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
}