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

        public Task<TestCacheObject> Get(CacheIdentity cacheIdentity, Func<Task<TestCacheObject>> dbQuery)
        {
            return FindItemInCacheOrDataStore(cacheIdentity, dbQuery);
        }
    }
}
