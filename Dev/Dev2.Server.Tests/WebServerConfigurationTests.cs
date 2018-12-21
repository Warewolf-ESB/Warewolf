/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common;
using System.Configuration;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class WebServerConfigurationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfigurationTests))]
        public void WebServerConfigurationTests_Execute__IsWebServerEnabled_False()
        {
            //----------------Arrange--------------------
            string expectedUsageStats = "33.33";
            string expectedPort = "44";
            string expectedSslPort = "55";
            ConfigurationManager.AppSettings.Set("CollectUsageStats", "33.33");
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(a => a.Write(It.IsAny<string>())).Verifiable();
            //----------------Act------------------------
            new WebServerConfiguration(mockWriter.Object).Execute();
            //----------------Assert---------------------
            Assert.AreEqual(expectedPort, GlobalConstants.WebServerPort);
            Assert.AreEqual(expectedSslPort, GlobalConstants.WebServerSslPort);
            Assert.AreEqual(expectedUsageStats, GlobalConstants.CollectUsageStats);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfigurationTests))]
        public void WebServerConfigurationTests_Execute__IsWebServerEnabled_True()
        {
            //----------------Arrange--------------------
            string expectedUsageStats = "33.33";
            string expectedPort = "44";
            string expectedSslPort = "55";
            
            ConfigurationManager.AppSettings.Set("CollectUsageStats", "33.33");
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);
            
            ConfigurationManager.AppSettings.Set("webServerEnabled", "True");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "True");

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(a => a.Write(It.IsAny<string>())).Verifiable();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object);
            webServerConfig.Execute();
            var endPoints = webServerConfig.EndPoints;
            //----------------Assert---------------------
            Assert.AreEqual(expectedPort, GlobalConstants.WebServerPort);
            Assert.AreEqual(expectedSslPort, GlobalConstants.WebServerSslPort);
            Assert.AreEqual(expectedUsageStats, GlobalConstants.CollectUsageStats);

            Assert.AreEqual(1, endPoints.Length);
            Assert.AreEqual("http://*:44/", endPoints[0].Url);
        }
    }
}
