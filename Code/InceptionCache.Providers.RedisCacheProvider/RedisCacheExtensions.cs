using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InceptionCache.Core.Serialization;
using StackExchange.Redis;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public static class RedisCacheExtensions
    {
        public static async Task<T> GetAsync<T>(this IDatabase cache, ISerializer serializer, string key)
        {
            return serializer.Deserialize<T>(await cache.StringGetAsync(key));
        }

        public static T Get<T>(this IDatabase cache, ISerializer serializer, string key)
        {
            return serializer.Deserialize<T>(cache.StringGet(key));
        }

        public static async Task<T[]> GetAsync<T>(this IDatabase cache, ISerializer serializer, string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks);

            return tasks.Select(result => serializer.Deserialize<T>(result.Result)).ToArray();
        }

        public static T[] Get<T>(this IDatabase cache, ISerializer serializer, string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            Task.WhenAll(tasks);

            return tasks.Select(result => serializer.Deserialize<T>(result.Result)).ToArray();
        }

        public static async Task SetAsync<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            await cache.StringSetAsync(key, serializer.Serialize(value), expiry);
        }

        public static async Task SetAsync<T>(this IDatabase cache, ISerializer serializer, Dictionary<string, T> values, TimeSpan? expiry)
        {
            var batch = cache.CreateBatch();

            var tasks = values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks);
        }

        public static void Set<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            cache.StringSet(key, serializer.Serialize(value), expiry);
        }

        public static void Set<T>(this IDatabase cache, ISerializer serializer, Dictionary<string, T> values, TimeSpan? expiry)
        {
            var batch = cache.CreateBatch();

            var tasks = values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry)).ToList();

            batch.Execute();

            Task.WhenAll(tasks).Wait();
        }


    }
}
