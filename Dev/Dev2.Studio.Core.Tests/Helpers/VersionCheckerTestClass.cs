/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Utils;
using Dev2.Common.Interfaces;

namespace Dev2.Core.Tests.Helpers
{
    public class VersionCheckerTestClass : VersionChecker
    {
        public int ShowPopUpHitCount = 0;
        public VersionCheckerTestClass(IWarewolfWebClient webClient)
            : base(webClient, VersionInfo.FetchVersionInfoAsVersion)
        {
        }

        public VersionCheckerTestClass(IWarewolfWebClient webClient, Func<Version> func)
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
