using System.Threading.Tasks;
using InceptionCache.Samples.Nancy.Models;

namespace InceptionCache.Samples.Nancy.Services
{
    public interface IDataService
    {
        Task<SampleObject> GetDaDataAsync();
    }
}