using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using InceptionCache.Providers.RedisCacheProvider;
using Shouldly;
using SimpleLogging.Core;
using Xunit;

namespace InceptionCache.Tests
{
    public class RedisCacheProviderFacts
    {
        public class SetFacts : RedisCacheProviderFacts
        {
            private readonly RedisCacheProvider _redis;

            public SetFacts()
            {
                _redis = GetRedisCacheProvider(false); // change to false to use cloud.
            }
            
            private static RedisCacheProvider GetRedisCacheProvider(bool isLocal)
            {
                var loggingService = A.Fake<ILoggingService>();
                return new RedisCacheProvider(isLocal ? 
                    "localhost" : "pub-redis-18660.ap-southeast-2-1.1.ec2.garantiadata.com:18660,ssl=false,password=ic.redis", 
                    loggingService);
            }

            [Fact]
            public async Task GivenASingleItem_AddingToSet_ResultsInASetWithOneItem()
            {
                // Arrange.
                var key = DateTime.Now.Ticks.ToString();
                var item = new TestCacheObject("a");

                // Act.
                await _redis.AddToSetAsync(key, item, TimeSpan.FromMinutes(1));
                
                // Assert.
                var members = await _redis.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(1);
                members.ShouldContain(member => member.Key == item.Key);
            }
            
            [Fact]
            public async Task GivenTwoItems_AddingToSet_ResultsInASetWithTwoItems()
            {
                // Arrange.
                var key = DateTime.Now.Ticks.ToString();
                var items = new[] { new TestCacheObject("a"), new TestCacheObject("b") };

                // Act.
                await _redis.AddToSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await _redis.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(2);
                foreach (var item in items)
                {
                    TestCacheObject item1 = item;
                    members.ShouldContain(member => member.Key == item1.Key);
                }
            }

            [Fact]
            public async Task GivenASetWithTwoItems_RemovingOneItemFromSet_ResultsInASetWithOneItem()
            {
                // Arrange.
                var key = DateTime.Now.Ticks.ToString();
                var itemOne = new TestCacheObject("a");
                var itemTwo = new TestCacheObject("b");
                await _redis.AddToSetAsync(key, new[] { itemOne, itemTwo }, TimeSpan.FromMinutes(1));

                // Act.
                await _redis.DeleteFromSetAsync(key, itemOne, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await _redis.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(1);
                members.ShouldContain(member => member.Key == itemTwo.Key);
                members.ShouldNotContain(member => member.Key == itemOne.Key);
            }

            [Fact]
            public async Task GivenASetWithTwoItems_RemovingTwoItemsFromSet_ResultsInASetWithNoItems()
            {
                // Arrange.
                var key = DateTime.Now.Ticks.ToString();
                var items = new[] { new TestCacheObject("a"), new TestCacheObject("b") };
                await _redis.AddToSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Act.
                await _redis.DeleteFromSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await _redis.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(0);
            }
        }
    }
}
