using System;
using System.Threading.Tasks;
using FakeItEasy;
using InceptionCache.Core;
using Shouldly;
using SimpleLogging.Core;
using Xunit;

namespace InceptionCache.Tests
{
    public class InceptionCacheManagerFacts
    {
        [Fact]
        public async Task GivenACacheIdentityWhichExistsInL1Cache_FindItemInCache_ReturnsItemFromL1Cache()
        {
            // Arrange.
            var loggingService = A.Fake<ILoggingService>();
            var l1Cache = A.Fake<ICacheProvider>();
            var l2Cache = A.Fake<ICacheProvider>();
            var cacheLayer = new CacheManagerTest(loggingService,
                                                  new[]
                                                  {
                                                      l1Cache,
                                                      l2Cache
                                                  });
            const string cacheKey = "Test";
            var expiry = TimeSpan.FromMinutes(1);
            var cacheIdentity = new CacheIdentity(cacheKey, expiry);
            var testCacheObject = A.Fake<TestCacheObject>();
            var dataStoreQuery = A.Fake<Func<Task<TestCacheObject>>>();
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult(testCacheObject));

            // Act.
            var result = await cacheLayer.GetAsync(cacheIdentity, dataStoreQuery);

            // Assert.
            result.ShouldNotBe(null);
            result.Key.ShouldBe(testCacheObject.Key);
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustNotHaveHappened();
            A.CallTo(() => dataStoreQuery.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public async Task GivenACacheIdentityWhichExistsInL2Cache_FindItemInCache_ReturnsItemFromL2CacheAndAddsToL1Cache()
        {
            // Arrange.
            var loggingService = A.Fake<ILoggingService>();
            var l1Cache = A.Fake<ICacheProvider>();
            var l2Cache = A.Fake<ICacheProvider>();
            var cacheLayer = new CacheManagerTest(loggingService,
                                                  new[]
                                                  {
                                                      l1Cache,
                                                      l2Cache
                                                  });
            const string cacheKey = "Test";
            var expiry = TimeSpan.FromMinutes(1);
            var cacheIdentity = new CacheIdentity(cacheKey, expiry);
            var testCacheObject = A.Fake<TestCacheObject>();
            var dataStoreQuery = A.Fake<Func<Task<TestCacheObject>>>();
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult<TestCacheObject>(null));
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult(testCacheObject));

            // Act.
            var result = await cacheLayer.GetAsync(cacheIdentity, dataStoreQuery);

            // Assert.
            result.ShouldNotBe(null);
            result.Key.ShouldBe(testCacheObject.Key);
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l1Cache.AddAsync(cacheIdentity.CacheKey, A<TestCacheObject>._, A<TimeSpan>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => dataStoreQuery.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public async Task GivenACacheIdentityWhichDoesntExistInAnyCache_FindItemInCache_ReturnsItemFromDataStoreQueryAndAddsToCaches()
        {
            // Arrange.
            var loggingService = A.Fake<ILoggingService>();
            var l1Cache = A.Fake<ICacheProvider>();
            var l2Cache = A.Fake<ICacheProvider>();
            var cacheLayer = new CacheManagerTest(loggingService,
                                                  new[]
                                                  {
                                                      l1Cache,
                                                      l2Cache
                                                  });
            const string cacheKey = "Test";
            var expiry = TimeSpan.FromMinutes(1);
            var cacheIdentity = new CacheIdentity(cacheKey, expiry);
            var testCacheObject = A.Fake<TestCacheObject>();
            var dataStoreQuery = A.Fake<Func<Task<TestCacheObject>>>();
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult<TestCacheObject>(null));
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult<TestCacheObject>(null));

            // Act.
            var result = await cacheLayer.GetAsync(cacheIdentity, dataStoreQuery);

            // Assert.
            result.ShouldNotBe(null);
            result.Key.ShouldBe(testCacheObject.Key);
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l1Cache.AddAsync(cacheIdentity.CacheKey, A<TestCacheObject>._, A<TimeSpan>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l2Cache.AddAsync(cacheIdentity.CacheKey, A<TestCacheObject>._, A<TimeSpan>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => dataStoreQuery.Invoke()).MustHaveHappened();
        }

        [Fact]
        public async Task GivenACacheIdentityWhichDoesntExistInAnyCacheAndIsNullInDataStore_FindItemInCache_ReturnsNullAndDoesntAddToAnyCaches()
        {
            // Arrange.
            var loggingService = A.Fake<ILoggingService>();
            var l1Cache = A.Fake<ICacheProvider>();
            var l2Cache = A.Fake<ICacheProvider>();
            var cacheLayer = new CacheManagerTest(loggingService,
                                                  new[]
                                                  {
                                                      l1Cache,
                                                      l2Cache
                                                  });
            const string cacheKey = "Test";
            var expiry = TimeSpan.FromMinutes(1);
            var cacheIdentity = new CacheIdentity(cacheKey, expiry);
            var testCacheObject = A.Fake<TestCacheObject>();
            Func<Task<TestCacheObject>> dataStoreQuery = () => Task.FromResult<TestCacheObject>(null);
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult<TestCacheObject>(null));
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).Returns(Task.FromResult<TestCacheObject>(null));

            // Act.
            var result = await cacheLayer.GetAsync(cacheIdentity, dataStoreQuery);

            // Assert.
            result.ShouldBe(null);
            A.CallTo(() => l1Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l2Cache.GetAsync<TestCacheObject>(cacheIdentity.CacheKey)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => l1Cache.AddAsync(cacheIdentity.CacheKey, A<TestCacheObject>._, A<TimeSpan>._)).MustHaveHappened(Repeated.Never);
            A.CallTo(() => l2Cache.AddAsync(cacheIdentity.CacheKey, A<TestCacheObject>._, A<TimeSpan>._)).MustHaveHappened(Repeated.Never);
        }
    }
}