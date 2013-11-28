using System.Net.Http;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationResponse
    {
        HttpResponseMessage Response { get; }
    }

}
