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
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class SharePointServiceSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Validate()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPath = "";
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object)
            {
                IsSharepointOnline = false
            };

            Assert.IsFalse(sharePointServiceSourceDefinition.IsSharepointOnline);

            Assert.AreEqual(expectedAuthenticationType, sharePointServiceSourceDefinition.AuthenticationType);
            Assert.AreEqual(expectedServer, sharePointServiceSourceDefinition.Server);
            Assert.AreEqual(expectedPath, sharePointServiceSourceDefinition.Path);
            Assert.AreEqual(expectedResourceID, sharePointServiceSourceDefinition.Id);
            Assert.AreEqual(expectedResourceName, sharePointServiceSourceDefinition.Name);
            Assert.AreEqual(expectedPassword, sharePointServiceSourceDefinition.Password);
            Assert.AreEqual(expectedUserName, sharePointServiceSourceDefinition.UserName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharepointSource_Null_Expected_False()
        {
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition();

            const ISharepointServerSource sharepointServerSource = null;

            var isEqual = sharePointServiceSourceDefinition.Equals(sharepointServerSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharepointSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var mockSharepointServerSource = new Mock<ISharepointServerSource>();
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.Server).Returns(expectedServer);
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.UserName).Returns(expectedUserName);
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.Password).Returns(expectedPassword);
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.AuthenticationType).Returns(expectedAuthenticationType);

            var isEqual = sharePointServiceSourceDefinition.Equals(mockSharepointServerSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_ReferenceEquals_SharepointSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            ISharepointServerSource sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var isEqual = sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharepointSource_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var mockSharepointServerSource = new Mock<ISharepointServerSource>();
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.Server).Returns("remoteServer");
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.UserName).Returns(expectedUserName);
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.Password).Returns(expectedPassword);
            mockSharepointServerSource.Setup(sharepointServerSource => sharepointServerSource.AuthenticationType).Returns(expectedAuthenticationType);

            var isEqual = sharePointServiceSourceDefinition.Equals(mockSharepointServerSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharePointServiceSourceDefinition_Null_Expected_False()
        {
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition();

            const SharePointServiceSourceDefinition nullSharePointServiceSourceDefinition = null;

            var isEqual = sharePointServiceSourceDefinition.Equals(nullSharePointServiceSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_ReferenceEquals_SharePointServiceSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var isEqual = sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharePointServiceSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);
            var sharePointServiceSourceDefinitionDup = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var isEqual = sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(sharePointServiceSourceDefinition == sharePointServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_SharePointServiceSourceDefinition_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var mockSharepointSourceDup = new Mock<ISharepointSource>();
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.Server).Returns("remoteServer");
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);

            var sharePointServiceSourceDefinitionDup = new SharePointServiceSourceDefinition(mockSharepointSourceDup.Object);

            var isEqual = sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(sharePointServiceSourceDefinition != sharePointServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition();

            const object sharepointSource = null;

            var isEqual = sharePointServiceSourceDefinition.Equals(sharepointSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_Object_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPath = "";
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            object sharepointSourceObj = sharePointServiceSourceDefinition;

            var isEqual = sharePointServiceSourceDefinition.Equals(sharepointSourceObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_Object_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPath = "";
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var mockSharepointSourceDup = new Mock<ISharepointSource>();
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.Server).Returns("remoteServer");
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSourceDup.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            object sharePointServiceSourceDefinitionDup = new SharePointServiceSourceDefinition(mockSharepointSourceDup.Object);

            var isEqual = sharePointServiceSourceDefinition.Equals(sharePointServiceSourceDefinitionDup);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPath = "";
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var sharepointSourceObj = new object();

            var isEqual = sharePointServiceSourceDefinition.Equals(sharepointSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedServer = "localhost";
            const string expectedPath = "";
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedUserName = "testUser";

            var mockSharepointSource = new Mock<ISharepointSource>();
            mockSharepointSource.Setup(sharepointSource => sharepointSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Server).Returns(expectedServer);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.GetSavePath()).Returns(expectedPath);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceID).Returns(expectedResourceID);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.ResourceName).Returns(expectedResourceName);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.Password).Returns(expectedPassword);
            mockSharepointSource.Setup(sharepointSource => sharepointSource.UserName).Returns(expectedUserName);

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var hashCode = sharePointServiceSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SharePointServiceSourceDefinition))]
        public void SharePointServiceSourceDefinition_GetHashCode_Expect_Zero()
        {
            var mockSharepointSource = new Mock<ISharepointSource>();

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition(mockSharepointSource.Object);

            var hashCode = sharePointServiceSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
