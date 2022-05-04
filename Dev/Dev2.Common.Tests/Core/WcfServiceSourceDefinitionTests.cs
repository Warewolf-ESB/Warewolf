#if NETFRAMEWORK
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class WcfServiceSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Validate()
        {
            const string expectedResourceName = "testResourceName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedEndpointUrl = "testEndpointUrl";
            const string expectedName = "testName";
            const string expectedPath = "testPath";
            var expectedId = Guid.NewGuid();
            const enSourceType expectedType = enSourceType.WcfSource;
            const string expectedResourceType = "WcfSource";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                ResourceName = expectedResourceName,
                ResourceID = expectedResourceID,
                EndpointUrl = expectedEndpointUrl,
                Name = expectedName,
                Path = expectedPath,
                Id = expectedId,
                Type = expectedType,
                ResourceType = expectedResourceType
            };

            Assert.AreEqual(expectedResourceName, wcfServiceSourceDefinition.ResourceName);
            Assert.AreEqual(expectedResourceID, wcfServiceSourceDefinition.ResourceID);
            Assert.AreEqual(expectedEndpointUrl, wcfServiceSourceDefinition.EndpointUrl);
            Assert.AreEqual(expectedName, wcfServiceSourceDefinition.Name);
            Assert.AreEqual(expectedPath, wcfServiceSourceDefinition.Path);
            Assert.AreEqual(expectedId, wcfServiceSourceDefinition.Id);
            Assert.AreEqual(expectedType, wcfServiceSourceDefinition.Type);
            Assert.AreEqual(expectedResourceType, wcfServiceSourceDefinition.ResourceType);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServerSource_Null_Expected_False()
        {
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition();

            const IWcfServerSource wcfServerSource = null;

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServerSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServerSource_Expected_True()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var mockWcfServerSource = new Mock<IWcfServerSource>();
            mockWcfServerSource.Setup(wcfServerSource => wcfServerSource.EndpointUrl).Returns(expectedEndpointUrl);

            var isEqual = wcfServiceSourceDefinition.Equals(mockWcfServerSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_ReferenceEquals_WcfServerSource_Expected_True()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            IWcfServerSource wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServerSource_Expected_False()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var mockWcfServerSource = new Mock<IWcfServerSource>();
            mockWcfServerSource.Setup(wcfServerSource => wcfServerSource.EndpointUrl).Returns("NewEndpointUrl");

            var isEqual = wcfServiceSourceDefinition.Equals(mockWcfServerSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServiceSourceDefinition_Null_Expected_False()
        {
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition();

            const WcfServiceSourceDefinition nullWcfServiceSourceDefinition = null;

            var isEqual = wcfServiceSourceDefinition.Equals(nullWcfServiceSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_ReferenceEquals_WcfServiceSourceDefinition_Expected_True()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServiceSourceDefinition_Expected_True()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };
            var wcfServiceSourceDefinitionDup = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(wcfServiceSourceDefinition == wcfServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_WcfServiceSourceDefinition_Expected_False()
        {
            const string expectedEndpointUrl = "testEndpointUrl";
            const string expectedEndpointUrlDup = "NewEndpointUrl";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };
            var wcfServiceSourceDefinitionDup = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrlDup
            };

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(wcfServiceSourceDefinition != wcfServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_RefEquals_Object_As_WcfServiceSourceDefinition_Expected_True()
        {
            const string expectedResourceName = "testResourceName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedEndpointUrl = "testEndpointUrl";
            const string expectedName = "testName";
            const string expectedPath = "testPath";
            var expectedId = Guid.NewGuid();
            const enSourceType expectedType = enSourceType.WcfSource;
            const string expectedResourceType = "WcfSource";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                ResourceName = expectedResourceName,
                ResourceID = expectedResourceID,
                EndpointUrl = expectedEndpointUrl,
                Name = expectedName,
                Path = expectedPath,
                Id = expectedId,
                Type = expectedType,
                ResourceType = expectedResourceType
            };

            object wcfServiceSource = wcfServiceSourceDefinition;

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_RefEquals_Object_As_WcfServerSource_Expected_True()
        {
            const string expectedEndpointUrl = "testEndpointUrl";

            IWcfServerSource wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                EndpointUrl = expectedEndpointUrl
            };

            var mockWcfServerSource = new Mock<IWcfServerSource>();
            mockWcfServerSource.Setup(wcfServerSource => wcfServerSource.EndpointUrl).Returns(expectedEndpointUrl);

            object wcfServiceSource = mockWcfServerSource.Object;

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_Equals_Null_Object_As_Unknown_Expected_False()
        {
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition();

            var wcfServiceSource = new object();

            var isEqual = wcfServiceSourceDefinition.Equals(wcfServiceSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedResourceName = "testResourceName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedEndpointUrl = "testEndpointUrl";
            const string expectedName = "testName";
            const string expectedPath = "testPath";
            var expectedId = Guid.NewGuid();
            const enSourceType expectedType = enSourceType.WcfSource;
            const string expectedResourceType = "WcfSource";

            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                ResourceName = expectedResourceName,
                ResourceID = expectedResourceID,
                EndpointUrl = expectedEndpointUrl,
                Name = expectedName,
                Path = expectedPath,
                Id = expectedId,
                Type = expectedType,
                ResourceType = expectedResourceType
            };

            var hashCode = wcfServiceSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfServiceSourceDefinition))]
        public void WcfServiceSourceDefinition_GetHashCode_Expect_Zero()
        {
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition();

            var hashCode = wcfServiceSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
#endif