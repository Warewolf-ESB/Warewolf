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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Serialization;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveRedisSourceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveRedisSource = new SaveRedisSource();
            //------------Execute Test---------------------------
            var resId = saveRedisSource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveRedisSource = new SaveRedisSource();
            //------------Execute Test---------------------------
            var resId = saveRedisSource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveRedisSource = new SaveRedisSource();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("SaveRedisSource", saveRedisSource.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var saveRedisSource = new SaveRedisSource();
            //------------Execute Test---------------------------
            var dynamicService = saveRedisSource.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveRedisSource = new SaveRedisSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveRedisSource.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var saveRedisSource = new SaveRedisSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveRedisSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_Execute_GivenResourceDefination_ShouldSaveNewSourceReturnResourceDefinationMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var source = new RedisSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = "HostName",
                Port = "3679",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            var serializeToJsonString = source.SerializeToJsonString(new DefaultSerializationBinder());
            compressedExecuteMessage.SetMessage(serializeToJsonString);
            var values = new Dictionary<string, StringBuilder>
            {
                { "RedisSource", source.SerializeToJsonStringBuilder() }
            };
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name));
            catalog.Setup(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()));
            var saveRedisSource = new SaveRedisSource(catalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = saveRedisSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsFalse(result.HasError);
            catalog.Verify(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name));
            catalog.Verify(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveRedisSource))]
        public void SaveRedisSource_Execute_GivenResourceDefination_GivenExising_ShouldReturnResourceDefinationMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var source = new RedisSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = "HostName",
                Port = "3679",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            var serializeToJsonString = source.SerializeToJsonString(new DefaultSerializationBinder());
            compressedExecuteMessage.SetMessage(serializeToJsonString);
            var values = new Dictionary<string, StringBuilder>
            {
                { "RedisSource", source.SerializeToJsonStringBuilder() }
            };
            var catalog = new Mock<IResourceCatalog>();
            var redisSource = new RedisSource();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name)).Returns(redisSource);
            catalog.Setup(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), redisSource, It.IsAny<string>()));
            var saveRedisSource = new SaveRedisSource(catalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = saveRedisSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsFalse(result.HasError);
            catalog.Verify(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name));
            catalog.Verify(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), redisSource, It.IsAny<string>()));
        }

    }
}
