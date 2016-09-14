using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InceptionCache.Core;
using InceptionCache.Core.Serialization;
using SimpleLogging.Core;
using StackExchange.Redis;

namespace InceptionCache.Providers.RedisCacheProvider
{
    public static class RedisCacheExtensions
    {
        public static async Task<T> GetStringAsync<T>(
            this IDatabase cache,
            ILoggingService loggingService,
            LoggingOptions loggingOptions,
            ISerializer serializer,
            string key)
        {
            var item = await cache.StringGetAsync(key).ConfigureAwait(false);
            if (item.IsNull &&
                loggingOptions.LogCacheMisses)
            {
                loggingService.Debug($"Cache Miss. Key: {key}");
                return default(T);
            }

            if (loggingOptions.LogCacheHits)
            {
                loggingService.Debug($"Cache Hit. Key: {key}");
            }

            return serializer.Deserialize<T>(item);
        }

        public static T GetString<T>(
            this IDatabase cache,
            ISerializer serializer,
            string key)
        {
            return serializer.Deserialize<T>(cache.StringGet(key));
        }

        public static async Task<T[]> GetStringAsync<T>(this IDatabase cache,
                                                        ISerializer serializer,
                                                        string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks
                .Where(result => !result.Result.IsNullOrEmpty)
                .Select(result => serializer.Deserialize<T>(result.Result))
                .ToArray();
        }

        public static T[] GetString<T>(this IDatabase cache,
                                       ISerializer serializer,
                                       string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();

            batch.Execute();

            Task.WaitAll(tasks.Cast<Task>().ToArray());

            return tasks.Select(result => serializer.Deserialize<T>(result.Result)).ToArray();
        }

        public static async Task AddStringAsync<T>(this IDatabase cache,
                                                   ISerializer serializer,
                                                   string key,
                                                   T value,
                                                   TimeSpan? expiry,
                                                   CommandFlags flags = CommandFlags.None)
        {
            await cache.StringSetAsync(key, serializer.Serialize(value), expiry, flags: flags).ConfigureAwait(false);
        }

        public static async Task AddStringAsync<T>(this IDatabase cache,
                                                   ISerializer serializer,
                                                   Dictionary<string, T> values,
                                                   TimeSpan? expiry,
                                                   CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks =
                values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry, flags: flags)).ToList();

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static void AddString<T>(this IDatabase cache,
                                        ISerializer serializer,
                                        string key,
                                        T value,
                                        TimeSpan? expiry,
                                        CommandFlags flags = CommandFlags.None)
        {
            cache.StringSet(key, serializer.Serialize(value), expiry, flags: flags);
        }

        public static void AddString<T>(this IDatabase cache,
                                        ISerializer serializer,
                                        Dictionary<string, T> values,
                                        TimeSpan? expiry,
                                        CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks = values.Select(kv => batch.StringSetAsync(kv.Key, serializer.Serialize(kv.Value), expiry, flags: flags));

            batch.Execute();

            Task.WaitAll(tasks.Cast<Task>().ToArray());
        }

        public static async Task AddManyToSetAsync<T>(this IDatabase cache,
                                                      ISerializer serializer,
                                                      string key,
                                                      T[] values,
                                                      TimeSpan? expiry,
                                                      CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks = new Task[]
            {
                batch.SetAddAsync(key, GetSerializedRedisValues(serializer, values), flags),
                batch.KeyExpireAsync(key, expiry, flags)
            };

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task AddSingleToSetAsync<T>(this IDatabase cache,
                                                        ISerializer serializer,
                                                        string key,
                                                        T value,
                                                        TimeSpan? expiry,
                                                        CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks = new Task[]
            {
                batch.SetAddAsync(key, serializer.Serialize(value), flags),
                batch.KeyExpireAsync(key, expiry, flags)
            };

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task RemoveManyFromSetAsync<T>(this IDatabase cache,
                                                           ISerializer serializer,
                                                           string key,
                                                           T[] values,
                                                           TimeSpan? expiry,
                                                           CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks = new Task[]
            {
                batch.SetRemoveAsync(key, GetSerializedRedisValues(serializer, values), flags),
                batch.KeyExpireAsync(key, expiry, flags)
            };

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task RemoveSingleFromSetAsync<T>(this IDatabase cache,
                                                             ISerializer serializer,
                                                             string key,
                                                             T value,
                                                             TimeSpan? expiry,
                                                             CommandFlags flags = CommandFlags.None)
        {
            var batch = cache.CreateBatch();

            var tasks = new Task[]
            {
                batch.SetRemoveAsync(key, serializer.Serialize(value), flags),
                batch.KeyExpireAsync(key, expiry, flags)
            };

            batch.Execute();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static async Task<T[]> GetSetAsync<T>(this IDatabase cache,
                                                     ILoggingService loggingService,
                                                     LoggingOptions loggingOptions,
                                                     ISerializer serializer,
                                                     string key)
        {
            var members = await cache.SetMembersAsync(key).ConfigureAwait(false);
            if (members == null &&
                loggingOptions.LogCacheMisses)
            {
                loggingService.Debug($"Cache Miss. Key: {key}");
            }
            else if (members != null &&
                     loggingOptions.LogCacheHits)
            {
                loggingService.Debug($"Cache Hit. Key: {key}");
            }

            var items = new List<T>();
            if (members != null)
            {
                items.AddRange(members.Select(member => serializer.Deserialize<T>(member)));
            }

            return items.ToArray();
        }

        public static async Task<Dictionary<string, T[]>> GetSetsAsync<T>(this IDatabase cache,
                                                                          ISerializer serializer,
                                                                          string[] keys)
        {
            var batch = cache.CreateBatch();

            var tasks = keys.Select(key => new KeyValuePair<string, Task<RedisValue[]>>(key, batch.SetMembersAsync(key))).ToList();

            batch.Execute();

            await Task.WhenAll(tasks.Select(x => x.Value)).ConfigureAwait(false);

            return tasks.ToDictionary(task => task.Key, task => task.Value.Result.Select(m => serializer.Deserialize<T>(m)).ToArray());
        }

        public static async Task AddManyToSetsAsync<T>(this IDatabase cache,
                                                       ISerializer serializer,
                                                       Dictionary<string, T[]> keysAndValues,
                                                       Dictionary<string, TimeSpan?> expiries,
                                                       CommandFlags flags = CommandFlags.None) where T : class
        {
            var batch = cache.CreateBatch();
            var tasks = keysAndValues
                .Select(key => batch.SetAddAsync(key.Key, key.Value.Select(value => (RedisValue) serializer.Serialize(value)).ToArray(), flags))
                .ToList();
            var expireTasks = expiries.Select(key => batch.KeyExpireAsync(key.Key, key.Value)).ToList();
            batch.Execute();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            await Task.WhenAll(expireTasks).ConfigureAwait(false);
        }

        private static RedisValue[] GetSerializedRedisValues<T>(ISerializer serializer,
                                                                IEnumerable<T> values)
        {
            return values.Select(value => (RedisValue) serializer.Serialize(value)).ToArray();
        }
    }
}