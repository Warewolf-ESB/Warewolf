using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using Dev2.CustomControls.Progress;
using Dev2.Helpers;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Dialogs;

namespace Dev2.Studio.Core.Helpers
{
    public class VersionChecker : IVersionChecker
    {
        readonly IDev2WebClient _webClient;
        bool _isDone;
        Version _latest;
        Version _current;

        public VersionChecker()
            : this(new Dev2WebClient(new WebClient()))
        {
        }

        public VersionChecker(IDev2WebClient webClient)
        {
            if(webClient == null)
            {
                throw new ArgumentNullException("webClient");
            }
            _webClient = webClient;
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
                return Latest == Current ? StringResources.Warewolf_Homepage_New : StringResources.Warewolf_Homepage_Start;
            }
        }

        #endregion

        #region IsLatest?

        public virtual bool IsLatest(IProgressFileDownloader downloader, IProgressDialog progressPopupController)
        {
            var result = true;
            var path = FileHelper.GetAppDataPath(StringResources.Uri_Studio_Homepage);

            // PBI 9512 - 2013.06.07 - TWR: added
            // PBI 9941 - 2013.07.07 - TWR: modified
            using(var latestGetter = new LatestWebGetter(_webClient))
            {
                latestGetter.GetLatest(StartPageUri, path);
            }

            if(Latest > Current)
            {
                // TESTING : "grepWin-1.6.0-64.msi"
                path = FileHelper.GetFullPath(string.Format("Installers\\" + "Warewolf-{0}.exe", Latest));
                result = false;
                if(!File.Exists(path))
                {
                    var downloadMessageBoxResult = ShowDownloadPopUp();
                    if(downloadMessageBoxResult == MessageBoxResult.Yes || downloadMessageBoxResult == MessageBoxResult.No)
                    {
                        FileHelper.CreateDirectoryFromString(path);
                        // TESTING : "https://s3-eu-west-1.amazonaws.com/warewolf/Archive/grepWin-1.6.0-64.msi"
                        downloader.Download(new Uri(string.Format(StringResources.Uri_DownloadPage + "Warewolf-{0}.exe", Latest)), path, downloadMessageBoxResult != MessageBoxResult.Yes);
                    }
                }
                else
                {
                    var setupMessageBoxResult = ShowStartNowPopUp();
                    if(setupMessageBoxResult == MessageBoxResult.Yes)
                    {
                        downloader.StartUpdate(path, false);
                    }
                }
            }
            return result;
        }

        protected virtual MessageBoxResult ShowDownloadPopUp()
        {
            return Dev2MessageBoxViewModel.ShowWithCustomButtons(string.Format(StringResources.DialogBody_UpdateAvailable, Latest), "Update Available", new List<string>() { "Download and Install", "Just Download", "Cancel" }, MessageBoxImage.Information, MessageBoxResult.Yes, "Download New Update Prompt");
        }

        protected virtual MessageBoxResult ShowStartNowPopUp()
        {
            return Dev2MessageBoxViewModel.ShowWithCustomButtons(string.Format(StringResources.DialogBody_UpdateReady, Latest), "Update Ready", new List<string>() { "Install", "Cancel" }, MessageBoxImage.Information, MessageBoxResult.Yes, "New Update Ready Prompt");
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
            return VersionInfo.FetchVersionInfoAsVersion();
        }

        #endregion
    }
}
