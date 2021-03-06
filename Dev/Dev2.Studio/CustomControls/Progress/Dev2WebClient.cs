/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Net;
using Dev2.Studio.Core.Helpers;

namespace Dev2.Helpers
{
    class Dev2WebClient : IDev2WebClient
    {
        readonly WebClient _webClient;

        public Dev2WebClient(WebClient webClient)
        {
            _webClient = webClient;
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
        
        public bool IsBusy { get; private set; }
        

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
