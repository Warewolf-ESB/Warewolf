/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common.ExtMethods;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Core;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Data.Options;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.Interfaces;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestWebServiceTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(TestWebService))]
        public void TestWebService_Execute_WebService_NotFound_ExpectHasError()
        {
            var serializer = new Dev2JsonSerializer();

            var sut = new TestWebService();

            var result = sut.Execute(new Dictionary<string, StringBuilder>(), new Workspace(Guid.Empty));

            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(executeMessage.HasError);
            Assert.IsTrue(executeMessage.Message.Contains("Parameter name: WebService"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void TestWebService_Execute_Source_NotFound_ExpectHasError()
        {
            var serializer = new Dev2JsonSerializer();
            var sut = new TestWebService();

            var webService = new WebServiceDefinition();
            
            var values = new Dictionary<string, StringBuilder> 
            { 
                { "WebService", webService.SerializeToJsonStringBuilder() } 
            };

            var result = sut.Execute(values, null);

            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(executeMessage.HasError);
            Assert.IsTrue(executeMessage.Message.Contains("Parameter name: source"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void TestWebService_Execute_FormDataParameters_Null_ExpectSuccess()
        {
            var serializer = new Dev2JsonSerializer();

            var mockWebServices = new Mock<IWebServices>();
            var sut = new TestWebService
            {
                WebServices = mockWebServices.Object,
                ResourceCatalogue = new Mock<IResourceCatalog>().Object
            };

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(o => o.Address)
                .Returns("http://address.co.za/examples");
            mockWebSource.Setup(o => o.Client)
                .Returns(mockWebClientWrapper.Object); //Ensure this is not instantiated here
            mockWebSource.Setup(o => o.AuthenticationType)
                .Returns(AuthenticationType.Anonymous);

            var webService = new Warewolf.Core.WebServiceDefinition()
            {
                Source = new WebServiceSourceDefinition(mockWebSource.Object),
                Headers = new List<INameValue> { new NameValue("Content-Type", "multipart/form-data") },
                FormDataParameters = null
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { "WebService", webService.SerializeToJsonStringBuilder() }
            };

            var result = sut.Execute(values, null);

            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);

            Assert.IsNotNull(result);
            Assert.IsFalse(executeMessage.HasError, "FormDataParameters can be null");
            Assert.IsFalse(executeMessage.Message.ToString().Contains("Value cannot be null.\r\nParameter name: source"));

            mockWebServices.Verify(o => o.TestWebService(It.IsAny<WebService>()), Times.Once, "Some request do not set FormDataParameters and hence it is not required field");
            mockWebSource.Verify(o => o.Client, Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void TestWebService_Execute_FormDataParameters_NotNull_ExpectSuccess()
        {
            var testFileKey = "testFileKey";
            var testFileName = "testFileName";
            var testFileContent = "this can be any file type parsed into base64 string";
            var testFileContentBytes = testFileContent.ToBytesArray();
            var testFileContentBase64 = testFileContentBytes.ToBase64String();

            var serializer = new Dev2.Communication.Dev2JsonSerializer();

            var mockWebServices = new Mock<IWebServices>();

            var sut = new TestWebService
            {
                WebServices = mockWebServices.Object,
                ResourceCatalogue = new Mock<IResourceCatalog>().Object
            };

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(o => o.Address)
                .Returns("http://address.co.za/examples");
            mockWebSource.Setup(o => o.Client)
                .Returns(mockWebClientWrapper.Object); //Ensure this is not instanciated here
            mockWebSource.Setup(o => o.AuthenticationType)
                .Returns(AuthenticationType.Anonymous);

            var webService = new Warewolf.Core.WebServiceDefinition()
            {
                Source = new WebServiceSourceDefinition(mockWebSource.Object),
                Headers = new List<INameValue> { new NameValue("Content-Type", "multipart/form-data") },
                IsManualChecked = false,
                IsFormDataChecked = true,
                FormDataParameters = new List<IFormDataParameters>
                {
                   new FormDataConditionExpression
                   {
                       Key = testFileKey,
                       Cond = new FormDataConditionFile
                       {
                           FileBase64 = testFileContentBase64,
                           FileName = testFileName
                       }
                   }.ToFormDataParameter()
                }

            };
            
            var values = new Dictionary<string, StringBuilder>
            {
                { "WebService", serializer.SerializeToBuilder(webService) }
            };

            var result = sut.Execute(values, null);

            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);

            Assert.IsNotNull(result);
            Assert.IsFalse(executeMessage.HasError);
            var message = executeMessage.Message.ToString();
            Assert.IsTrue(message.Contains("\"Headers\":[{\"$id\":\"2\",\"$type\":\"Dev2.Common.Interfaces.NameValue, Dev2.Common.Interfaces\",\"Name\":\"Content-Type\",\"Value\":\"multipart/form-data\"}]"));
            Assert.IsTrue(message.Contains("\"FileName\":\"testFileName\",\"ContentType\":null,\"FileBase64\":\"dGhpcyBjYW4gYmUgYW55IGZpbGUgdHlwZSBwYXJzZWQgaW50byBiYXNlNjQgc3RyaW5n\",\"Key\":\"testFileKey\""));
            Assert.IsTrue(message.Contains("\"IsFormDataChecked\":true,\"IsManualChecked\":false,"));
            mockWebServices.Verify(o => o.TestWebService(It.IsAny<WebService>()), Times.Once);
            mockWebSource.Verify(o => o.Client, Times.Never);
        }

    }
}
