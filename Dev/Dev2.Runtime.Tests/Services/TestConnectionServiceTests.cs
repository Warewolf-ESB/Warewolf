using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestConnectionServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var comPluginActions = new TestConnectionService();

            //------------Execute Test---------------------------
            var resId = comPluginActions.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchComPluginActions = new TestConnectionService();

            //------------Execute Test---------------------------
            var resId = fetchComPluginActions.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestConnectionService_HandlesType")]
        public void TestConnectionService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var directDeploy = new TestConnectionService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("TestConnectionService", directDeploy.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestConnectionService_Execute")]
        public void TestConnectionService_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var connectionService = new TestConnectionService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = connectionService.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestConnectionService_Execute")]
        public void TestConnectionService_Execute_TestInvalid_ExpectInvalid()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IConnections>();
            var connectionService = new TestConnectionService(mock.Object);
            Dev2JsonSerializer dev2JsonSerializer = new Dev2JsonSerializer();
            
            mock.Setup(connections => connections.CanConnectToServer(It.IsAny<Connection>()))
                .Returns(new DatabaseValidationResult()
                {
                    IsValid = false,
                    ErrorMessage = "error"
                });
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var serverSource = new ServerSource();
            StringBuilder jsonResult = connectionService.Execute(new Dictionary<string, StringBuilder>()
            {
                {"ServerSource", dev2JsonSerializer.SerializeToBuilder(serverSource) }
            }, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("error", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestConnectionService_Execute")]
        public void TestConnectionService_Execute_TestISvalid_ExpectValid()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IConnections>();
            var connectionService = new TestConnectionService(mock.Object);
            Dev2JsonSerializer dev2JsonSerializer = new Dev2JsonSerializer();

            mock.Setup(connections => connections.CanConnectToServer(It.IsAny<Connection>()))
                .Returns(new DatabaseValidationResult()
                {
                    IsValid = true,
                });
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var serverSource = new ServerSource();
            StringBuilder jsonResult = connectionService.Execute(new Dictionary<string, StringBuilder>()
            {
                {"ServerSource", dev2JsonSerializer.SerializeToBuilder(serverSource) }
            }, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
            Assert.AreEqual("", result.Message.ToString());
        }
    }
}
