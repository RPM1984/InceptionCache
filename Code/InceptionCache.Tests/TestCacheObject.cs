using Shouldly;

namespace InceptionCache.Tests
{
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