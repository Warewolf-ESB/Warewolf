using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Dev2.Runtime.ServiceModel.Data;
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities
{
    public static class HttpClientCredentialManager
    {
    
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