
using System;
using System.Collections.Specialized;
using System.Net;

namespace Dev2.Web2.Controllers
{
    public interface IWebClientFactory
    {
        IWebClientWrapper New(string userName, string password);
    }

    public class WebClientFactory : IWebClientFactory
    {
        public IWebClientWrapper New(string userName, string password)
        {
            return new WebClientWrapper(userName, password);
        }
    }

    public interface IWebClientWrapper : IDisposable
    {
        byte[] UploadValues(string wareWolfResumeUrl, string method, NameValueCollection nameValueCollection);
    }

    public class WebClientWrapper : IWebClientWrapper
    {
        private readonly WebClient _webClient;

        public WebClientWrapper(string userName, string password)
        {
            _webClient = new WebClient
            {
                Credentials = new NetworkCredential(userName, password)
            };
        }

        public byte[] UploadValues(string wareWolfResumeUrl, string method, NameValueCollection nameValueCollection)
        {
            return _webClient.UploadValues(wareWolfResumeUrl, method, nameValueCollection);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _webClient.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}