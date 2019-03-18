#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Studio.Utils;
using Warewolf.Core;


namespace Dev2.Studio.Core.Helpers
{
    public class VersionChecker : IVersionChecker
    {
        readonly IWarewolfWebClient _webClient;
        readonly Func<Version> _versionGetter;

        bool _isDone;
        Version _latest;
        Version _current;

        public VersionChecker()
            : this(new WarewolfWebClient(new WebClient()), VersionInfo.FetchVersionInfoAsVersion)
        {
        }

        public VersionChecker(IWarewolfWebClient webClient, Func<Version> versionGetter)
        {
            VerifyArgument.IsNotNull("webClient", webClient);
            VerifyArgument.IsNotNull("versionGetter", versionGetter);
            _webClient = webClient;
            _versionGetter = versionGetter;
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

        public string CommunityPageUri
        {
            get
            {
                Check();
                return StringResources.Uri_Community_HomePage;
            }
        }

        #endregion

        #region IsLatest?

        public async Task<bool> GetNewerVersionAsync()
        {
            var latest = await GetLatestVersionAsync();
            return latest > Current;
        }

        #endregion

        #region Check

        protected void Check()
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

        async Task<Version> GetLatestVersionAsync()
        {

            try
            {
                var version = await _webClient.DownloadStringAsync(InstallerResources.WarewolfVersion);
                return new Version(version);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Version GetLatestVersion()
        {

            try
            {
                var version = _webClient.DownloadString(InstallerResources.WarewolfVersion);
                return new Version(version);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region GetCurrentVersion

        protected virtual Version GetCurrentVersion() => _versionGetter();

        #endregion
    }

    class InstallerResources
    {
        protected InstallerResources()
        {
        }

        public static bool InstallerTesting => ConfigurationManager.AppSettings["InstallerTesting"] == null || bool.Parse(ConfigurationManager.AppSettings["InstallerTesting"]);
        public static string WarewolfVersion => InstallerTesting ? ConfigurationManager.AppSettings["TestVersionLocation"] : ConfigurationManager.AppSettings["VersionLocation"];
        public static string WarewolfChecksum => InstallerTesting ? ConfigurationManager.AppSettings["TestCheckSumLocation"] : ConfigurationManager.AppSettings["CheckSumLocation"];
    }
}
