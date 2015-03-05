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
            L1CacheExpiry = new TimeSpan(expiry.Ticks / 2);
            L2CacheExpiry = expiry;
        }

        public string CacheKey { get; private set; }
        public TimeSpan L1CacheExpiry { get; private set; }
        public TimeSpan L2CacheExpiry { get; private set; }

        public override string ToString()
        {
            return string.Format("Cache Identity - Key: '{0}', L1 Expiry: '{1}' minutes, L2 Expiry: '{2}' minutes",
                CacheKey,
                L1CacheExpiry.TotalMinutes,
                L2CacheExpiry.TotalMinutes);
        }
    }
}