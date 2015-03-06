using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InceptionCache.Core;
using InceptionCache.Core.Serialization;
using Shouldly;
using SimpleLogging.Core;
using StackExchange.Redis;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public class RedisCacheProvider : ICacheProvider
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
                LogDebug<T>("GET", key);
                return await Cache.GetAsync<T>(_serializer, key);
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
                LogDebug<T>("GET", key);
                return Cache.Get<T>(_serializer, key);
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
                LogDebug<T>("BATCH GET", string.Format("(multiple) ({0}) keys", keys.Length));
                return Cache.GetAsync<T>(_serializer, keys);
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
                LogDebug<T>("BATCH GET", string.Format("(multiple) ({0}) keys", keys.Length));
                return Cache.Get<T>(_serializer, keys);
            }
            catch (Exception exc)
            {
                _loggingService.Error(exc);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("SET", key);
                await Cache.SetAsync(_serializer, key, value, expiry);
            }
            catch (Exception exc)
            {
                _loggingService.Error(exc);
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("SET", key);
                Cache.Set(_serializer, key, value, expiry);
            }
            catch (Exception exc)
            {
                _loggingService.Error(exc);
            }
        }

        public async Task SetAsync<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("BATCH SET", string.Format("(multiple) ({0}) keys", values.Keys.Count));
                await Cache.SetAsync(_serializer, values, expiry);
            }
            catch (Exception exc)
            {
                _loggingService.Error(exc);
            }
        }

        public void Set<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class
        {
            try
            {
                LogDebug<T>("BATCH SET", string.Format("(multiple) ({0}) keys", values.Keys.Count));
                Cache.Set(_serializer, values, expiry);
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
                LogDebug<string>("DELETE", key);
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
                LogDebug<string>("DELETE", key);
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
    }
}
