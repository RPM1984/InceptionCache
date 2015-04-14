using System;
using System.Collections.Generic;
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
            private readonly RedisCacheProvider _redis = new RedisCacheProvider("localhost", A.Fake<ILoggingService>());

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

            [Fact]
            public async Task GivenTwoSetsWithTwoItems_GetSetsAsync_ReturnsTwoSetsWithTwoItems()
            {
                // Arrange.
                const string keyA = "a";
                const string keyB = "b";
                var keyAItems = new[] { new TestCacheObject("a"), new TestCacheObject("b") };
                var keyBItems = new[] { new TestCacheObject("c"), new TestCacheObject("d") };
                await _redis.AddToSetAsync(keyA, keyAItems, TimeSpan.FromMinutes(1));
                await _redis.AddToSetAsync(keyB, keyBItems, TimeSpan.FromMinutes(1));

                // Act.
                var setsAndMembers = await _redis.GetSetsAsync<TestCacheObject>(new[] { keyA, keyB });

                // Assert.
                setsAndMembers.ShouldNotBe(null);
                setsAndMembers.Keys.Count.ShouldBe(2);
                setsAndMembers.ShouldContain(member => member.Key == keyA);
                setsAndMembers.ShouldContain(member => member.Key == keyB);
                foreach (var keyAItem in keyAItems)
                {
                    TestCacheObject item = keyAItem;
                    setsAndMembers[keyA].ShouldContain(member => member.Key == item.Key);
                }
                foreach (var keyBItem in keyBItems)
                {
                    TestCacheObject item = keyBItem;
                    setsAndMembers[keyB].ShouldContain(member => member.Key == item.Key);
                }
            }

            [Fact]
            public async Task GivenTwoSetsEachWithTwoItems_AddToSetsAsync_ResultsInTwoSetsEachWithTwoItems()
            {
                // Arrange.
                const string keyA = "a";
                const string keyB = "b";
                var keyAItems = new[] { new TestCacheObject("a"), new TestCacheObject("b") };
                var keyBItems = new[] { new TestCacheObject("c"), new TestCacheObject("d") };
                var sets = new Dictionary<string, TestCacheObject[]>
                {
                    {keyA, keyAItems},
                    {keyB, keyBItems}
                };
                var setsAndExpiries = new Dictionary<string, TimeSpan?>
                {
                    {keyA, TimeSpan.FromMinutes(1)},
                    {keyB, TimeSpan.FromMinutes(1)}
                };

                // Act.
                await _redis.AddToSetsAsync(sets, setsAndExpiries);
                
                // Assert.
                var setsAndMembers = await _redis.GetSetsAsync<TestCacheObject>(new[] { keyA, keyB });
                setsAndMembers.ShouldNotBe(null);
                setsAndMembers.Keys.Count.ShouldBe(2);
                setsAndMembers.ShouldContain(member => member.Key == keyA);
                setsAndMembers.ShouldContain(member => member.Key == keyB);
                foreach (var keyAItem in keyAItems)
                {
                    TestCacheObject item = keyAItem;
                    setsAndMembers[keyA].ShouldContain(member => member.Key == item.Key);
                }
                foreach (var keyBItem in keyBItems)
                {
                    TestCacheObject item = keyBItem;
                    setsAndMembers[keyB].ShouldContain(member => member.Key == item.Key);
                }
            }
        }
    }
}
