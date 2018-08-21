using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.ESB.Management.Services;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class WorkflowResumeTests
    {       
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
