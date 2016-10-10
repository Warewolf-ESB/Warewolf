using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SavePerformanceCountesTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SavePerformanceCounters_Execute")]
        public void SavePerformanceCounters_Execute_WhenValid_ShouldCallManagerSave()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var serializedCounter = serializer.SerializeToBuilder(new PerformanceCounterTo());
            var mockPerfCounterManager = new Mock<IPerformanceCounterRepository>();
            mockPerfCounterManager.Setup(repository => repository.Save(It.IsAny<IPerformanceCounterTo>())).Verifiable();
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(GlobalConstants.ServerWorkspaceID));
            var savePerformanceCounters = new SavePerformanceCounters(mock.Object) { Manager = mockPerfCounterManager.Object };
            var values = new Dictionary<string, StringBuilder> { { "PerformanceCounterTo", serializedCounter } };
            //------------Execute Test---------------------------
            var result = savePerformanceCounters.Execute(values, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            var message = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsNotNull(message);
            Assert.IsFalse(message.HasError);
            mockPerfCounterManager.Verify(repository => repository.Save(It.IsAny<IPerformanceCounterTo>()));
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SavePerformanceCounters_Execute")]
        public void SavePerformanceCounters_Execute_WhenError_ShouldSetMessageWithError()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var serializedCounter = serializer.SerializeToBuilder(new PerformanceCounterTo());
            var mockPerfCounterManager = new Mock<IPerformanceCounterRepository>();
            mockPerfCounterManager.Setup(repository => repository.Save(It.IsAny<IPerformanceCounterTo>())).Throws(new Exception("This call failed"));
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(GlobalConstants.ServerWorkspaceID));
            var savePerformanceCounters = new SavePerformanceCounters(mock.Object) { Manager = mockPerfCounterManager.Object };
            var values = new Dictionary<string, StringBuilder> { { "PerformanceCounterTo", serializedCounter } };
            //------------Execute Test---------------------------
            var result = savePerformanceCounters.Execute(values, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            var message = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsNotNull(message);
            Assert.IsTrue(message.HasError);
            Assert.AreEqual("This call failed", message.GetDecompressedMessage());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SavePerformanceCounters_HandlesType")]
        public void SavePerformanceCounters_HandlesType_ShouldReturnSavePerformanceCounters()
        {
            //------------Setup for test--------------------------
            var savePerformanceCounters = new SavePerformanceCounters();
            
            //------------Execute Test---------------------------
            var handlesType = savePerformanceCounters.HandlesType();
            //------------Assert Results-------------------------
            Assert.AreEqual("SavePerformanceCounters",handlesType);
        }
    }
}
