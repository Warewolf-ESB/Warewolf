using System;
using System.Windows;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Utils;

namespace Dev2.Core.Tests.Helpers
{
    public class VersionCheckerTestClass : VersionChecker
    {
        public int ShowPopUpHitCount = 0;
        public VersionCheckerTestClass(IDev2WebClient webClient)
            : base(webClient, VersionInfo.FetchVersionInfoAsVersion)
        {
        }

        public VersionCheckerTestClass(IDev2WebClient webClient, Func<Version> func)
            : base(webClient, func)
        {
        }

        public Version CurrentVersion { get; set; }

        public MessageBoxResult ShowPopupResult { get; set; }

        public MessageBoxResult StartNowResult
        {
            get;
            set;
        }

        protected int ShowStartHitCount
        {
            get;
            set;
        }

        protected override Version GetCurrentVersion()
        {
            return CurrentVersion;
        }
    }
}
