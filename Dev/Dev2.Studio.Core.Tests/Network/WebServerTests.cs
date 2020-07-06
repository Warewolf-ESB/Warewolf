/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public class WebServerTests
    {

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenInvalidResourceModel_DoesNotThrow()
        {
            const string xmlData = "";
            IContextualResourceModel resourceModel = null;

            var result = resourceModel.GetWorkflowUri(xmlData, UrlType.API);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenInvalidEnvironment_DoesNotThrow()
        {
            const string xmlData = "";
            var mockResourceModel = new Mock<IContextualResourceModel>();

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.API);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenDisconnectedEnvironment_DoesNotThrow()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.API);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenAPI_UrlType()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.Category).Returns("some category");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.API);

            Assert.AreEqual("http://somehost:1234/secure/some category.api", result.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenAPI_UrlType_CategoryDefaultsToResourceName()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.ResourceName).Returns("some resource name");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.API);

            Assert.AreEqual("http://somehost:1234/secure/some resource name.api", result.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenXml_UrlType_CategoryDefaultsToResourceName()
        {
            const string workspaceId = "1c52f5da-2c9d-4320-911e-8fa2d2b1fd62";
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);
            mockConnection.Setup(o => o.WorkspaceID).Returns(Guid.Parse(workspaceId));

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.ResourceName).Returns("some resource name");
            mockResourceModel.Setup(o => o.Category).Returns("some category");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.Xml);

            Assert.AreEqual("http://somehost:1234/secure/some category.xml?<xmltag></xmltag>&wid=1c52f5da-2c9d-4320-911e-8fa2d2b1fd62", result.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenJson_UrlType_CategoryDefaultsToResourceName()
        {
            const string workspaceId = "1c52f5da-2c9d-4320-911e-8fa2d2b1fd62";
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);
            mockConnection.Setup(o => o.WorkspaceID).Returns(Guid.Parse(workspaceId));

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.ResourceName).Returns("some resource name");
            mockResourceModel.Setup(o => o.Category).Returns("some category");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.Json);

            Assert.AreEqual("http://somehost:1234/secure/some category.json?<xmltag></xmltag>&wid=1c52f5da-2c9d-4320-911e-8fa2d2b1fd62", result.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenTests_UrlType_CategoryDefaultsToResourceName()
        {
            const string workspaceId = "1c52f5da-2c9d-4320-911e-8fa2d2b1fd62";
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);
            mockConnection.Setup(o => o.WorkspaceID).Returns(Guid.Parse(workspaceId));

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.ResourceName).Returns("some resource name");
            mockResourceModel.Setup(o => o.Category).Returns("some category");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.Tests);

            Assert.AreEqual("http://somehost:1234/secure/some category.tests", result.ToString());
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServer))]
        public void ResourceModelWebserverUtil_GetWorkflowUri_GivenCoverage_UrlType_CategoryDefaultsToResourceName()
        {
            const string workspaceId = "1c52f5da-2c9d-4320-911e-8fa2d2b1fd62";
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(o => o.WebServerUri).Returns(new Uri("http://somehost:1234/"));
            mockConnection.Setup(o => o.IsConnected).Returns(true);
            mockConnection.Setup(o => o.WorkspaceID).Returns(Guid.Parse(workspaceId));

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockConnection.Object);
            mockServer.Setup(o => o.IsConnected).Returns(mockConnection.Object.IsConnected);

            const string xmlData = "<xmltag></xmltag>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            mockResourceModel.Setup(o => o.ResourceName).Returns("some resource name");
            mockResourceModel.Setup(o => o.Category).Returns("some category");

            var result = mockResourceModel.Object.GetWorkflowUri(xmlData, UrlType.Coverage);

            Assert.AreEqual("http://somehost:1234/secure/some category.coverage", result.ToString());
        }
    }
}
