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
    [TestCategory("Runtime Hosting")]
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
        [Owner("Warewolf")]
        [TestCategory("TestDbSourceService_Execute")]
        public void Execute_GivenTestFromDefinitionTrue_UsesPassedDefinition()
        {
            //------------Setup for test--------------------------
            var db = new Mock<IDbSources>();
            DbSource capturedSource = null;
            db.Setup(a => a.DoDatabaseValidation(It.IsAny<DbSource>()))
                .Callback<DbSource>(source => capturedSource = source)
                .Returns(new DatabaseValidationResult { IsValid = true, DatabaseList = new List<string> { "TestDb" } });
            var service = new TestDbSourceService(db.Object);
            var dbSourceDefinition = new DbSourceDefinition
            {
                ServerName = "TestServer",
                UserName = "TestUser",
                Password = "TestPassword",
                AuthenticationType = Common.Interfaces.Runtime.ServiceModel.AuthenticationType.User,
                ConnectionTimeout = 30
            };

            //------------Execute Test---------------------------
            var stringBuilder = service.Execute(new Dictionary<string, StringBuilder>()
            {
                {"DbSource", dbSourceDefinition.SerializeToJsonStringBuilder() },
                {"TestFromDefinition", new StringBuilder("true") }
            }, It.IsAny<IWorkspace>());
            //------------Assert Results-------------------------
            db.Verify(a => a.DoDatabaseValidation(It.IsAny<DbSource>()), Times.Once);
            Assert.IsNotNull(capturedSource);
            Assert.AreEqual("TestServer", capturedSource.Server);
            Assert.AreEqual("TestUser", capturedSource.UserID);
            Assert.AreEqual("TestPassword", capturedSource.Password);
            Assert.AreEqual(30, capturedSource.ConnectionTimeout);
            var message = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.IsFalse(message.HasError);
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory("TestDbSourceService_Execute")]
        public void Execute_GivenTestFromDefinitionFalse_DefaultBehaviorLoadsFromCatalog()
        {
            //------------Setup for test--------------------------
            var db = new Mock<IDbSources>();
            db.Setup(a => a.DoDatabaseValidation(It.IsAny<DbSource>()))
                .Returns(new DatabaseValidationResult { IsValid = true, DatabaseList = new List<string> { "TestDb" } });
            var service = new TestDbSourceService(db.Object);
            var dbSourceDefinition = new DbSourceDefinition
            {
                Id = Guid.NewGuid(),
                ServerName = "TestServer",
                UserName = "TestUser",
                Password = "TestPassword"
            };

            //------------Execute Test---------------------------
            var stringBuilder = service.Execute(new Dictionary<string, StringBuilder>()
            {
                {"DbSource", dbSourceDefinition.SerializeToJsonStringBuilder() },
                {"TestFromDefinition", new StringBuilder("false") }
            }, It.IsAny<IWorkspace>());
            //------------Assert Results-------------------------
            // Since resource won't be found in catalog, it falls back to definition
            db.Verify(a => a.DoDatabaseValidation(It.IsAny<DbSource>()), Times.Once);
            var message = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.IsFalse(message.HasError);
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory("TestDbSourceService_Execute")]
        public void Execute_GivenTestFromDefinitionNotProvided_DefaultBehaviorLoadsFromCatalog()
        {
            //------------Setup for test--------------------------
            var db = new Mock<IDbSources>();
            db.Setup(a => a.DoDatabaseValidation(It.IsAny<DbSource>()))
                .Returns(new DatabaseValidationResult { IsValid = true, DatabaseList = new List<string> { "TestDb" } });
            var service = new TestDbSourceService(db.Object);
            var dbSourceDefinition = new DbSourceDefinition
            {
                Id = Guid.NewGuid(),
                ServerName = "TestServer",
                UserName = "TestUser",
                Password = "TestPassword"
            };

            //------------Execute Test---------------------------
            var stringBuilder = service.Execute(new Dictionary<string, StringBuilder>()
            {
                {"DbSource", dbSourceDefinition.SerializeToJsonStringBuilder() }
            }, It.IsAny<IWorkspace>());
            //------------Assert Results-------------------------
            // Default behavior when TestFromDefinition not provided - tries to load from catalog
            db.Verify(a => a.DoDatabaseValidation(It.IsAny<DbSource>()), Times.Once);
            var message = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.IsFalse(message.HasError);
        }
    }
}
