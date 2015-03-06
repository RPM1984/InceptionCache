using InceptionCache.Samples.Nancy.Models;
using InceptionCache.Samples.Nancy.Services;
using Nancy;

namespace InceptionCache.Samples.Nancy.Modules
{
    public class HomeModule : NancyModule
    {
        private readonly IDataService _dataService;
        private readonly StringBuilderLoggingService _loggingService;

        public HomeModule(
            IDataService dataService, 
            StringBuilderLoggingService loggingService)
        {
            _dataService = dataService;
            _loggingService = loggingService;

            Get["/"] = _ => View["Index", DoALittleDance()];
        }

        private string DoALittleDance()
        {
            _dataService.GetDaDataAsync();
            return _loggingService.GetLog;
        }
    }
}