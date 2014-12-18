
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
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
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg);

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);
            //------------Execute Test---------------------------

            WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsTrue(activity.IsWorkflow);
            Assert.AreEqual("Workflow", activity.Type.Expression.ToString());
            Assert.AreEqual("My Env", activity.FriendlySourceName.Expression.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowPropertyInterigator_SetActivityProperties")]
        public void WorkflowPropertyInterigator_SetActivityProperties_WhenNotNullXmlPayload_ExpectAllPropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = new StringBuilder("<x><HelpLink>a:\\help.txt</HelpLink></x>") };

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);
            //------------Execute Test---------------------------

            WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsTrue(activity.IsWorkflow);
            Assert.AreEqual("Workflow", activity.Type.Expression.ToString());
            Assert.AreEqual("My Env", activity.FriendlySourceName.Expression.ToString());
            Assert.AreEqual("a:\\help.txt", activity.HelpLink.Expression.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowPropertyInterigator_SetActivityProperties")]
        public void WorkflowPropertyInterigator_SetActivityProperties_WhenNotNullXmlPayloadButNullProperty_ExpectAllPropertiesSet()
        {
            //------------Setup for test--------------------------
            IEventAggregator evtAg = new EventAggregator();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IStudioResourceRepository> exp = new Mock<IStudioResourceRepository>();
            env.Setup(e => e.Name).Returns("My Env");
            var resource = new ResourceModel(env.Object, evtAg) { WorkflowXaml = new StringBuilder("<x><HelpLink2>a:\\help.txt</HelpLink2></x>") };

            var activity = new DsfActivity("FriendlyName", String.Empty, "ServiceName", string.Empty, string.Empty, string.Empty);
            //------------Execute Test---------------------------

            WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);

            //------------Assert Results-------------------------
            Assert.IsTrue(activity.IsWorkflow);
            Assert.AreEqual("Workflow", activity.Type.Expression.ToString());
            Assert.AreEqual("My Env", activity.FriendlySourceName.Expression.ToString());
            Assert.IsNull(activity.HelpLink);
        }

    }
}
