
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IWebCommunication
    {
        IWebCommunicationResponse Get(string uri);
        IWebCommunicationResponse Post(string uri, string data);
    }
}
