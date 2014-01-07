using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core
{
    public class WebCommunicationResponse : IWebCommunicationResponse
    {
        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public string Content { get; set; }
    }
}
