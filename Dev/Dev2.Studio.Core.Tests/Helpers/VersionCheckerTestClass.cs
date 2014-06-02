using System;
using System.Windows;
using Dev2.Common.Wrappers;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Dialogs;

namespace Dev2.Core.Tests.Helpers
{
    public class VersionCheckerTestClass : VersionChecker
    {
        public int ShowPopUpHitCount = 0;
        public VersionCheckerTestClass(IDev2WebClient webClient)
            : base(webClient, new FileWrapper(), VersionInfo.FetchVersionInfoAsVersion, Dev2MessageBoxViewModel.ShowWithCustomButtons)
        {
        }

        public VersionCheckerTestClass(IDev2WebClient webClient, IFile file, Func<Version> func)
            : base(webClient, file, func, Dev2MessageBoxViewModel.ShowWithCustomButtons)
        {
        }

        public Version CurrentVersion { get; set; }

        public MessageBoxResult ShowPopupResult { get; set; }

        protected override MessageBoxResult ShowDownloadPopUp()
        {
            ShowPopUpHitCount++;
            return ShowPopupResult;
        }
        protected override MessageBoxResult ShowStartNowPopUp()
        {
            ShowStartHitCount++;
            return StartNowResult;
        }

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
