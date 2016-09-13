using System;
using InceptionCache.Core;
using SimpleLogging.Core;
using System.Threading.Tasks;

namespace InceptionCache.Tests
{
    public class CacheManagerTest : InceptionCacheManager
    {
        public CacheManagerTest(
            ILoggingService loggingService,
            ICacheProvider[] cacheProviders)
            : base(loggingService, cacheProviders)
        {
        }

        public async Task<TestCacheObject> GetAsync(CacheIdentity cacheIdentity,
                                                    Func<Task<TestCacheObject>> dbQuery)
        {
            return await FindItemInCacheOrDataStoreAsync(cacheIdentity, dbQuery);
        }
    }
}