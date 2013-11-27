using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;

namespace Dev2.CustomControls.Progress
{
    public class ProgressFileDownloader : IProgressFileDownloader
    {
        readonly IDev2WebClient _webClient;
        protected readonly IProgressDialog _progressDialog;
        protected bool DontStartUpdate;

        #region CTOR

        public ProgressFileDownloader(Window owner)
            : this(new Dev2WebClient(new WebClient()), new ProgressDialog(owner))
        {
        }

        public ProgressFileDownloader(IDev2WebClient webClient, IProgressDialog progressDialog)
        {
            VerifyArgument.IsNotNull("progressDialog", progressDialog);
            VerifyArgument.IsNotNull("webClient", webClient);

            _webClient = webClient;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;

            _progressDialog = progressDialog;
            _progressDialog.CancelClick += (sender, args) => Cancel();
            _progressDialog.Closing += (sender, args) => args.Cancel = _webClient.IsBusy;

            DontStartUpdate = false;
        }

        #endregion

        #region Download

        /// <summary>
        /// Downloads the resource at the URI specified by in the address parameter. 
        /// When the download completes successfully, the downloaded file is named fileName on the local computer.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="fileName"></param>
        public void Download(Uri address, string fileName, bool dontStartUpdate)
        {
            DontStartUpdate = dontStartUpdate;
            _webClient.DownloadFileAsync(address, fileName, fileName);
            _progressDialog.ShowDialog();
        }

        #endregion

        #region Cancel

        protected void Cancel()
        {
            _progressDialog.SubLabel = "Please wait while the process is being cancelled...";
            _progressDialog.IsCancelButtonEnabled = false;
            _webClient.CancelAsync();
            _progressDialog.Close();
        }

        #endregion

        #region OnDownloadProgressChanged

        void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            RehydrateDialog((string)args.UserState, args.ProgressPercentage, args.TotalBytesToReceive);
        }

        protected void RehydrateDialog(string fileName, int progressPercent, long totalBytes)
        {
            var file = Path.GetFileName(fileName);
            _progressDialog.Label = string.Format("{0} downloaded {1}% of {2:0} KB", file, progressPercent, totalBytes / 1024);
            _progressDialog.ProgressValue = progressPercent;
        }

        #endregion

        #region OnDownloadFileCompleted

        void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
        {
            StartUpdate((string)args.UserState, args.Cancelled);
        }

        public void StartUpdate(string fileName, bool cancelled)
        {
            _progressDialog.Close();
            if (!cancelled && !DontStartUpdate)
            {
                Application.Current.Shutdown();
                Process.Start(fileName);
            }
        }

        #endregion
    }
}
