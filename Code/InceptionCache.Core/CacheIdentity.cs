using System;
using Shouldly;

namespace InceptionCache.Core
{
    public class CacheIdentity
    {
        public CacheIdentity(string cacheKey,
                             TimeSpan expiry)
        {
            cacheKey.ShouldNotBe(null);
            expiry.ShouldBeGreaterThan(TimeSpan.Zero);

            CacheKey = cacheKey;
            Expiry = expiry;
        }

        public string CacheKey { get; }
        public TimeSpan Expiry { get; }

        public override string ToString()
        {
            return $"Cache Identity - Key: '{CacheKey}', Expiry: '{Expiry.TotalMinutes}' minutes";
        }
    }
}