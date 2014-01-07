
namespace Dev2.Studio.Core.TO
{
    public interface IWebBrowserNavigateRequestTO
    {
        object DataContext { get; set; }
        string Uri { get; set; }
        string Payload { get; set; }
    }
}
