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

            Assert.AreEqual("<ExecutionError><MessageId>NotImplemented</MessageId><Message>Please consult system Administrator.</Message></ExecutionError>", sut, "If the system was able to get this far then this might be a behavior NotImplemented currently by Warewolf");
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

            Assert.AreEqual("{\"ExecutionError\":{\"MessageId\":\"BadRequest\",\"Message\":\"test error message\"}}", sut);
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

            Assert.AreEqual("<ExecutionError><MessageId>BadRequest</MessageId><Message>test error message</Message></ExecutionError>", sut);
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

            Assert.AreEqual("<ExecutionError><MessageId>BadRequest</MessageId><Message>test error message</Message></ExecutionError>", sut);
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

            Assert.AreEqual("{\"ExecutionError\":{\"MessageId\":\"InternalServerError\",\"Message\":\"test error message\"}}", sut);
        }
    }
}
