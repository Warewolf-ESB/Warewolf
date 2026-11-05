using System.Net.Http;
using System.Threading.Tasks;

namespace AzureFunctions.ProfilerServices.Domain.Interfaces
{
    public interface IProfilerHttpService
    {
        Task<HttpResponseMessage> GetAsync(string requestUrl);
    }
}
