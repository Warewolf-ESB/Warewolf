
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IWebCommunicationResponse
    {
        string ContentType { get; set; }
        long ContentLength { get; set; }
        string Content { get; set; }
    }
}
