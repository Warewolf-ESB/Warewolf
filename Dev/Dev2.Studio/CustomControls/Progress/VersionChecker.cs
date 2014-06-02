using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Dev2.Common.Wrappers;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.CustomControls.Progress;
using Dev2.Helpers;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Dialogs;
using Dev2.Threading;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
{
    public class VersionChecker : IVersionChecker
    {
        readonly IDev2WebClient _webClient;
        readonly IFile _fileWrapper;
        readonly Func<Version> _versionGetter;
        readonly Func<string, string, List<string>, MessageBoxImage,
                                            MessageBoxResult, string, MessageBoxResult> _showPopup;

        bool _isDone;
        Version _latest;
        Version _current;
        private string _latestVersionCheckSum;

        public VersionChecker()
            : this(new Dev2WebClient(new WebClient()), new FileWrapper(), VersionInfo.FetchVersionInfoAsVersion, Dev2MessageBoxViewModel.ShowWithCustomButtons)
        {
        }

        public VersionChecker(IDev2WebClient webClient,IFile fileWrapper,Func<Version> versionGetter,
            Func<string , string , List<string> , MessageBoxImage ,
                                            MessageBoxResult , string ,MessageBoxResult> showPopup    )
        {
          VerifyArgument.IsNotNull("webClient",webClient);
          VerifyArgument.IsNotNull("fileWrapper", fileWrapper);
          VerifyArgument.IsNotNull("versionGetter", versionGetter);
            _webClient = webClient;
            _fileWrapper = fileWrapper;
            _versionGetter = versionGetter;
            _showPopup = showPopup;
            _isDone = false;
     }

        #region Latest

        public virtual Version Latest
        {
            get
            {
                Check();
                return _latest;
            }
        }

        #endregion

        #region Current

        public virtual Version Current
        {
            get
            {
                Check();
                return _current;
            }
        }

        #endregion

        #region StartPageUri

        public string StartPageUri
        {
            get
            {
                Check();
                return StringResources.Warewolf_Homepage_Start;
            }
        }

        public string LatestVersionCheckSum
        {
            get
            {
                Check();
                return _latestVersionCheckSum;
            }
           
        }

        #endregion

        #region IsLatest?

        public virtual bool IsLatest(IProgressFileDownloader downloader, IProgressDialog progressPopupController, IAsyncWorker asyncWorker)
        {
            var result = true;
            asyncWorker.Start(Check, () =>
            {

                result = PerformDownLoad(downloader);
            });
            
            return result;
        }

        public bool PerformDownLoad(IProgressFileDownloader downloader)
        {
            if (Latest > Current)
            {
                // TESTING : "grepWin-1.6.0-64.msi"
                var latestVersionpath = FileHelper.GetFullPath(string.Format("Installers\\Warewolf-{0}.exe", Latest));
                var path = string.Format("Installers\\tmpWarewolf--{0}.exe", Latest);
                if (!_fileWrapper.Exists(latestVersionpath))
                {
                    var downloadMessageBoxResult = ShowDownloadPopUp();

                    if (downloadMessageBoxResult == MessageBoxResult.Yes || downloadMessageBoxResult == MessageBoxResult.No)
                    {
                        PerformDownload(downloader, path, downloadMessageBoxResult,latestVersionpath);
                    }
                }
                else
                {
                    PerformInstall(downloader, latestVersionpath);
                }
                return true;
            }

            return false;
        }

        private void PerformInstall(IProgressFileDownloader downloader, string path)
        {
            var setupMessageBoxResult = ShowStartNowPopUp();
            if (setupMessageBoxResult == MessageBoxResult.Yes)
            {
                downloader.StartUpdate(path, false);
            }
        }

        private void PerformDownload(IProgressFileDownloader downloader, string path, MessageBoxResult downloadMessageBoxResult, string latestVersionpath)
        {
            FileHelper.CreateDirectoryFromString(path);
            // TESTING : "https://s3-eu-west-1.amazonaws.com/warewolf/Archive/grepWin-1.6.0-64.msi"
            downloader.Download(
                new Uri(string.Format(StringResources.Uri_DownloadPage + "Warewolf-{0}.exe", Latest)), path,
                downloadMessageBoxResult != MessageBoxResult.Yes, latestVersionpath,_latestVersionCheckSum);
        }

        protected virtual MessageBoxResult ShowDownloadPopUp()
        {
            return _showPopup(string.Format(StringResources.DialogBody_UpdateAvailable, Latest), "Update Available", new List<string> { "Download and Install", "Just Download", "Cancel" }, MessageBoxImage.Information, MessageBoxResult.Yes, "Download New Update Prompt");
        }

        protected virtual MessageBoxResult ShowStartNowPopUp()
        {
            return _showPopup(string.Format(StringResources.DialogBody_UpdateReady, Latest), "Update Ready", new List<string> { "Install", "Cancel" }, MessageBoxImage.Information, MessageBoxResult.Yes, "New Update Ready Prompt");
        }

        #endregion

        #region Check

        protected virtual void Check()
        {
            if(!_isDone)
            {
                _isDone = true;
                _latest = GetLatestVersion();
                _current = GetCurrentVersion();
                _latestVersionCheckSum = GetLatestVersionCheckSum();
            }
        }

        private string GetLatestVersionCheckSum()
        {
            try
            {
                return _webClient.DownloadString(StringResources.Warewolf_Checksum);
               
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetLatestVersion

        Version GetLatestVersion()
        {
        
            try
            {
                var version = _webClient.DownloadString(StringResources.Warewolf_Version);
                return new Version(version);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetCurrentVersion

        protected virtual Version GetCurrentVersion()
        {
            return _versionGetter();
        }

        #endregion
    }
}
