using System;
using System.Threading.Tasks;
using InceptionCache.Core;
using InceptionCache.Samples.Nancy.Models;
using SimpleLogging.Core;

namespace InceptionCache.Samples.Nancy.Services
{
    public class DataService : InceptionCacheManager, IDataService
    {
        public DataService(
            ILoggingService loggingService, 
            ICacheProvider[] cacheProviders) : base(loggingService, cacheProviders)
        {
        }

        public async Task<SampleObject> GetDaDataAsync()
        {
            var cacheIdentity = new CacheIdentity("Test-Cache-Key", TimeSpan.FromSeconds(30));
            return await FindItemInCacheOrDataStoreAsync(cacheIdentity, GetFromDataStore);
        }

        private static async Task<SampleObject> GetFromDataStore()
        {
            return await Task.Run(() => new SampleObject("Hi, i'm an object!"));
        }
    }
}