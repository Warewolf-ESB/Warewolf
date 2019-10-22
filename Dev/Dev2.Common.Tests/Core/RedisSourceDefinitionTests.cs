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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class RedisSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Validate()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            Assert.AreEqual(expectedAuthenticationType, redisSourceDefinition.AuthenticationType);
            Assert.AreEqual(expectedId, redisSourceDefinition.Id);
            Assert.AreEqual(expectedName, redisSourceDefinition.Name);
            Assert.AreEqual(expectedPassword, redisSourceDefinition.Password);
            Assert.AreEqual(expectedHostName, redisSourceDefinition.HostName);
            Assert.AreEqual(expectedPath, redisSourceDefinition.Path);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisServiceSource_Null_Expected_False()
        {
            var redisSourceDefinition = new RedisSourceDefinition();

            const IRedisServiceSource redisServiceSource = null;

            var isEqual = redisSourceDefinition.Equals(redisServiceSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisServiceSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var mockRedisServiceSource = new Mock<IRedisServiceSource>();
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.HostName).Returns(expectedHostName);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Id).Returns(expectedId);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Name).Returns(expectedName);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Password).Returns(expectedPassword);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.AuthenticationType).Returns(expectedAuthenticationType);

            var isEqual = redisSourceDefinition.Equals(mockRedisServiceSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_ReferenceEquals_RedisServiceSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            IRedisServiceSource redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var isEqual = redisSourceDefinition.Equals(redisSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisServiceSource_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var mockRedisServiceSource = new Mock<IRedisServiceSource>();
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.HostName).Returns("NewHostName");
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Id).Returns(expectedId);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Name).Returns(expectedName);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.Password).Returns(expectedPassword);
            mockRedisServiceSource.Setup(redisServiceSource => redisServiceSource.AuthenticationType).Returns(expectedAuthenticationType);

            var isEqual = redisSourceDefinition.Equals(mockRedisServiceSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisSourceDefinition_Null_Expected_False()
        {
            var redisSourceDefinition = new RedisSourceDefinition();

            const RedisSourceDefinition nullRedisSourceDefinition = null;

            var isEqual = redisSourceDefinition.Equals(nullRedisSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_ReferenceEquals_RedisSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var isEqual = redisSourceDefinition.Equals(redisSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);
            var redisSourceDefinitionDup = new RedisSourceDefinition(mockRedisSource.Object);

            var isEqual = redisSourceDefinition.Equals(redisSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(redisSourceDefinition == redisSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_RedisSourceDefinition_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var mockRedisSourceDup = new Mock<IRedisSource>();
            mockRedisSourceDup.Setup(redisSource => redisSource.HostName).Returns("NewHostName");
            mockRedisSourceDup.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSourceDup.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSourceDup.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSourceDup.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);

            var redisSourceDefinitionDup = new RedisSourceDefinition(mockRedisSourceDup.Object);

            var isEqual = redisSourceDefinition.Equals(redisSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(redisSourceDefinition != redisSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var redisSourceDefinition = new RedisSourceDefinition();

            const object redisSource = null;

            var isEqual = redisSourceDefinition.Equals(redisSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_Object_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            object redisSourceObj = redisSourceDefinition;

            var isEqual = redisSourceDefinition.Equals(redisSourceObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_Object_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var mockRedisSourceDup = new Mock<IRedisSource>();
            mockRedisSourceDup.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSourceDup.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSourceDup.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSourceDup.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSourceDup.Setup(redisSource => redisSource.HostName).Returns("NewHostName");
            mockRedisSourceDup.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            object redisSourceObj = new RedisSourceDefinition(mockRedisSourceDup.Object);

            var isEqual = redisSourceDefinition.Equals(redisSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var redisSourceObj = new object();

            var isEqual = redisSourceDefinition.Equals(redisSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Password;
            var expectedId = Guid.NewGuid();
            const string expectedName = "testResourceName";
            const string expectedPassword = "test123";
            const string expectedHostName = "testHost";
            const string expectedPath = "testPath";

            var mockRedisSource = new Mock<IRedisSource>();
            mockRedisSource.Setup(redisSource => redisSource.AuthenticationType).Returns(expectedAuthenticationType);
            mockRedisSource.Setup(redisSource => redisSource.ResourceID).Returns(expectedId);
            mockRedisSource.Setup(redisSource => redisSource.ResourceName).Returns(expectedName);
            mockRedisSource.Setup(redisSource => redisSource.Password).Returns(expectedPassword);
            mockRedisSource.Setup(redisSource => redisSource.HostName).Returns(expectedHostName);
            mockRedisSource.Setup(redisSource => redisSource.GetSavePath()).Returns(expectedPath);

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var hashCode = redisSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisSourceDefinition))]
        public void RedisSourceDefinition_GetHashCode_Expect_Zero()
        {
            var mockRedisSource = new Mock<IRedisSource>();

            var redisSourceDefinition = new RedisSourceDefinition(mockRedisSource.Object);

            var hashCode = redisSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
