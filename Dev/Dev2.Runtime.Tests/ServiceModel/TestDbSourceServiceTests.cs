using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class TestDbSourceServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------

            var service = new TestDbSourceService();

            //------------Execute Test---------------------------
            var resId = service.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var service = new TestDbSourceService();

            //------------Execute Test---------------------------
            var resId = service.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestDbSourceService_HandlesType")]
        public void TestDbSourceService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var service = new TestDbSourceService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("TestDbSourceService", service.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestDbSourceService_HandlesType")]
        public void Execute_Problemtestingconnection_GivenNullBrokerResult_ReturnsError()
        {
            //------------Setup for test--------------------------
            var db = new Mock<IDbSources>();
            db.Setup(a => a.DoDatabaseValidation(It.IsAny<DbSource>())).Returns(default(DatabaseValidationResult));
            var service = new TestDbSourceService(db.Object);
            var dbSourceDefinition = new DbSourceDefinition();

            //------------Execute Test---------------------------
            var stringBuilder = service.Execute(new Dictionary<string, StringBuilder>()
            {
                {"DbSource",dbSourceDefinition.SerializeToJsonStringBuilder() }
            }, It.IsAny<IWorkspace>());
            //------------Assert Results-------------------------
            Assert.AreEqual("TestDbSourceService", service.HandlesType());
            db.Verify(a => a.DoDatabaseValidation(It.IsAny<DbSource>()), Times.Once);
            var message = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.AreEqual("Problem testing connection.", message.Message.ToString());
            Assert.IsTrue(message.HasError);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestDbSourceService_HandlesType")]
        public void Execute_Problemtestingconnection_GivenThrowsException_ReturnsActualError()
        {
            //------------Setup for test--------------------------
            var db = new Mock<IDbSources>();
            db.Setup(a => a.DoDatabaseValidation(It.IsAny<DbSource>())).Throws(new Exception("Problem testing connection."));
            var service = new TestDbSourceService(db.Object);
            var dbSourceDefinition = new DbSourceDefinition();

            //------------Execute Test---------------------------
            var stringBuilder = service.Execute(new Dictionary<string, StringBuilder>()
            {
                {"DbSource",dbSourceDefinition.SerializeToJsonStringBuilder() }
            }, It.IsAny<IWorkspace>());
            //------------Assert Results-------------------------
            Assert.AreEqual("TestDbSourceService", service.HandlesType());
            db.Verify(a => a.DoDatabaseValidation(It.IsAny<DbSource>()), Times.Once);
            var message = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.AreEqual("Problem testing connection.", message.Message.ToString());
            Assert.IsTrue(message.HasError);
        }
    }
}
