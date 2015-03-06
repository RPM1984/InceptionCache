using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InceptionCache.Core
{
    public interface ICacheProvider
    {
        Task<T> GetAsync<T>(string key) where T : class;
        T Get<T>(string key) where T : class;

        Task<T[]> GetAsync<T>(string[] keys) where T : class;
        T[] Get<T>(string[] keys) where T : class;

        Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class;
        void Set<T>(string key, T value, TimeSpan expiry) where T : class;

        Task SetAsync<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class;
        void Set<T>(Dictionary<string, T> values, TimeSpan expiry) where T : class;

        Task DeleteAsync(string key);
        void Delete(string key);

        string Name { get; }
    }
}
