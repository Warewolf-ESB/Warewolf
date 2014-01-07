
namespace Dev2.Studio.Core.TO
{
    public class WebBrowserNavigateRequestTO : IWebBrowserNavigateRequestTO
    {
        public WebBrowserNavigateRequestTO(object dataContext, string uri, string payload)
        {
            DataContext = dataContext;
            Uri = uri;
            Payload = payload;
        }

        public object DataContext { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
    }
}
