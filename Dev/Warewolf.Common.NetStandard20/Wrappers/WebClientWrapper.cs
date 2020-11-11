/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Specialized;
using System.Net;
using Warewolf.Common.Interfaces.NetStandard20;

namespace Warewolf.Common.NetStandard20
{
    public class WebClientWrapper : IWebClientWrapper
    {
        private readonly WebClient _webClient;

        public WebClientWrapper()
        {
            _webClient = new WebClient();
        }

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

        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection { }; 

        public ICredentials Credentials { get; set; }

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

        public byte[] DownloadData(string address)
        {
            return _webClient.DownloadData(address);
        }

        public byte[] UploadData(string address, string method, byte[] data)
        {
           return _webClient.UploadData(address, method, data);
        }
    }
}
