using System;
using System.Collections.Generic;
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
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_endpoint));
        private static IDatabase Cache { get { return LazyConnection.Value.GetDatabase(); } }
        private readonly ILoggingService _loggingService;
        private readonly ISerializer _serializer;

        public RedisCacheProvider(string endpoint, ILoggingService loggingService, ISerializer serializer)
        {
            endpoint.ShouldNotBe(null);
            loggingService.ShouldNotBe(null);
            serializer.ShouldNotBe(null);

            _endpoint = endpoint;
            _loggingService = loggingService;
            _serializer = serializer;

            _loggingService.Info("Created Redis Cache at endpoint: {0}", endpoint);
        }

        private void LogDebug<T>(string operation, string key)
        {
            _loggingService.Debug(string.Format("Redis Cache|{0}|Type:{1}|Key:{2}", operation, typeof(T).Name, key));
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
                return null;
            }
        }

        public Task<T[]> GetAsync<T>(string[] keys) where T : class
        {
            try
            {
                LogDebug<T>("Get STRING (Batch)", string.Format("(multiple) ({0}) keys", keys.Length));
                return Cache.GetStringAsync<T>(_serializer, keys);
            }
            catch (Exception exc)
            {
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
            }
        }

        public string Name
        {
            get
            {
                return "Redis Cache Provider";
            }
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
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
                _loggingService.Error(exc);
            }
        }
    }
}
