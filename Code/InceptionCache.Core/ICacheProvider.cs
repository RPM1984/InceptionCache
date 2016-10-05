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

        Task AddAsync<T>(string key,
                         T value,
                         TimeSpan expiry) where T : class;

        void Add<T>(string key,
                    T value,
                    TimeSpan expiry) where T : class;

        Task AddAsync<T>(Dictionary<string, T> values,
                         TimeSpan expiry) where T : class;

        void Add<T>(Dictionary<string, T> values,
                    TimeSpan expiry) where T : class;

        Task DeleteAsync(string key);
        Task DeleteAsync(string[] keys);
        void Delete(string key);
        void Delete(string[] keys);

        string Name { get; }
    }
}