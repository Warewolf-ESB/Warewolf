/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class ExecutionDtoExtensionsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesSWAGGER_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IWarewolfResource>();

            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo { VersionNumber = "1.0" };
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            mockResource.Setup(o => o.DataList).Returns(new StringBuilder("<DataList>the test string to be built</DataList>"));

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.SWAGGER);

            var dataListDataFormat = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");

                       var webRequestTO = new WebRequestTO { WebServerUrl = "http://serverName:3142/public/resourceName.api" };

            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
                WebRequestTO = webRequestTO,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            var expectedPayload = "{\r\n  \"openapi\": \"3.0.1\",\r\n  \"info\": {\r\n    \"title\": \"resourceName\",\r\n    \"description\": \"resourceName\",\r\n    \"version\": \"1.0\"\r\n  },\r\n  \"servers\": [\r\n    {\r\n      \"url\": \"http://servername\"\r\n    }\r\n  ],\r\n  \"paths\": {\r\n    \"/public/resourceName\": {\r\n      \"get\": {\r\n        \"tags\": [\r\n          \"\"\r\n        ],\r\n        \"description\": \"\",\r\n        \"parameters\": [],\r\n        \"responses\": {\r\n          \"200\": {\r\n            \"description\": \"Success\",\r\n            \"content\": {\r\n              \"application/json\": {\r\n                \"schema\": {\r\n                  \"type\": \"object\",\r\n                  \"properties\": {}\r\n                }\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }\r\n}";
            Assert.AreEqual(expected: expectedPayload, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesXML_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IWarewolfResource>();

            mockResource.Setup(o => o.DataList).Returns(new StringBuilder("<DataList>the test string to be built</DataList>"));

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.XML);

            var dataListDataFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml");

            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "<DataList />", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesJSON_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IWarewolfResource>();

            mockResource.Setup(o => o.DataList).Returns(new StringBuilder ( "<DataList>the test string to be built</DataList>" ));
            
            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.JSON);
            
            var dataListDataFormat = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            
            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "{}", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_NotWasInternalService_And_IsDebug_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDataObject.Setup(o => o.IsDebug).Returns(true);

            var dataListDataFormat = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");

            var executionDto = new ExecutionDto
            {
                Resource = null,
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: string.Empty, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_WasInternalService_And_ExecuteMessageJSON_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append("{\"test\":\"message\"}");

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "{\"test\":\"message\"}", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_WasInternalService_And_ExecuteMessageXML_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var esbExecuteRequestMessage = "<xml>test message</xml>";
            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append(esbExecuteRequestMessage);

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: esbExecuteRequestMessage, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_NotWasInternalService_And_HasErrors_ShouldReturnError_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(true);

            var esbExecuteRequest = "<xml> Execute Request test message</xml>";

            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append(esbExecuteRequest);

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDataObject.Object,
                DataListFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml"),
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: esbExecuteRequest, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtensions))]
        public void ExecutionDtoExtensions_CreateResponseWriter_WasInternalService_And_ExecuteMessageXML_PayLoadIsNullOrEmpty_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDataObject = new Mock<IDSFDataObject>();

            mockDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var esbExecuteRequestMessage = "<xml>test message</xml>";

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(esbExecuteRequestMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDataObject.Object,
                DataListFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml"),
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "\"<xml>test message</xml>\"", actual: executionDto.PayLoad);
        }

    }
}
