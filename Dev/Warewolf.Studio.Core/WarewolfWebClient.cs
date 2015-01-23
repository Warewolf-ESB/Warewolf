using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Core
{
    public class WarewolfWebClient : IWarewolfWebClient
    {
        readonly WebClient _webClient;

        public WarewolfWebClient(WebClient webClient)
        {
            _webClient = webClient;
        }

        public string DownloadString(string address)
        {
            using(var client = _webClient)
            {
                if(!String.IsNullOrEmpty(address))
                {
                    return client.DownloadString(address);
                }
            }
            return null;
        }
        
        public async Task<string> DownloadStringAsync(string address)
        {
            using(var client = _webClient)
            {
                if(!String.IsNullOrEmpty(address))
                {
                    await client.DownloadStringTaskAsync(address);
                }
            }
            return null;
        }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged
        {
            add
            {
                _webClient.DownloadProgressChanged += value;
            }
            remove
            {
                _webClient.DownloadProgressChanged -= value;
            }
        }
        public event AsyncCompletedEventHandler DownloadFileCompleted
        {
            add
            {
                _webClient.DownloadFileCompleted += value;
            }
            remove
            {
                _webClient.DownloadFileCompleted -= value;
            }
        }
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public bool IsBusy { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public void DownloadFileAsync(Uri address, string fileName, string userToken)
        {
            _webClient.DownloadDataCompleted+=_webClient_DownloadDataCompleted;
            _webClient.DownloadFileAsync(address, fileName, userToken);
          
        }

        private void _webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            // check version 
        }

        public void CancelAsync()
        {
            _webClient.CancelAsync();
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _webClient.Dispose();
        }

        #endregion
    }
}