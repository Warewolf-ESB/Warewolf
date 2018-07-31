using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Communication;
using Moq;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class WorkflowResumeTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("WorkflowResume_Execute")]
        public void WorkflowResume_Execute_ExpectTestList()
        {
            //------------Setup for test--------------------------
            var workflowResume = new WorkflowResume();
            var ws = new Mock<IWorkspace>();
            var resID = Guid.NewGuid();
            var values = new Dictionary<string, StringBuilder>
            {
                { "resourceID", new StringBuilder(resID.ToString()) }
            };
            var serializer = new Dev2JsonSerializer();
            var resourceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var res = workflowResume.Execute(values, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(res);
            //------------Assert Results-------------------------
            Assert.IsNotNull(msg);
            Assert.IsFalse(msg.HasError);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("WorkflowResume_Execute")]
        public void WorkflowResume_Returns_HandleType_WorkflowResume()
        {
            //------------Setup for test--------------------------
            var workflowResume = new WorkflowResume();
            //------------Execute Test--------------------------- 
            //------------Assert Results-------------------------
            Assert.AreEqual("WorkflowResume", workflowResume.HandlesType());
        }
    }
}
