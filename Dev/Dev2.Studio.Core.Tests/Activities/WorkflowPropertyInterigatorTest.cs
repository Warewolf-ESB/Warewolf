using System;
using Caliburn.Micro;
using Dev2.Studio.Core.Activities.Interegators;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class WorkflowPropertyInterigatorTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowPropertyInterigator_SetActivityProperties")]
        public void WorkflowPropertyInterigator_SetActivityProperties_WhenNullXmlPayload_ExpectSomePropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            var resource = new ResourceModel(env.Object, evtAg);
            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);
            //------------Execute Test---------------------------

            WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsTrue(activity.IsWorkflow);
            Assert.AreEqual("Workflow", activity.Type);
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
