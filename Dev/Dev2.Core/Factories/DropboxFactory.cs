using System;
using System.Net.Http;
using Dev2.Common;
using Dev2.Common.Interfaces.Dropbox;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dropbox.Api;

namespace Dev2.Factories
{
    public class DropboxFactory : IDropboxFactory
    {
        public IDropboxClientWrapper CreateWithSecret(string secret)
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            var client = new DropboxClient(secret, new DropboxClientConfig(GlobalConstants.UserAgentString) { HttpClient = httpClient });
            return new DropboxClientWrapper(client);
        }
    }
}
