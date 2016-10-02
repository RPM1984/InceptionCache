using System;

namespace InceptionCache.Core
{
    public class LoggingOptions
    {
        public bool LogErrors { get; set; } = true;
        public bool LogCacheHits { get; set; } = false;
        public bool LogCacheMisses { get; set; } = false;
        public TimeSpan? LogCacheStatsSummaryAfter { get; set; }
    }
}
