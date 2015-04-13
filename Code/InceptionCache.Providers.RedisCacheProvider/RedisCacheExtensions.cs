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
        public static async Task<T> GetStringAsync<T>(this IDatabase cache, ISerializer serializer, string key)
        {
            return serializer.Deserialize<T>(await cache.StringGetAsync(key));
        }

        public static T GetString<T>(this IDatabase cache, ISerializer serializer, string key)
        {
            return serializer.Deserialize<T>(cache.StringGet(key));
        }

        public static async Task<T[]> GetStringAsync<T>(this IDatabase cache, ISerializer serializer, string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks);

            return tasks.Select(result => serializer.Deserialize<T>(result.Result)).ToArray();
        }

        public static T[] GetString<T>(this IDatabase cache, ISerializer serializer, string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            Task.WhenAll(tasks);

            return tasks.Select(result => serializer.Deserialize<T>(result.Result)).ToArray();
        }

        public static async Task AddStringAsync<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            await cache.StringSetAsync(key, serializer.Serialize(value), expiry);
        }

        public static async Task AddStringAsync<T>(this IDatabase cache, ISerializer serializer, Dictionary<string, T> values, TimeSpan? expiry)
        {
            var batch = cache.CreateBatch();

            var tasks = values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks);
        }

        public static void AddString<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            cache.StringSet(key, serializer.Serialize(value), expiry);
        }

        public static void AddString<T>(this IDatabase cache, ISerializer serializer, Dictionary<string, T> values, TimeSpan? expiry)
        {
            var batch = cache.CreateBatch();

            var tasks = values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry)).ToList();

            batch.Execute();

            Task.WhenAll(tasks).Wait();
        }

        public static async Task AddManyToSetAsync<T>(this IDatabase cache, ISerializer serializer, string key, T[] values, TimeSpan? expiry)
        {
            await cache.SetAddAsync(key, GetSerializedRedisValues(serializer, values));
            await cache.KeyExpireAsync(key, expiry);
        }

        public static async Task AddSingleToSetAsync<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            await cache.SetAddAsync(key, serializer.Serialize(value));
            await cache.KeyExpireAsync(key, expiry);
        }

        public static async Task RemoveManyFromSetAsync<T>(this IDatabase cache, ISerializer serializer, string key, T[] values, TimeSpan? expiry)
        {
            await cache.SetRemoveAsync(key, GetSerializedRedisValues(serializer, values));
            await cache.KeyExpireAsync(key, expiry);
        }

        public static async Task RemoveSingleFromSetAsync<T>(this IDatabase cache, ISerializer serializer, string key, T value, TimeSpan? expiry)
        {
            await cache.SetRemoveAsync(key, serializer.Serialize(value));
            await cache.KeyExpireAsync(key, expiry);
        }
        
        public static async Task<T[]> GetSetAsync<T>(this IDatabase cache, ISerializer serializer, string key)
        {
            var members = await cache.SetMembersAsync(key);
            var items = new List<T>();
            if (members != null)
            {
                items.AddRange(members.Select(member => serializer.Deserialize<T>(member)));
            }

            return items.ToArray();
        }

        private static RedisValue[] GetSerializedRedisValues<T>(ISerializer serializer, IEnumerable<T> values)
        {
            return values.Select(value => (RedisValue)serializer.Serialize(value)).ToArray();
        }
    }
}
