using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Dev2.Common.Utils;
using Dev2.Common.Wrappers;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;
namespace Dev2.CustomControls.Progress
{
    public class ProgressFileDownloader : IProgressFileDownloader
    {
        readonly IDev2WebClient _webClient;
        protected readonly IProgressNotifier ProgressDialog;
        private readonly IFile _file;
        private readonly ICryptoProvider _cryptoProvider;
        private bool _dontStartUpdate;
        private readonly Window _owner;
        private string _tmpFileName = "";

        #region Properties

        public bool IsBusyDownloading { get; private set; }

        #endregion

        #region CTOR
        [ExcludeFromCodeCoverage]
        public ProgressFileDownloader(Window owner) // cant cover this because a window needs to be shown to be a parent of something else. 
            : this(new Dev2WebClient(new WebClient()), new FileWrapper(), new CryptoProvider(new SHA256CryptoServiceProvider()))
        {
            _owner = owner;
        }

        public static Func<Window, Action, IProgressNotifier> GetProgressDialogViewModel = (owner, cancelAction) => DialogViewModel(owner, cancelAction);

        
        [ExcludeFromCodeCoverage]
        static IProgressNotifier DialogViewModel(Window owner, Action cancelAction)
        {
            var dialog = new ProgressDialog(owner);
            dialog.Closed += (sender, args) => cancelAction();
            var dialogViewModel = new ProgressDialogViewModel(cancelAction, dialog.Show, dialog.Close);
            dialog.DataContext = dialogViewModel;
            return dialogViewModel;
        }

        public ProgressFileDownloader(IDev2WebClient webClient, IFile file, ICryptoProvider cryptoProvider)
        {
            VerifyArgument.IsNotNull("webClient", webClient);
            VerifyArgument.IsNotNull("file", file);
            VerifyArgument.IsNotNull("cryptoProvider", cryptoProvider);
            _webClient = webClient;

            ProgressDialog = GetProgressDialogViewModel(_owner, Cancel);
            _file = file;
            _cryptoProvider = cryptoProvider;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _dontStartUpdate = false;
            ShutDownAction = ShutdownAndInstall;

        }

        public static void PerformCleanup(IDirectory dir, string path, IFile file)
        {
            try
            {
                foreach(var v in dir.GetFiles(path).Where(a => a.Contains("tmp")))
                    file.Delete(v);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //best effort.
            }

        }

        #endregion

        #region Download

        /// <summary>
        /// Downloads the resource at the URI specified by in the address parameter. 
        /// When the download completes successfully, the downloaded file is named fileName on the local computer.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="tmpFileName"></param>
        /// <param name="dontStartUpdate"></param>
        /// <param name="fileName"></param>
        /// <param name="checkSum"></param>
        public void Download(Uri address, string tmpFileName, bool dontStartUpdate, string fileName, string checkSum)
        {
            _tmpFileName = tmpFileName;
            if(_file.Exists(_tmpFileName))
            {
                _file.Delete(_tmpFileName);
            }

            _dontStartUpdate = dontStartUpdate;
            _webClient.DownloadFileAsync(address, tmpFileName, tmpFileName);
            _webClient.DownloadFileCompleted += (o, args) =>
                {

                    if(!args.Cancelled && null == args.Error && PerformCheckSum(tmpFileName, checkSum))
                    {

                        _file.Move(tmpFileName, fileName);
                        OnDownloadFileCompleted(args, fileName);
                    }
                    else
                    {
                        _file.Delete(tmpFileName);
                        ProgressDialog.Close();

                    }



                };

            ProgressDialog.Show();
            IsBusyDownloading = true;
        }



        public bool PerformCheckSum(string tmpFileName, string checkSum)
        {
            StringBuilder sb = new StringBuilder();
            using(var stream = _file.Open(tmpFileName, FileMode.Open))
            {
                var hash = _cryptoProvider.ComputeHash(stream);
                foreach(var b in hash)
                {
                    sb.Append(b);
                }
                return checkSum == sb.ToString();

            }
        }

        #endregion

        #region Cancel

        public void Cancel()
        {
            _webClient.CancelAsync();
            ProgressDialog.Close();
            IsBusyDownloading = false;
            if(_file.Exists(_tmpFileName))
            {
                _file.Delete(_tmpFileName);
            }
        }

        #endregion

        #region OnDownloadProgressChanged
        [ExcludeFromCodeCoverage] // cant test this because the DownloadProgressChangedEventArgs has no public ctor
        void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {

            RehydrateDialog((string)args.UserState, args.ProgressPercentage, args.TotalBytesToReceive);
        }

        protected void RehydrateDialog(string fileName, int progressPercent, long totalBytes)
        {
            ProgressDialog.StatusChanged(fileName, progressPercent, totalBytes);
        }

        #endregion

        #region OnDownloadFileCompleted

        void OnDownloadFileCompleted(AsyncCompletedEventArgs args, string fileName)
        {
            StartUpdate(fileName, args.Cancelled);
        }

        public void StartUpdate(string fileName, bool cancelled)
        {
            ProgressDialog.Close();
            IsBusyDownloading = false;
            if(!cancelled && !_dontStartUpdate)
            {
                ShutDownAction(fileName);
            }
        }

        public Action<string> ShutDownAction { get; set; }

        [ExcludeFromCodeCoverage]
        static void ShutdownAndInstall(string fileName)
        {
            Application.Current.Shutdown();
            Process.Start(fileName);
        }

        #endregion

        #region Implementation of IDisposable



        #endregion
    }
}
