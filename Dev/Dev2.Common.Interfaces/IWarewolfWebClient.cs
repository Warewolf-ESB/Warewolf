
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Common.Interfaces
{
    public interface IWarewolfWebClient : IDisposable
    {
        string DownloadString(string address);
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event AsyncCompletedEventHandler DownloadFileCompleted;
        bool IsBusy { get; }
        void DownloadFileAsync(Uri address, string fileName, string userToken);
        void CancelAsync();

        Task<string> DownloadStringAsync(string address);
    }
}
