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
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Serialization;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveElasticsearchSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var elasticsearchSource = new SaveElasticsearchSource();
            //------------Execute Test---------------------------
            var resId = elasticsearchSource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var elasticsearchSource = new SaveElasticsearchSource();
            //------------Execute Test---------------------------
            var resId = elasticsearchSource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var elasticsearchSource = new SaveElasticsearchSource();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("SaveElasticsearchSource", elasticsearchSource.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var elasticsearchSource = new SaveElasticsearchSource();
            //------------Execute Test---------------------------
            var dynamicService = elasticsearchSource.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var elasticsearchSource = new SaveElasticsearchSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = elasticsearchSource.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var elasticsearchSource = new SaveElasticsearchSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = elasticsearchSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_Execute_GivenResourceDefination_ShouldSaveNewSourceReturnResourceDefinationMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var source = new ElasticsearchSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = "HostName",
                Port = "3679",
                AuthenticationType = AuthenticationType.Anonymous
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            var serializeToJsonString = source.SerializeToJsonString(new DefaultSerializationBinder());
            compressedExecuteMessage.SetMessage(serializeToJsonString);
            var values = new Dictionary<string, StringBuilder>
            {
                { "ElasticsearchSource", source.SerializeToJsonStringBuilder() }
            };
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name));
            catalog.Setup(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()));
            var elasticsearchSource = new SaveElasticsearchSource(catalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = elasticsearchSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsFalse(result.HasError);
            catalog.Verify(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_Execute_GivenResourceDefination_GivenExising_ShouldReturnResourceDefinationMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var source = new ElasticsearchSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = "HostName",
                Port = "3679",
                AuthenticationType = AuthenticationType.Anonymous
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            var serializeToJsonString = source.SerializeToJsonString(new DefaultSerializationBinder());
            compressedExecuteMessage.SetMessage(serializeToJsonString);
            var values = new Dictionary<string, StringBuilder>
            {
                { "ElasticsearchSource", source.SerializeToJsonStringBuilder() }
            };
            var catalog = new Mock<IResourceCatalog>();
            var elasticsearchSource = new ElasticsearchSource();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource(It.IsAny<Guid>(), source.Name)).Returns(elasticsearchSource);
            catalog.Setup(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), elasticsearchSource, It.IsAny<string>()));
            var saveElasticsearchSource = new SaveElasticsearchSource(catalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = saveElasticsearchSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsFalse(result.HasError);
            catalog.Verify(resourceCatalog => resourceCatalog.SaveResource(It.IsAny<Guid>(), elasticsearchSource, It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveElasticsearchSource))]
        public void SaveElasticsearchSource_GivenResourceDefinition_ShouldSave()
        {
            //----------------------Arrange----------------------
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var elasticsearchSourceDefinition = new ElasticsearchSourceDefinition()
            {
                Name = "ElasticsearchSource",
                HostName = "testHost",
                Password = "testPaass",
                AuthenticationType = AuthenticationType.Password
            };
            
            mockResourceCatalog.Setup(o => o.SaveResource(It.IsAny<Guid>(), elasticsearchSourceDefinition.SerializeToJsonStringBuilder(), string.Empty));

            var sut = new SaveElasticsearchSource(mockResourceCatalog.Object);
            //----------------------Act--------------------------
            var result = sut.Execute(new Dictionary<string, StringBuilder> { { "ElasticsearchSource", elasticsearchSourceDefinition.SerializeToJsonStringBuilder() } }, new Mock<IWorkspace>().Object);
            //----------------------Assert-----------------------
            var serializer = new Dev2JsonSerializer();

            Assert.IsFalse(serializer.Deserialize<ExecuteMessage>(result).HasError);
            mockResourceCatalog.Verify(o => o.SaveResource(It.IsAny<Guid>(), It.IsAny<ElasticsearchSource>(), It.IsAny<string>()), Times.Once);

        }

    }
}
