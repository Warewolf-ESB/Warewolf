using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Factories
{
    public static class WebCommunicationResponseFactory
    {
        public static IWebCommunicationResponse CreateWebCommunicationResponse(string contentType, long contentLength, string content)
        {
            IWebCommunicationResponse webCommunicationResponse = new WebCommunicationResponse();
            webCommunicationResponse.ContentType = contentType;
            webCommunicationResponse.ContentLength = contentLength;
            webCommunicationResponse.Content = content;
            return webCommunicationResponse;
        }
    }
}
