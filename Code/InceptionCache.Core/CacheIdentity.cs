using System;
using Shouldly;

namespace InceptionCache.Core
{
    public class CacheIdentity
    {
        public CacheIdentity(string cacheKey, TimeSpan expiry)
        {
            cacheKey.ShouldNotBe(null);
            expiry.ShouldBeGreaterThan(TimeSpan.Zero);

            CacheKey = cacheKey;
            Expiry = expiry;
        }

        public string CacheKey { get; private set; }
        public TimeSpan Expiry { get; private set; }

        public override string ToString()
        {
            return string.Format("Cache Identity - Key: '{0}', Expiry: '{1}' minutes",
                CacheKey,
                Expiry.TotalMinutes);
        }
    }
}