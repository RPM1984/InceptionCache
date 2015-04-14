using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InceptionCache.Core;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public interface IRedisCacheProvider : ICacheProvider
    {
        Task<T[]> GetSetAsync<T>(string key) where T : class;
        Task<Dictionary<string,T[]>> GetSetsAsync<T>(string[] keys) where T : class;
        Task AddToSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class;
        Task AddToSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class;
        Task AddToSetsAsync<T>(Dictionary<string,T[]> keysAndValues, Dictionary<string,TimeSpan?> expiries) where T : class;
        Task DeleteFromSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class;
        Task DeleteFromSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class;
    }
}
