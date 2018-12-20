using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common;
using System.Configuration;
using Dev2.Runtime.WebServer;
using System.Net;

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
            bool _isWebServerSslEnabled;
            string expectedUsageStats = "33.33";
            string expectedPort = "44";
            string expectedSslPort = "55";
            ConfigurationManager.AppSettings.Set("CollectUsageStats", "33.33");
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);

            //var _isWebServerEnabled = false;
            //bool.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);
            //bool.TryParse(ConfigurationManager.AppSettings["webServerSslEnabled"], out _isWebServerSslEnabled);

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
            bool _isWebServerSslEnabled;
            string expectedUsageStats = "33.33";
            string expectedPort = "44";
            string expectedSslPort = "55";

            string expectedHttpEndpoint = "http://*:44/";
            ConfigurationManager.AppSettings.Set("CollectUsageStats", "33.33");
            ConfigurationManager.AppSettings.Set("webServerPort", expectedPort);
            ConfigurationManager.AppSettings.Set("webServerSslPort", expectedSslPort);

           // var _isWebServerEnabled = true;
            ConfigurationManager.AppSettings.Set("webServerEnabled", "True");
            ConfigurationManager.AppSettings.Set("webServerSslEnabled", "True");

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(a => a.Write(It.IsAny<string>())).Verifiable();
            //----------------Act------------------------
            var ss = new WebServerConfiguration(mockWriter.Object);
            ss.Execute();
            var endPoints = ss.EndPoints;
            //----------------Assert---------------------
            Assert.AreEqual(expectedPort, GlobalConstants.WebServerPort);
            Assert.AreEqual(expectedSslPort, GlobalConstants.WebServerSslPort);
            Assert.AreEqual(expectedUsageStats, GlobalConstants.CollectUsageStats);

            Assert.AreEqual(1, endPoints.Length);
            Assert.AreEqual("http://*:44/", endPoints[0].Url);
        }
    }
}
