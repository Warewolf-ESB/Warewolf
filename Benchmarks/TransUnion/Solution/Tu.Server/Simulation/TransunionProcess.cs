using System.Configuration;
using System.Net;

namespace Tu.Simulation
{
    public interface ITransunionProcess
    {
        void Run();
    }

    public class TransunionProcess : ITransunionProcess
    {

        public void Run()
        {
            var webClient = new WebClient();
            var result = webClient.DownloadString(ConfigurationManager.AppSettings["TransunionProcessUri"]);
        }
    }
}