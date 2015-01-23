
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
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.Core.Test
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VersionCheckerTests
    {


        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        public void GetNewerVersion_LaterVersion_ReturnsTrue()
        {
            Mock<IWarewolfWebClient> mockWebClient = new Mock<IWarewolfWebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.2").Verifiable();

            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object) { ShowPopupResult = MessageBoxResult.No, CurrentVersion = new Version(0, 0, 0, 1) };
            var newerVersion = versionChecker.GetNewerVersion();

            Assert.IsTrue(newerVersion);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        public void GetNewerVersion_NotLaterVersion_ReturnsFalse()
        {
            Mock<IWarewolfWebClient> mockWebClient = new Mock<IWarewolfWebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.1").Verifiable();

            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object) { ShowPopupResult = MessageBoxResult.No, CurrentVersion = new Version(0, 0, 0, 1) };
            var newerVersion = versionChecker.GetNewerVersion();

            Assert.IsFalse(newerVersion);
        }



        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Currentr_NullVersionChecker_ExpectException()
        {
            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            new VersionChecker(new WarewolfWebClient(new WebClient()), null);
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
            var webClient = new Mock<IWarewolfWebClient>();
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
