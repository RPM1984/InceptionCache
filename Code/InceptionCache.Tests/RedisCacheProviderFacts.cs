﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using InceptionCache.Core;
using InceptionCache.Providers.RedisCacheProvider;
using Shouldly;
using SimpleLogging.Core;
using Xunit;

namespace InceptionCache.Tests
{
    public class RedisCacheProviderFacts
    {
        private readonly RedisCacheProvider _redis;

        protected RedisCacheProviderFacts()
        {
            _redis = GetRedisCacheProvider(false); // change to false to use cloud.
        }

        protected static RedisCacheProvider GetRedisCacheProvider(bool isLocal)
        {
            var loggingService = A.Fake<ILoggingService>();
            return new RedisCacheProvider(isLocal
                                              ? "localhost"
                                              : "catfish.redistogo.com:10856,ssl=false,password=8a7901dbbf0fd52888e26ac777683c53",
                                          loggingService,
                                          new LoggingOptions());
        }

        public class DeleteAsyncTests : RedisCacheProviderFacts
        {
            [Fact]
            public async Task GivenACacheWithMultipleKeys_DeleteAsync_RemovesMultipleKeys()
            {
                // Arrange.
                var keysAndValues = new Dictionary<string, string>
                {
                    {"key1", "val"},
                    {"key2", "val"}
                };
                await _redis.AddAsync(keysAndValues, TimeSpan.FromMinutes(1));
                var keys = await _redis.GetAsync<string>(keysAndValues.Keys.ToArray());
                keys.ShouldNotBeNull();
                keys.Length.ShouldBe(keysAndValues.Count);

                // Act.
                await _redis.DeleteAsync(keysAndValues.Keys.ToArray());

                // Assert.
                keys = await _redis.GetAsync<string>(keysAndValues.Keys.ToArray());
                keys.ShouldNotBeNull();
                keys.ShouldBeEmpty();
            }
        }

        public class SetFacts : RedisCacheProviderFacts
        {
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
                var items = new[]
                {
                    new TestCacheObject("a"),
                    new TestCacheObject("b")
                };

                // Act.
                await _redis.AddToSetAsync(key, items, TimeSpan.FromMinutes(1));

                // Assert.
                var members = await _redis.GetSetAsync<TestCacheObject>(key);
                members.ShouldNotBe(null);
                members.Count().ShouldBe(2);
                foreach (var item in items)
                {
                    var item1 = item;
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
                await _redis.AddToSetAsync(key,
                                           new[]
                                           {
                                               itemOne,
                                               itemTwo
                                           },
                                           TimeSpan.FromMinutes(1));

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
                var items = new[]
                {
                    new TestCacheObject("a"),
                    new TestCacheObject("b")
                };
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
                var keyAItems = new[]
                {
                    new TestCacheObject("a"),
                    new TestCacheObject("b")
                };
                var keyBItems = new[]
                {
                    new TestCacheObject("c"),
                    new TestCacheObject("d")
                };
                await _redis.AddToSetAsync(keyA, keyAItems, TimeSpan.FromMinutes(1));
                await _redis.AddToSetAsync(keyB, keyBItems, TimeSpan.FromMinutes(1));

                // Act.
                var setsAndMembers = await _redis.GetSetsAsync<TestCacheObject>(new[]
                {
                    keyA,
                    keyB
                });

                // Assert.
                setsAndMembers.ShouldNotBe(null);
                setsAndMembers.Keys.Count.ShouldBe(2);
                setsAndMembers.ShouldContain(member => member.Key == keyA);
                setsAndMembers.ShouldContain(member => member.Key == keyB);
                foreach (var keyAItem in keyAItems)
                {
                    var item = keyAItem;
                    setsAndMembers[keyA].ShouldContain(member => member.Key == item.Key);
                }
                foreach (var keyBItem in keyBItems)
                {
                    var item = keyBItem;
                    setsAndMembers[keyB].ShouldContain(member => member.Key == item.Key);
                }
            }

            [Fact]
            public async Task GivenTwoSetsEachWithTwoItems_AddToSetsAsync_ResultsInTwoSetsEachWithTwoItems()
            {
                // Arrange.
                const string keyA = "a";
                const string keyB = "b";
                var keyAItems = new[]
                {
                    new TestCacheObject("a"),
                    new TestCacheObject("b")
                };
                var keyBItems = new[]
                {
                    new TestCacheObject("c"),
                    new TestCacheObject("d")
                };
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
                var setsAndMembers = await _redis.GetSetsAsync<TestCacheObject>(new[]
                {
                    keyA,
                    keyB
                });
                setsAndMembers.ShouldNotBe(null);
                setsAndMembers.Keys.Count.ShouldBe(2);
                setsAndMembers.ShouldContain(member => member.Key == keyA);
                setsAndMembers.ShouldContain(member => member.Key == keyB);
                foreach (var keyAItem in keyAItems)
                {
                    var item = keyAItem;
                    setsAndMembers[keyA].ShouldContain(member => member.Key == item.Key);
                }
                foreach (var keyBItem in keyBItems)
                {
                    var item = keyBItem;
                    setsAndMembers[keyB].ShouldContain(member => member.Key == item.Key);
                }
            }
        }
    }
}