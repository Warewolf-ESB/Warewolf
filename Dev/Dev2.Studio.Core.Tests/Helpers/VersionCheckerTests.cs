
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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Windows;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class VersionCheckerTests
    {
        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsLatestExpectedTake5()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(1, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.Warewolf_Homepage_Start, startPage);
        }

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
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.Uri_Community_HomePage, startPage);
        }

        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsNotLatestExpectedStart()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(2, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.Warewolf_Homepage_Start, startPage);
        }

        #region IsLastest Tests


        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        // ReSharper disable InconsistentNaming
        public void GetNewerVersion_LaterVersion_ReturnsTrue()
        // ReSharper restore InconsistentNaming
        {
            Mock<IDev2WebClient> mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.2").Verifiable();

            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object) { ShowPopupResult = MessageBoxResult.No, CurrentVersion = new Version(0, 0, 0, 1) };
            var newerVersion = versionChecker.GetNewerVersion();

            Assert.IsTrue(newerVersion);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        // ReSharper disable InconsistentNaming
        public void GetNewerVersion_NotLaterVersion_ReturnsFalse()
        // ReSharper restore InconsistentNaming
        {
            Mock<IDev2WebClient> mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.1").Verifiable();

            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object) { ShowPopupResult = MessageBoxResult.No, CurrentVersion = new Version(0, 0, 0, 1) };
            var newerVersion = versionChecker.GetNewerVersion();

            Assert.IsFalse(newerVersion);
        }

        #endregion


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Currentr_NullVersionChecker_ExpectException()
        {
            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            new VersionChecker(new Dev2WebClient(new WebClient()), null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_LatestVersionCheckSum")]
        public void VersionChecker_LatestVersionCheckSum_CallsCorrectDownloadString()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            webClient.Setup(a => a.DownloadString(It.IsAny<string>())).Returns("1.2.1.1");
            var versionChecker = new VersionChecker(webClient.Object, () => new Version(0, 0, 0, 1));


            var ax = versionChecker.LatestVersionCheckSum;
            //------------Execute Test---------------------------

            Assert.AreEqual("1.2.1.1", ax);
            //------------Assert Results-------------------------
        }
    }
}
// ReSharper restore InconsistentNaming
