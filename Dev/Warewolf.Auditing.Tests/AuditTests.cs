using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Auditing;
using Warewolf.Storage.Interfaces;

namespace Warewolf.Tests
{
    [TestClass]
    public class AuditTests
    {
        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(Audit))]
        public void Audit_Audit_CheckEnvironmentVariable_ShouldReturnEnvironment()
        {
            Config.Server.IncludeEnvironmentVariable = true;
            Config.Server.Save();
            
            var executionID = Guid.NewGuid();
            var mockDataObject = SetupDataObjectWithAssignedInputs(executionID);
            var auditLog = new Audit(mockDataObject.Object, "LogAdditionalDetail", "Test", null, null);
            Assert.AreEqual("Not an empty string", auditLog.Environment);
            Assert.AreEqual("Test-Workflow", auditLog.WorkflowName);
        }
        
        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(Audit))]
        public void Audit_Audit_CheckEnvironmentVariable_EmptyEnvironment()
        {
            Config.Server.IncludeEnvironmentVariable = false;
            Config.Server.Save();
            
            var executionID = Guid.NewGuid();
            var mockDataObject = SetupDataObjectWithAssignedInputs(executionID);
            var auditLog = new Audit(mockDataObject.Object, "LogAdditionalDetail", "Test", null, null);
            Assert.AreEqual("", auditLog.Environment);
            Assert.AreEqual("Test-Workflow", auditLog.WorkflowName);
        }

        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        [TestCategory(nameof(Audit))]
        public void Audit_Audit_CheckForException_ShouldReturnErroLogLEvel()
        {
            var executionID = Guid.NewGuid();
            var mockDataObject = SetupDataObjectWithAssignedInputs(executionID);
            var auditLog = new Audit(mockDataObject.Object, "LogAdditionalDetail", "Test", null, null, new SerializableException());
            Assert.AreEqual(Logging.LogLevel.Error, auditLog.LogLevel);
        }


        Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid executionId)
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            var mock = new Mock<IExecutionEnvironment>();
            mock.Setup(o => o.ToJson()).Returns("Not an empty string");
            mockedDataObject.Setup(o => o.Environment).Returns(mock.Object);
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Test-Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => Guid.NewGuid());
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }
    }
}
