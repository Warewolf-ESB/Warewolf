/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class ExchangeSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Validate()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const enSourceType expectedType = enSourceType.ExchangeSource;
            const string expectedResourceType = "ExchangeSource";
            const int expectedTimeout = 30;
            const string expectedEmailTo = "testEmailTo";
            const string expectedPath = "testPath";
            var expectedID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                ResourceID = expectedResourceID,
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Type = expectedType,
                ResourceType = expectedResourceType,
                Timeout = expectedTimeout,
                EmailTo = expectedEmailTo,
                Path = expectedPath,
                Id = expectedID,
                ResourceName = expectedResourceName
            };

            Assert.AreEqual(expectedResourceID, exchangeSourceDefinition.ResourceID);
            Assert.AreEqual(expectedAutoDiscoverUrl, exchangeSourceDefinition.AutoDiscoverUrl);
            Assert.AreEqual(expectedUserName, exchangeSourceDefinition.UserName);
            Assert.AreEqual(expectedPassword, exchangeSourceDefinition.Password);
            Assert.AreEqual(expectedType, exchangeSourceDefinition.Type);
            Assert.AreEqual(expectedResourceType, exchangeSourceDefinition.ResourceType);
            Assert.AreEqual(expectedTimeout, exchangeSourceDefinition.Timeout);
            Assert.AreEqual(expectedEmailTo, exchangeSourceDefinition.EmailTo);
            Assert.AreEqual(expectedPath, exchangeSourceDefinition.Path);
            Assert.AreEqual(expectedID, exchangeSourceDefinition.Id);
            Assert.AreEqual(expectedResourceName, exchangeSourceDefinition.ResourceName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSource_Null_Expected_False()
        {
            var exchangeSourceDefinition = new ExchangeSourceDefinition();

            const IExchangeSource exchangeSource = null;

            var isEqual = exchangeSourceDefinition.Equals(exchangeSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSource_Expected_True()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var mockExchangeSource = new Mock<IExchangeSource>();
            mockExchangeSource.Setup(exchangeSource => exchangeSource.AutoDiscoverUrl).Returns(expectedAutoDiscoverUrl);
            mockExchangeSource.Setup(exchangeSource => exchangeSource.UserName).Returns(expectedUserName);
            mockExchangeSource.Setup(exchangeSource => exchangeSource.Password).Returns(expectedPassword);
            mockExchangeSource.Setup(exchangeSource => exchangeSource.Timeout).Returns(expectedTimeout);

            var isEqual = exchangeSourceDefinition.Equals(mockExchangeSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_ReferenceEquals_ExchangeSource_Expected_True()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            IExchangeSource exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var isEqual = exchangeSourceDefinition.Equals(exchangeSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSource_Expected_False()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var exchangeSourceDefinitionDup = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = "NewAutoDiscoverUrl",
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var isEqual = exchangeSourceDefinition.Equals(exchangeSourceDefinitionDup);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSourceDefinition_Null_Expected_False()
        {
            var exchangeSourceDefinition = new ExchangeSourceDefinition();

            const ExchangeSourceDefinition nullExchangeSourceDefinition = null;

            var isEqual = exchangeSourceDefinition.Equals(nullExchangeSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_ReferenceEquals_ExchangeSourceDefinition_Expected_True()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var isEqual = exchangeSourceDefinition.Equals(exchangeSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSourceDefinition_Expected_True()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };
            var exchangeSourceDefinitionDup = exchangeSourceDefinition;

            var isEqual = exchangeSourceDefinition.Equals(exchangeSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(exchangeSourceDefinition == exchangeSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_ExchangeSourceDefinition_Expected_False()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const int expectedTimeout = 30;

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var exchangeSourceDefinitionDup = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = "NewAutoDiscoverUrl",
                UserName = expectedUserName,
                Password = expectedPassword,
                Timeout = expectedTimeout
            };

            var isEqual = exchangeSourceDefinition.Equals(exchangeSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(exchangeSourceDefinition != exchangeSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var exchangeSourceDefinition = new ExchangeSourceDefinition();

            const object exchangeSource = null;

            var isEqual = exchangeSourceDefinition.Equals(exchangeSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_Object_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const enSourceType expectedType = enSourceType.ExchangeSource;
            const string expectedResourceType = "ExchangeSource";
            const int expectedTimeout = 30;
            const string expectedEmailTo = "testEmailTo";
            const string expectedPath = "testPath";
            var expectedID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                ResourceID = expectedResourceID,
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Type = expectedType,
                ResourceType = expectedResourceType,
                Timeout = expectedTimeout,
                EmailTo = expectedEmailTo,
                Path = expectedPath,
                Id = expectedID,
                ResourceName = expectedResourceName
            };

            object exhangeSource = exchangeSourceDefinition;

            var isEqual = exchangeSourceDefinition.Equals(exhangeSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_Object_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const enSourceType expectedType = enSourceType.ExchangeSource;
            const string expectedResourceType = "ExchangeSource";
            const int expectedTimeout = 30;
            const string expectedEmailTo = "testEmailTo";
            const string expectedPath = "testPath";
            var expectedID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                ResourceID = expectedResourceID,
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Type = expectedType,
                ResourceType = expectedResourceType,
                Timeout = expectedTimeout,
                EmailTo = expectedEmailTo,
                Path = expectedPath,
                Id = expectedID,
                ResourceName = expectedResourceName
            };

            var exchangeSourceDefinitionDup = new ExchangeSourceDefinition
            {
                ResourceID = expectedResourceID,
                AutoDiscoverUrl = "NewAutoDiscoverUrl",
                UserName = expectedUserName,
                Password = expectedPassword,
                Type = expectedType,
                ResourceType = expectedResourceType,
                Timeout = expectedTimeout,
                EmailTo = expectedEmailTo,
                Path = expectedPath,
                Id = expectedID,
                ResourceName = expectedResourceName
            };

            object exchangeSource = exchangeSourceDefinitionDup;

            var isEqual = exchangeSourceDefinition.Equals(exchangeSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";
            const enSourceType expectedType = enSourceType.ExchangeSource;
            const string expectedResourceType = "ExchangeSource";
            const int expectedTimeout = 30;
            const string expectedEmailTo = "testEmailTo";
            const string expectedPath = "testPath";
            var expectedID = Guid.NewGuid();
            const string expectedResourceName = "testResourceName";

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                ResourceID = expectedResourceID,
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword,
                Type = expectedType,
                ResourceType = expectedResourceType,
                Timeout = expectedTimeout,
                EmailTo = expectedEmailTo,
                Path = expectedPath,
                Id = expectedID,
                ResourceName = expectedResourceName
            };

            var exchangeSource = new object();

            var isEqual = exchangeSourceDefinition.Equals(exchangeSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedAutoDiscoverUrl = "testAutoDiscoverUrl";
            const string expectedUserName = "testuser";
            const string expectedPassword = "test123";

            var exchangeSourceDefinition = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = expectedAutoDiscoverUrl,
                UserName = expectedUserName,
                Password = expectedPassword
            };

            var hashCode = exchangeSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeSourceDefinition))]
        public void ExchangeSourceDefinition_GetHashCode_Expect_Zero()
        {
            var exchangeSourceDefinition = new ExchangeSourceDefinition();

            var hashCode = exchangeSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
