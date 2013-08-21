using System;
using System.Windows;
using Dev2.Studio.Core.Helpers;

namespace Dev2.Core.Tests.Helpers
{
    public class VersionCheckerTestClass : VersionChecker
    {
        public int ShowPopUpHitCount = 0;
        public VersionCheckerTestClass(IDev2WebClient webClient)
            : base(webClient)
        {
        }

        public Version CurrentVersion { get; set; }

        public MessageBoxResult ShowPopupResult { get; set; }

        protected override MessageBoxResult ShowDownloadPopUp()
        {
            ShowPopUpHitCount++;
            return ShowPopupResult;
        }

        protected override Version GetCurrentVersion()
        {
            return CurrentVersion;
        }
    }
}
