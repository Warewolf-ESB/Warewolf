
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Net;
using Dev2.Common.Wrappers;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs
{
    [Binding]
    public class VersionDownloaderSteps
    {
        [Given(@"the version of the software on the server is ""(.*)""")]
        public void GivenTheVersionOfTheSoftwareOnTheServerIs(string p0)
        {

            VersionChecker checker = new VersionChecker(new Dev2WebClient(new WebClient()),new FileWrapper(), (a=>new Version(p0)));
        }
        
        [Given(@"the current version is ""(.*)""")]
        public void GivenTheCurrentVersionIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I start the studio")]
        public void WhenIStartTheStudio()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"choose to download the new version ""(.*)""")]
        public void ThenChooseToDownloadTheNewVersion(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the file will be saved on the filesystem ""(.*)""")]
        public void ThenTheFileWillBeSavedOnTheFilesystem(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the file will not be saved on the filesystem ""(.*)""")]
        public void ThenTheFileWillNotBeSavedOnTheFilesystem(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the studio will shutdown and install the new version ""(.*)""")]
        public void ThenTheStudioWillShutdownAndInstallTheNewVersion(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"choose to download the new version but the download ""(.*)""")]
        public void ThenChooseToDownloadTheNewVersionButTheDownload(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the file will be not saved on the filesystem ""(.*)""")]
        public void ThenTheFileWillBeNotSavedOnTheFilesystem(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
