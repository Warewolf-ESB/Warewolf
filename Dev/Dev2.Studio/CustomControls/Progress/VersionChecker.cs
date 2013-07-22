using System;
using System.IO;
using System.Net;
using System.Windows;
using Dev2.CustomControls.Progress;
using Dev2.Helpers;
using Dev2.Studio.Utils;

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
            if (webClient == null)
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
                return Latest == Current ? StringResources.Warewolf_Homepage_Take5 : StringResources.Warewolf_Homepage_Start;
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
            ILatestGetter latestGetter = new LatestWebGetter(_webClient);
            latestGetter.GetLatest(StartPageUri, path);

            if (Latest > Current)
            {
                path = FileHelper.GetFullPath(string.Format(/*"Installers\\Warewolf-{0}.exe"*/"Installers\\grepWin-1.6.0-64.msi", Latest));
                result = false;
                if(!File.Exists(path))
                {
                    var downloadMessageBoxResult = ShowDownloadPopUp();
                    if (downloadMessageBoxResult == MessageBoxResult.Yes || downloadMessageBoxResult == MessageBoxResult.No)
                    {
                        FileHelper.CreateDirectoryFromString(path);
                        downloader.Download(new Uri(/*StringResources.Uri_DownloadPage + string.Format("Warewolf-{0}.exe", Latest)*/"https://s3-eu-west-1.amazonaws.com/warewolf/Archive/grepWin-1.6.0-64.msi"), path, downloadMessageBoxResult != MessageBoxResult.Yes);
                    }
                }
                else
                {
                    var setupMessageBoxResult = ShowStartNowPopUp();
                    if (setupMessageBoxResult == MessageBoxResult.Yes)
                    {
                        downloader.StartUpdate(path, false);
                    }
                }
            }
            return result;
        }

        protected virtual MessageBoxResult ShowDownloadPopUp()
        {
            return MessageBox.Show(string.Format(StringResources.DialogBody_UpdateAvailable, Latest), "Update Available", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
        }

        protected virtual MessageBoxResult ShowStartNowPopUp()
        {
            return MessageBox.Show(string.Format("A new version of Warewolf has already been downloaded:\r\n\r\nWarewolf-{0}.exe\r\n\r\nWould you like to start the setup now?", Latest), "Update Finished Downloading", MessageBoxButton.YesNo, MessageBoxImage.Information);
        }

        #endregion

        #region Check

        protected virtual void Check()
        {
            if (!_isDone)
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
