using System;
using System.Threading.Tasks;
using InceptionCache.Core;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public interface IRedisCacheProvider : ICacheProvider
    {
        Task<T[]> GetSetAsync<T>(string key) where T : class;
        Task AddToSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class;
        Task AddToSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class;
        Task DeleteFromSetAsync<T>(string key, T[] values, TimeSpan? expiry) where T : class;
        Task DeleteFromSetAsync<T>(string key, T value, TimeSpan? expiry) where T : class;
    }
}
