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
using System;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class WebServerConfigurationTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ConfigurationManager.AppSettings.Set("webServerEnabled", "false");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "false");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        [DoNotParallelize]
        public void WebServerConfigurationTests_Execute_IsWebServerEnabled_False()
        {
            //----------------Arrange--------------------
            string expectedUsageStats = "33.33";
            string expectedPort = "44";
            string expectedSslPort = "55";
            ConfigurationManager.AppSettings.Set("CollectUsageStats", expectedUsageStats);
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);
            
            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            //----------------Assert---------------------
            Assert.AreEqual(expectedUsageStats, GlobalConstants.CollectUsageStats);
            Assert.AreEqual(expectedPort, GlobalConstants.WebServerPort);
            Assert.AreEqual(expectedSslPort, GlobalConstants.WebServerSslPort);

            Assert.IsFalse(webServerConfig.IsWebServerEnabled);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        public void WebServerConfigurationTests_Execute__IsWebServerEnabled_True()
        {
            //----------------Arrange--------------------
            ConfigurationManager.AppSettings.Set("webServerPort", "80");
            ConfigurationManager.AppSettings.Set("webServerEnabled", "true");

            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            var endPoints = webServerConfig.EndPoints;

            Assert.AreEqual(1, endPoints.Length);
            Assert.AreEqual("http://*:80/", endPoints[0].Url);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        public void WebServerConfigurationTests_Execute__IsWebServerEnabled_and_isWebServerSslEnabled()
        {
            //----------------Arrange--------------------
            ConfigurationManager.AppSettings.Set("webServerEnabled", "True");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "True");

            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            //----------------Assert---------------------

            Assert.IsTrue(webServerConfig.IsWebServerEnabled);
            Assert.IsTrue(webServerConfig.IsWebServerSslEnabled);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        public void WebServerConfigurationTests_Execute_WebServerPort_Invalid_ExpectException()
        {
            //----------------Arrange--------------------

            ConfigurationManager.AppSettings.Set("webServerPort", "ihjyyhygh");
            ConfigurationManager.AppSettings.Set("webServerSslPort", "asdfddd");

            ConfigurationManager.AppSettings.Set("webServerEnabled", "true");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "true");

            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            //----------------Assert---------------------
            mockWriter.Verify(a => a.Fail("Server initialization failed", It.IsAny<ArgumentException>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        public void WebServerConfigurationTests_Execute_WebServerSslPort_Invalid_ExpectException()
        {
            //----------------Arrange--------------------

            ConfigurationManager.AppSettings.Set("webServerPort", "80");
            ConfigurationManager.AppSettings.Set("webServerSslPort", "asdfddd");

            ConfigurationManager.AppSettings.Set("webServerEnabled", "true");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "true");
            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            //----------------Assert---------------------
            mockWriter.Verify(a => a.Fail("Server initialization failed", It.IsAny<ArgumentException>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServerConfiguration))]
        public void WebServerConfigurationTests_Execute_WebServerPort_IsEmptyOrNull_ExpectExeption()
        {
            //----------------Arrange--------------------
            string expectedUsageStats = "33.33";
            string expectedPort = "";
            string expectedSslPort = "55";
            ConfigurationManager.AppSettings.Set("CollectUsageStats", expectedUsageStats);
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);

            ConfigurationManager.AppSettings.Set("webServerEnabled", "true");

            var mockWriter = new Mock<IWriter>();
            var mockFileWrapper = new Mock<IFile>();
            //----------------Act------------------------
            var webServerConfig = new WebServerConfiguration(mockWriter.Object, mockFileWrapper.Object);
            webServerConfig.Execute();
            //----------------Assert---------------------
            mockWriter.Verify(a => a.Fail("Server initialization failed", It.IsAny<ArgumentException>()), Times.Once);
            
        }
    }
}
