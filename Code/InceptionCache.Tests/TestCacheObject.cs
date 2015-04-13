using System;
using Shouldly;

namespace InceptionCache.Tests
{
    [Serializable]
    public class TestCacheObject
    {
        public TestCacheObject(string key)
        {
            key.ShouldNotBe(null);
            Key = key;
        }

        public string Key { get; private set; }
    }
}