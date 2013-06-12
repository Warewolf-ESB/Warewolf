using Dev2.Studio.Core.TO;

namespace Dev2.Studio.Core.Messages
{
    public class UpdateWebpagePreviewMessage:IMessage
    {
        public WebBrowserNavigateRequestTO WebBrowserNavigateRequestTo { get; set; }

        public UpdateWebpagePreviewMessage(WebBrowserNavigateRequestTO webBrowserNavigateRequestTo)
        {
            WebBrowserNavigateRequestTo = webBrowserNavigateRequestTo;
        }
    }
}