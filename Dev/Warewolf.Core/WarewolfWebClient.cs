/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;

namespace Warewolf.Core
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
            using (var client = _webClient)
            {
                if (!String.IsNullOrEmpty(address))
                {
                    return client.DownloadString(address);
                }
            }
            return null;
        }

        public async Task<string> DownloadStringAsync(string address)
        {
            using (var client = _webClient)
            {
                if (!String.IsNullOrEmpty(address))
                {
                    var result = await client.DownloadStringTaskAsync(address);
                    return result;
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
