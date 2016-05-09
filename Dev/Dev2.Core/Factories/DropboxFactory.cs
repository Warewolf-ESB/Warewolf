using System;
using System.Net.Http;
using Dev2.Common;
using Dev2.Common.Interfaces.Dropbox;
using Dropbox.Api;

namespace Dev2.Factories
{
    public class DropboxFactory : IDropboxFactory
    {
        public DropboxClient Create()
        {
            //return new DropboxClient(GlobalConstants.DropBoxApiKey);       
            return new DropboxClient("31qf750f1vzffhu", new DropboxClientConfig(GlobalConstants.UserAgentString));
        }
        public DropboxClient CreateWithSecret(string secret)
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            return new DropboxClient(secret, new DropboxClientConfig(GlobalConstants.UserAgentString) { HttpClient = httpClient });
        }


    }
}
