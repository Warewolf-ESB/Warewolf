using System;
using System.Activities.Expressions;
using System.Text;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Studio.Core.Activities.Interegators;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
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
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg);

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);

            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.IsNull(((Literal<string>)(activity.Type.Expression)).Value);
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
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = new StringBuilder("<Action SourceName=\"TheSource\" Type=\"TheType\" SourceMethod=\"SourceMethod\"></Action>") };
            resource.ServerResourceType = "TheType";
            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);

            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.AreEqual("TheType", ((Literal<string>)(activity.Type.Expression)).Value);
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
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = new StringBuilder("<Action></Action>") };

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);

            //------------Execute Test---------------------------
            WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsFalse(activity.IsWorkflow);
            Assert.IsNull(((Literal<string>)(activity.Type.Expression)).Value);
            Assert.IsNull(activity.FriendlySourceName);
            Assert.IsNull(activity.ActionName);
        }
    }
}
