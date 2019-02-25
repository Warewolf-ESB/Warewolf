/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class WebServiceSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Validate()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            Assert.AreEqual(expectedAuthenticationType, webServiceSourceDefinition.AuthenticationType);
            Assert.AreEqual(expectedDefaultQuery, webServiceSourceDefinition.DefaultQuery);
            Assert.AreEqual(expectedId, webServiceSourceDefinition.Id);
            Assert.AreEqual(expectedName, webServiceSourceDefinition.Name);
            Assert.AreEqual(expectedPassword, webServiceSourceDefinition.Password);
            Assert.AreEqual(expectedHostName, webServiceSourceDefinition.HostName);
            Assert.AreEqual(expectedPath, webServiceSourceDefinition.Path);
            Assert.AreEqual(expectedUserName, webServiceSourceDefinition.UserName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSource_Null_Expected_False()
        {
            var webServiceSourceDefinition = new WebServiceSourceDefinition();

            const IWebServiceSource webServiceSource = null;

            var isEqual = webServiceSourceDefinition.Equals(webServiceSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var mockWebServiceSource = new Mock<IWebServiceSource>();
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.HostName).Returns(expectedHostName);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Id).Returns(expectedId);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Name).Returns(expectedName);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.UserName).Returns(expectedUserName);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Password).Returns(expectedPassword);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.DefaultQuery).Returns(expectedDefaultQuery);

            var isEqual = webServiceSourceDefinition.Equals(mockWebServiceSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_ReferenceEquals_WebServiceSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            IWebServiceSource webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var isEqual = webServiceSourceDefinition.Equals(webServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSource_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var mockWebServiceSource = new Mock<IWebServiceSource>();
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.HostName).Returns("NewHostName");
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Id).Returns(expectedId);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Name).Returns(expectedName);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.UserName).Returns(expectedUserName);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.Password).Returns(expectedPassword);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebServiceSource.Setup(webServiceSource => webServiceSource.DefaultQuery).Returns(expectedDefaultQuery);

            var isEqual = webServiceSourceDefinition.Equals(mockWebServiceSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSourceDefinition_Null_Expected_False()
        {
            var webServiceSourceDefinition = new WebServiceSourceDefinition();

            const WebServiceSourceDefinition nullWebServiceSourceDefinition = null;

            var isEqual = webServiceSourceDefinition.Equals(nullWebServiceSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_ReferenceEquals_WebServiceSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var isEqual = webServiceSourceDefinition.Equals(webServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);
            var webServiceSourceDefinitionDup = new WebServiceSourceDefinition(mockWebSource.Object);

            var isEqual = webServiceSourceDefinition.Equals(webServiceSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(webServiceSourceDefinition == webServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_WebServiceSourceDefinition_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var mockWebSourceDup = new Mock<IWebSource>();
            mockWebSourceDup.Setup(webSource => webSource.Address).Returns("NewHostName");
            mockWebSourceDup.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSourceDup.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSourceDup.Setup(webSource => webSource.UserName).Returns(expectedUserName);
            mockWebSourceDup.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSourceDup.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSourceDup.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);

            var webServiceSourceDefinitionDup = new WebServiceSourceDefinition(mockWebSourceDup.Object);

            var isEqual = webServiceSourceDefinition.Equals(webServiceSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(webServiceSourceDefinition != webServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var webServiceSourceDefinition = new WebServiceSourceDefinition();

            const object webSource = null;

            var isEqual = webServiceSourceDefinition.Equals(webSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_Object_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            object webSourceObj = webServiceSourceDefinition;

            var isEqual = webServiceSourceDefinition.Equals(webSourceObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_Object_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var mockWebSourceDup = new Mock<IWebSource>();
            mockWebSourceDup.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSourceDup.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSourceDup.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSourceDup.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSourceDup.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSourceDup.Setup(webSource => webSource.Address).Returns("NewHostName");
            mockWebSourceDup.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSourceDup.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            object webSourceObj = new WebServiceSourceDefinition(mockWebSourceDup.Object);

            var isEqual = webServiceSourceDefinition.Equals(webSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var webSourceObj = new object();

            var isEqual = webServiceSourceDefinition.Equals(webSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDefaultQuery = "testDefaultQuery";
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";
            const string expectedUserName = "testUser";

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(webSource => webSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockWebSource.Setup(webSource => webSource.DefaultQuery).Returns(expectedDefaultQuery);
            mockWebSource.Setup(webSource => webSource.ResourceID).Returns(expectedId);
            mockWebSource.Setup(webSource => webSource.ResourceName).Returns(expectedName);
            mockWebSource.Setup(webSource => webSource.Password).Returns(expectedPassword);
            mockWebSource.Setup(webSource => webSource.Address).Returns(expectedHostName);
            mockWebSource.Setup(webSource => webSource.GetSavePath()).Returns(expectedPath);
            mockWebSource.Setup(webSource => webSource.UserName).Returns(expectedUserName);

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var hashCode = webServiceSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebServiceSourceDefinition))]
        public void WebServiceSourceDefinition_GetHashCode_Expect_Zero()
        {
            var mockWebSource = new Mock<IWebSource>();

            var webServiceSourceDefinition = new WebServiceSourceDefinition(mockWebSource.Object);

            var hashCode = webServiceSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
