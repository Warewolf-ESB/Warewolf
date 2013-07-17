using System;
using System.Net;
using System.Reflection;
using Dev2.Studio.Utils;

namespace Dev2.Studio.Core.Helpers
{
    public class VersionChecker : IVersionChecker
    {
        bool _isDone;
        Version _latest;
        Version _current;

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

        static Version GetLatestVersion()
        {
            using(var client = new WebClient())
            {
                try
                {
                    var version = client.DownloadString(StringResources.Warewolf_Version);
                    return new Version(version);
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        #region GetCurrentVersion

        static Version GetCurrentVersion()
        {
            return VersionInfo.FetchVersionInfoAsVersion();
        }

        #endregion
    }
}
