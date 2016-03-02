using System.Activities;
using System.Net;
using System.Net.Http;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Activities
{
    public interface IWebRequestData
    {
        WebRequestMethod WebRequestMethod { get; set; }
        string DisplayName { get; set; }
        InArgument<string> Type { set; get; }
    }
    public class WebRequestDataDto : IWebRequestData
    {
        private WebRequestDataDto()
        {

        }
        public static WebRequestDataDto CreateRequestDataDto(WebRequestMethod requestMethod, InArgument<string> type, string displayName)
        {
            return new WebRequestDataDto()
            {
                WebRequestMethod = requestMethod,
                DisplayName = displayName,
                Type = type
            };
        }

        public WebRequestMethod WebRequestMethod { get; set; }
        public string DisplayName { get; set; }
        public InArgument<string> Type { get; set; }
    }

    public class HttpClientCredentialManager
    {
        private HttpClientCredentialManager()
        {

        }

        public static HttpClientHandler SetCredentialOnHandler(WebSource source, HttpClientHandler httpClientHandler)
        {
            if (source.AuthenticationType == AuthenticationType.User && httpClientHandler != null)
            {
                httpClientHandler.Credentials = new NetworkCredential(source.UserName, source.Password);
                return httpClientHandler;
            }
            return httpClientHandler;
        }
    }
}