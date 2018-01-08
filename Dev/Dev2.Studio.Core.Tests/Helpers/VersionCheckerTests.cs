/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Dev2.Studio.Core;
using System.ComponentModel;

namespace Dev2.Core.Tests.Helpers
{
    
    [TestClass]
    public class VersionCheckerTests
    {

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VersionChecker_GetCommunityPage")]
        public void VersionChecker_GetCommunityPage_ReturnsResourceCommunityPageUrl()
        {
            //------------Setup for test--------------------------
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(1, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));
            //------------Execute Test---------------------------
            var startPage = checker.Object.CommunityPageUri;
            //------------Assert Results-------------------------
            Assert.AreEqual(StringResources.Uri_Community_HomePage, startPage);
        }


        #region IsLastest Tests

        
        #endregion


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Currentr_NullVersionChecker_ExpectException()
        {
            //------------Setup for test--------------------------
            
            new VersionChecker(new WarewolfWebClient(new WebClient()), null);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("VersionChecker_Ctor")]
        public void WarewolfWebClient_AddRemove_EventHandlers()
        {
            using (var warewolfWebClient =
                        new WarewolfWebClient(new WebClient()))
            {
                warewolfWebClient.DownloadProgressChanged += new Mock<DownloadProgressChangedEventHandler>().Object;
                warewolfWebClient.DownloadProgressChanged -= new Mock<DownloadProgressChangedEventHandler>().Object;
                warewolfWebClient.DownloadFileCompleted += new Mock<AsyncCompletedEventHandler>().Object;
                warewolfWebClient.DownloadFileCompleted -= new Mock<AsyncCompletedEventHandler>().Object;
            }
        }
    }
}

