using System.Configuration;
using System.Runtime.Caching;
using InceptionCache.Core;
using InceptionCache.Core.Serialization;
using InceptionCache.Providers.RedisCacheProvider;
using InceptionCache.Samples.Nancy.Models;
using InceptionCache.Samples.Nancy.Services;
using Nancy;
using Nancy.TinyIoc;
using SimpleLogging.Core;
using InceptionCache.Providers.InMemoryCacheProvider;

namespace InceptionCache.Samples.Nancy
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var loggingService = new StringBuilderLoggingService();
            container.Register<ILoggingService>((c, p) => loggingService);

            // Cache Level 1 - In Memory
            ICacheProvider l1Cache = new InMemoryCacheProvider(
                new MemoryCache("In-Memory Cache"),
                loggingService);

            // Cache Level 2 - Redis
            ICacheProvider l2Cache = new RedisCacheProvider(
                ConfigurationManager.AppSettings["RedisHost"],
                loggingService, 
                new BinarySerializer());
            l2Cache.Delete("Test-Cache-Key");

            container.Register<IDataService>(new DataService(
                loggingService,
                new[] { l1Cache, l2Cache }));
        }
    }
}