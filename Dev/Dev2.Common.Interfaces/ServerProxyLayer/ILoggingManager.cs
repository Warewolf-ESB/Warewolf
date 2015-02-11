using System.ComponentModel;
using System.Net;
using System.Text;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface ILoggingManager
    {
        // set the maximim log size in megabytes
        void SetMaxLogSize(int sizeInMb);
        /// <summary>
        /// get the log file. the dowloadedfilecompleted event arg will fire when the download has completed
        /// </summary>
        /// <returns></returns>
        void GetLog(int sizeInMb);

        /// <summary>
        /// event that tracks progress of download
        /// </summary>
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        /// <summary>
        /// Event that fires when download has completed
        /// </summary>
        event AsyncCompletedEventHandler DownloadFileCompleted;
    }
}