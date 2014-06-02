using System;
using System.ComponentModel;
using System.Net;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
// ReSharper restore CheckNamespace
{
    public interface IDev2WebClient : IDisposable
    {
        string DownloadString(string address);
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event AsyncCompletedEventHandler DownloadFileCompleted;
        bool IsBusy { get; }
        void DownloadFileAsync(Uri address, string fileName, string userToken);
        void CancelAsync();
    }
}