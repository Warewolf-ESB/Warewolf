/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Communication;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Core;
using Warewolf.Data.Options;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveWebServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------
            var resId = saveWebService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------
            var resId = saveWebService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_CreateServiceEntry_Returns_New_Dynamic_Service_DeleteAllTestsService()
        {
            //------------Setup for test-------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------
            var dynamicService = saveWebService.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_SavePluginService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("SaveWebService", saveWebService.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> {{"item", new StringBuilder()}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> {{"resourceID", new StringBuilder("ABCDE")}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_TestDefinitionsNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
                {{"resourceID", new StringBuilder(Guid.NewGuid().ToString())}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ItemToDeleteNotListOfServiceTestTO_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(Guid.NewGuid().ToString())},
                {"testDefinitions", new StringBuilder("This is not deserializable to ServerExplorerItem")}
            };
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_FormDataParameters_NotNull_ExpectSuccess()
        {
            var testFileKey = "testFileKey";
            var testFileName = "testFileName";
            var testFileValue = "testFileContent";

            var testTextKey = "testTextKey";
            var testTextValue = "testTextValue";

            var resourcePath = "c:\\path/to/save/resource.ext";

            var serializer = new Dev2JsonSerializer();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<WebSource>(Guid.Empty, Guid.Empty))
                .Returns(new Mock<WebSource>().Object)
                .Verifiable();
            mockResourceCatalog.Setup(o => o.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()))
                .Verifiable();

            var mockExplorerServerResourceRepository = new Mock<IExplorerServerResourceRepository>();
            mockExplorerServerResourceRepository.Setup(o => o.UpdateItem(It.IsAny<IResource>()))
                .Verifiable();

            var saveWebService = new SaveWebService
            {
                ResourceCatalogue = mockResourceCatalog.Object,
                ServerExplorerRepo = mockExplorerServerResourceRepository.Object
            };

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(o => o.Address).Returns("http://address.co.za/examples");
            mockWebSource.Setup(o => o.Client).Returns(mockWebClientWrapper.Object);
            mockWebSource.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Anonymous);

            var serviceOutputMapping = new ServiceOutputMapping("from", "=", "[[rec().a]]");
            var webService = new WebServiceDefinition
            {
                Path = resourcePath,
                Source = new WebServiceSourceDefinition(),
                OutputMappings = new List<IServiceOutputMapping> { serviceOutputMapping },
                Headers = new List<INameValue>(),
                FormDataParameters = new List<IFormDataParameters>
                {
                    new FormDataConditionExpression
                    { 
                        Key = testFileKey,
                        Cond = new FormDataConditionFile
                        {
                            FileBase64 = testFileValue,
                            FileName = testFileName
                        }
                    }.ToFormDataParameter(),
                    new FormDataConditionExpression
                    {
                        Key = testTextKey,
                        Cond = new FormDataConditionText
                        {
                            Value = testTextValue
                        }
                    }.ToFormDataParameter()
                },
                IsManualChecked = false,
                IsFormDataChecked = true,
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { "Webservice", serializer.SerializeToBuilder(webService) }
            };

            var result = saveWebService.Execute(values, null);
            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);

            Assert.IsNotNull(result);
            Assert.IsFalse(executeMessage.HasError);
            mockResourceCatalog.Verify(o => o.GetResource<WebSource>(Guid.Empty, Guid.Empty), Times.Once);
            mockResourceCatalog.Verify(o => o.SaveResource(Guid.Empty, It.IsAny<IResource>(), resourcePath), Times.Once);
            mockExplorerServerResourceRepository.Verify(o => o.UpdateItem(It.IsAny<IResource>()), Times.Once);
        }
    }
}