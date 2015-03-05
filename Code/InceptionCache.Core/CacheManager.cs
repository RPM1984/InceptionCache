using System;
using System.Threading.Tasks;
using Shouldly;
using SimpleLogging.Core;

namespace InceptionCache.Core
{
    public abstract class CacheManager
    {
        private readonly ILoggingService _loggingService;
        private readonly ICacheProvider _l1Cache;
        private readonly ICacheProvider _l2Cache;

        protected CacheManager(
            ILoggingService loggingService,
            ICacheProvider l1Cache,
            ICacheProvider l2Cache)
        {
            loggingService.ShouldNotBe(null);
            l1Cache.ShouldNotBe(null);
            l2Cache.ShouldNotBe(null);

            _loggingService = loggingService;
            _l1Cache = l1Cache;
            _l2Cache = l2Cache;
        }

        protected async Task<T> FindItemInCache<T>(CacheIdentity cacheIdentity, Func<Task<T>> dbQuery) where T : class
        {
            // Check L1-Cache
            T result;

            var l1Cached = await _l1Cache.GetAsync<T>(cacheIdentity.CacheKey);
            if (l1Cached == null)
            {
                _loggingService.Debug("Cache L1 - MISS! ({0})", cacheIdentity);

                // Check L2-Cache
                var l2Cached = await _l2Cache.GetAsync<T>(cacheIdentity.CacheKey);
                if (l2Cached == null)
                {
                    _loggingService.Debug("Cache L2 - MISS! ({0})", cacheIdentity);

                    // Go to DB.
                    var data = await dbQuery();

                    // Add to L2-Cache.
                    await _l2Cache.SetAsync(cacheIdentity.CacheKey, data, cacheIdentity.L2CacheExpiry);

                    // Add to L1-Cache.
                    await _l1Cache.SetAsync(cacheIdentity.CacheKey, data, cacheIdentity.L1CacheExpiry);

                    result = data;
                }
                else
                {
                    _loggingService.Debug("Cache L2 - HIT! ({0})", cacheIdentity);

                    // Add to L1-Cache
                    await _l1Cache.SetAsync(cacheIdentity.CacheKey, l2Cached, cacheIdentity.L1CacheExpiry);

                    result = l2Cached;
                }
            }
            else
            {
                _loggingService.Debug("Cache L1 - HIT! ({0})", cacheIdentity);

                result = l1Cached;
            }

            return result;
        }
    }
}
