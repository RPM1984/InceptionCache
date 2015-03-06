using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SimpleLogging.Core;

namespace InceptionCache.Core
{
    public abstract class InceptionCacheManager
    {
        private readonly ILoggingService _loggingService;
        private readonly ICacheProvider[] _cacheProviders;

        protected InceptionCacheManager(
            ILoggingService loggingService,
            ICacheProvider[] cacheProviders)
        {
            loggingService.ShouldNotBe(null);
            cacheProviders.ShouldNotBe(null);

            _loggingService = loggingService;
            _cacheProviders = cacheProviders;

            _loggingService.Info("Creating InceptionCache with {0} levels ({1})",
                cacheProviders.Length,
                string.Join(", ", cacheProviders.Select(cacheProvider => cacheProvider.Name)));
        }

        protected async Task<T> FindItemInCacheOrDataStore<T>(
            CacheIdentity cacheIdentity, 
            Func<Task<T>> dataStoreQuery) where T : class
        {
            var cacheProviderMisses = new HashSet<int>();
            T result = null;

            for (var i = 0; i <= _cacheProviders.Length - 1; i++)
            {
                var cacheProvider = _cacheProviders[i];

                _loggingService.Debug("Checking L{0} cache...", i + 1);

                var cached = await cacheProvider.GetAsync<T>(cacheIdentity.CacheKey);
                if (cached == null)
                {
                    _loggingService.Debug("L{0} - MISS", i + 1);
                    cacheProviderMisses.Add(i);
                }
                else
                {
                    _loggingService.Debug("L{0} - HIT", i + 1);
                    result = cached;
                    break;
                }
            }

            if (result == null)
            {
                _loggingService.Debug("Fetching data from data store...");
                result = await dataStoreQuery();
            }

            foreach (var cacheProviderMiss in cacheProviderMisses)
            {
                var cacheProvider = _cacheProviders[cacheProviderMiss];

                var expiryOnThisLevel = new TimeSpan(cacheIdentity.Expiry.Ticks / ((_cacheProviders.Length + 1) - (cacheProviderMiss + 1)));
                _loggingService.Debug("Adding to L{0} cache with an expiry of {1} minutes...", cacheProviderMiss + 1, expiryOnThisLevel.TotalMinutes);
                await cacheProvider.SetAsync(cacheIdentity.CacheKey, result, expiryOnThisLevel);
            }
            
            return result;
        }
    }
}
