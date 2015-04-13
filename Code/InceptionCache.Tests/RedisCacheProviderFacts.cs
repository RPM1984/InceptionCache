using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using InceptionCache.Core.Serialization;
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
            private static RedisCacheProvider RedisCacheProvider
            {
                get
                {
                    var loggingService = A.Fake<ILoggingService>();
                    return new RedisCacheProvider("localhost", loggingService, new BinarySerializer());
                }
            }

            [Fact]
            public async Task GivenASingleItem_AddingToSet_ResultsInASetWithOneItem()
            {
                // Arrange.
                var key = DateTime.Now.Ticks.ToString();
                var item = new TestCacheObject("a");

                // Act.
                await RedisCacheProvider.AddToSetAsync(key, item, TimeSpan.FromMinutes(1));
                
                // Assert.
                var members = await RedisCacheProvider.GetSetAsync<TestCacheObject>(key);
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
                await RedisCacheProvider.AddToSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await RedisCacheProvider.GetSetAsync<TestCacheObject>(key);
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
                await RedisCacheProvider.AddToSetAsync(key, new[] { itemOne, itemTwo }, TimeSpan.FromMinutes(1));

                // Act.
                await RedisCacheProvider.DeleteFromSetAsync(key, itemOne, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await RedisCacheProvider.GetSetAsync<TestCacheObject>(key);
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
                await RedisCacheProvider.AddToSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Act.
                await RedisCacheProvider.DeleteFromSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await RedisCacheProvider.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(0);
            }
        }
    }
}
