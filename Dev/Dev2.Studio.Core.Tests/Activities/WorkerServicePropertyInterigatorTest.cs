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
    public class WorkerServicePropertyInterigatorTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkerServicePropertyInterigator_SetActivityProperties")]
        public void WorkerServicePropertyInterigator_SetActivityProperties_WhenNullXML_ExpectSomePropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg);

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);
            
            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.IsNull(activity.Type);
            Assert.IsNull(activity.FriendlySourceName);
            Assert.IsNull(activity.ActionName);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkerServicePropertyInterigator_SetActivityProperties")]
        public void WorkerServicePropertyInterigator_SetActivityProperties_WhenNotNullXML_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = "<Action SourceName=\"TheSource\" Type=\"TheType\" SourceMethod=\"SourceMethod\"></Action>" };

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);

            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.AreEqual("TheType",activity.Type.Expression.ToString());
            Assert.AreEqual("TheSource", activity.FriendlySourceName.Expression.ToString());
            Assert.AreEqual("SourceMethod", activity.ActionName.Expression.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkerServicePropertyInterigator_SetActivityProperties")]
        public void WorkerServicePropertyInterigator_SetActivityProperties_WhenXMLWithOutAttributes_ExpectSomePropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = "<Action></Action>" };

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);

            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.IsNull(activity.Type);
            Assert.IsNull(activity.FriendlySourceName);
            Assert.IsNull(activity.ActionName);
        }
    }
}
