/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Interfaces;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using Warewolf.Storage;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    [TestCategory(nameof(ExecuteExceptionPayload))]
    public class ExecuteExceptionPayloadTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_ShouldDefaultToXML()
        {
            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
               .Returns(new ExecutionEnvironment());
            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual("<Error>\r\n  <Status>501</Status>\r\n  <Title>not_implemented</Title>\r\n  <Message>The method or operation is not implemented.</Message>\r\n</Error>", sut, "If the system was able to get this far then this might be a new behavior NotImplemented currently by Warewolf");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_Given_IsDebug_true_ShouldReturnEmptyString()
        {
            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
               .Returns(new ExecutionEnvironment());
            mockDataObject.Setup(o => o.IsDebug)
                .Returns(true);

            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual(string.Empty, sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_Given_RemoteInvoke_True_And_EmitionTypes_Cover_ShouldReturnJSON()
        {
            var errorMessage = "test error message";
            var env = new ExecutionEnvironment();
            env.Errors.Add(errorMessage);
            env.Errors.Add(errorMessage);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.RemoteInvoke)
                .Returns(true);
            mockDataObject.Setup(o => o.ReturnType)
                .Returns(EmitionTypes.Cover);

            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 400,\r\n    \"Title\": \"bad_request\",\r\n    \"Message\": \"test error message\"\r\n  }\r\n}", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_Given_RemoteInvoke_True_And_EmitionTypes_XML_ShouldReturnXML()
        {
            var errorMessage = "test error message";
            var env = new ExecutionEnvironment();
            env.Errors.Add(errorMessage);
            env.Errors.Add(errorMessage);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.RemoteInvoke)
                .Returns(true);
            mockDataObject.Setup(o => o.ReturnType)
                .Returns(EmitionTypes.XML);

            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual("<Error>\r\n  <Status>400</Status>\r\n  <Title>bad_request</Title>\r\n  <Message>test error message</Message>\r\n</Error>", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_Given_RemoteInvoke_True_And_EmitionTypes_TRX_ShouldReturnXML()
        {
            var errorMessage = "test error message";
            var env = new ExecutionEnvironment();
            env.Errors.Add(errorMessage);
            env.Errors.Add(errorMessage);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.RemoteInvoke)
                .Returns(true);
            mockDataObject.Setup(o => o.ReturnType)
                .Returns(EmitionTypes.TRX);

            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual("<Error>\r\n  <Status>400</Status>\r\n  <Title>bad_request</Title>\r\n  <Message>test error message</Message>\r\n</Error>", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_Given_RemoteNonDebugInvoke_True_And_EmitionTypes_OPENAPI_ShouldReturnJSON()
        {
            var errorMessage = "test error message";
            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.RemoteNonDebugInvoke)
                .Returns(true);
            mockDataObject.Setup(o => o.ReturnType)
                .Returns(EmitionTypes.OPENAPI);
            mockDataObject.Setup(o => o.ExecutionException)
                .Returns(new Exception(errorMessage));

            var sut = ExecuteExceptionPayload.Calculate(mockDataObject.Object);

            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 500,\r\n    \"Title\": \"internal_server_error\",\r\n    \"Message\": \"test error message\"\r\n  }\r\n}", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_CreateErrorResponse_Given_EmmitionTytesOpenAPI_ShouldDefaltToJSON()
        {
            var sut = ExecuteExceptionPayload.CreateErrorResponse(EmitionTypes.OPENAPI, HttpStatusCode.BadRequest, "test_title", "test_message");

            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 400,\r\n    \"Title\": \"test_title\",\r\n    \"Message\": \"test_message\"\r\n  }\r\n}", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_CreateErrorResponse_Given_EmmitionTytesJSON_ShouldReturnJSON()
        {
            var sut = ExecuteExceptionPayload.CreateErrorResponse(EmitionTypes.JSON, HttpStatusCode.BadRequest, "test_title", "test_message");

            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 400,\r\n    \"Title\": \"test_title\",\r\n    \"Message\": \"test_message\"\r\n  }\r\n}", sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void ExecuteExceptionPayload_CreateErrorResponse_Given_EmmitionTytesTRX_ShouldToXML()
        {
            var sut = ExecuteExceptionPayload.CreateErrorResponse(EmitionTypes.TRX, HttpStatusCode.Forbidden, "test_title", "test_message");

            Assert.AreEqual("<Error>\r\n  <Status>403</Status>\r\n  <Title>test_title</Title>\r\n  <Message>test_message</Message>\r\n</Error>", sut);
        }
    }
}
