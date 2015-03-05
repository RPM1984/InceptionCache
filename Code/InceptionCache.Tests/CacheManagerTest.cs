using System;
using InceptionCache.Core;
using SimpleLogging.Core;
using System.Threading.Tasks;

namespace InceptionCache.Tests
{
    public class CacheManagerTest : CacheManager
    {
        public CacheManagerTest(
            ILoggingService loggingService, 
            ICacheProvider l1Cache, 
            ICacheProvider l2Cache) : base(loggingService, l1Cache, l2Cache)
        {
        }

        public Task<TestCacheObject> Get(CacheIdentity cacheIdentity, Func<Task<TestCacheObject>> dbQuery)
        {
            return FindItemInCache(cacheIdentity, dbQuery);
        }
    }
}
