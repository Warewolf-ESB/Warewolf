using System.Threading.Tasks;
using Dev2.Hosting;

namespace Dev2.Services
{
    public interface IPushService
    {
        Task<string> ProcessRequest(IHostContext context, string jsonObj);
    }
}
