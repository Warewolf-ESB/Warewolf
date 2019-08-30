/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.WebServerTests
{
    [TestClass]
    public class ExecutionDtoExtentionsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesSWAGGER_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IResource>();

            mockResource.Setup(o => o.DataList).Returns(new StringBuilder("<DataList>the test string to be built</DataList>"));

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDSFDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.SWAGGER);

            var dataListDataFormat = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");

            var webRequestTO = new WebRequestTO { WebServerUrl = "http://serverName:3142/public/resourceName.api" };

            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
                WebRequestTO = webRequestTO,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "{\r\n  \"swagger\": 2,\r\n  \"info\": {\r\n    \"title\": null,\r\n    \"description\": \"\",\r\n    \"version\": \"1.0.0\"\r\n  },\r\n  \"host\": \":0/\",\r\n  \"basePath\": \"/\",\r\n  \"schemes\": [\r\n    \"http\"\r\n  ],\r\n  \"produces\": [\r\n    \"application/json\",\r\n    \"application/xml\"\r\n  ],\r\n  \"paths\": {\r\n    \"serviceName\": \"/public/resourceName.api\",\r\n    \"get\": {\r\n      \"summary\": \"\",\r\n      \"description\": \"\",\r\n      \"parameters\": []\r\n    }\r\n  },\r\n  \"responses\": {\r\n    \"200\": {\r\n      \"schema\": {\r\n        \"$ref\": \"#/definition/Output\"\r\n      }\r\n    }\r\n  },\r\n  \"definitions\": {\r\n    \"Output\": {\r\n      \"type\": \"object\",\r\n      \"properties\": {}\r\n    }\r\n  }\r\n}", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesXML_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IResource>();

            mockResource.Setup(o => o.DataList).Returns(new StringBuilder("<DataList>the test string to be built</DataList>"));

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDSFDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.XML);

            var dataListDataFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml");

            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "<DataList />", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_NotWasInternalService_And_EmitionTypesJSON_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResource = new Mock<IResource>();

            mockResource.Setup(o => o.DataList).Returns(new StringBuilder ( "<DataList>the test string to be built</DataList>" ));
            
            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDSFDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.JSON);
            
            var dataListDataFormat = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            
            var executionDto = new ExecutionDto
            {
                Resource = mockResource.Object,
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "{}", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_NotWasInternalService_And_IsDebug_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);

            var dataListDataFormat = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");

            var executionDto = new ExecutionDto
            {
                Resource = null,
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                DataListFormat = dataListDataFormat,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: string.Empty, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_WasInternalService_And_ExecuteMessageJSON_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append("{\"test\":\"message\"}");

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "{\"test\":\"message\"}", actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_WasInternalService_And_ExecuteMessageXML_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var esbExecuteRequestMessage = "<xml>test message</xml>";
            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append(esbExecuteRequestMessage);

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDSFDataObject.Object,
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: esbExecuteRequestMessage, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_NotWasInternalService_And_HasErrors_ShouldReturnError_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(true);

            var esbExecuteRequest = "<xml> Execute Request test message</xml>";

            var executeMessage = new ExecuteMessage();
            executeMessage.Message.Append(esbExecuteRequest);

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(executeMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDSFDataObject.Object,
                DataListFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml"),
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: esbExecuteRequest, actual: executionDto.PayLoad);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExecutionDtoExtentions))]
        public void ExecutionDtoExtentions_CreateResponseWriter_WasInternalService_And_ExecuteMessageXML_PayLoadIsNullOrEmpty_Success()
        {
            //-------------------------------Arrange----------------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(false);

            var esbExecuteRequestMessage = "<xml>test message</xml>";

            var jsonSerializer = new Dev2JsonSerializer();
            var serExecuteMessage = jsonSerializer.Serialize(esbExecuteRequestMessage);

            var executionDto = new ExecutionDto
            {
                DataObject = mockDSFDataObject.Object,
                DataListFormat = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "application/xml"),
                ErrorResultTO = new ErrorResultTO(),
                Request = new EsbExecuteRequest { WasInternalService = true, ExecuteResult = new StringBuilder(serExecuteMessage) },
                Serializer = jsonSerializer,
            };

            var executionDtoExtentions = new ExecutionDtoExtentions(executionDto);

            //-------------------------------Act--------------------------------------
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());

            //-------------------------------Assert-----------------------------------
            Assert.AreEqual(expected: "\"<xml>test message</xml>\"", actual: executionDto.PayLoad);
        }

    }
}
