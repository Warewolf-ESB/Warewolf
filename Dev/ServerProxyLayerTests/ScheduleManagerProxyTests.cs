using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class ScheduleManagerProxyTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduleManagerProxy_SaveSchedule")]
        // ReSharper disable InconsistentNaming
        public void ScheduleManagerProxy_SaveSchedule_PassThrough_ExpectSuccess()
        
        {
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            //------------Setup for test--------------------------
            var scheduleManagerProxy = new ScheduleManagerProxy(factory.Object,connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns("-1");
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveScheduledResourceService")).Returns(controller.Object);
            //------------Execute Test---------------------------
            scheduleManagerProxy.SaveSchedule(new Mock<IScheduledResource>().Object,"userName","password");
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("Resource", It.IsAny<StringBuilder>()),Times.Once());
            controller.Verify(a => a.AddPayloadArgument("UserName", "userName"),Times.Once());
            controller.Verify(a => a.AddPayloadArgument("Password", "password"),Times.Once());
            factory.Verify(a => a.CreateController("SaveScheduledResourceService"), Times.Once());
            controller.Verify(a=>a.ExecuteCommand<string>(connection.Object,GlobalConstants.ServerWorkspaceID));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduleManagerProxy_DeleteSchedule")]
        public void ScheduleManagerProxy_DeleteSchedule_PassThrough_ExpectSuccess()
        {
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            //------------Setup for test--------------------------
            var scheduleManagerProxy = new ScheduleManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns("-1");
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("DeleteScheduledResourceService")).Returns(controller.Object);
            //------------Execute Test---------------------------
            scheduleManagerProxy.Delete( "ScheduleName", "SchedulePath");
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("ScheduleName", "ScheduleName"), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("SchedulePath", "SchedulePath"), Times.Once());
            factory.Verify(a => a.CreateController("DeleteScheduledResourceService"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduleManagerProxy_GetSchedules")]
        public void ScheduleManagerProxy_GetSchedules_PassThrough_ExpectSuccess()
        {
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var output = new List<IScheduledResource>();
            //------------Setup for test--------------------------
            var scheduleManagerProxy = new ScheduleManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IList<IScheduledResource>>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(output);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("GetScheduledResources")).Returns(controller.Object);
            //------------Execute Test---------------------------
            var returned = scheduleManagerProxy.FetchSchedules();
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            controller.Verify(a => a.AddPayloadArgument(It.IsAny<string>(), It.IsAny<StringBuilder>()), Times.Never());
            factory.Verify(a => a.CreateController("GetScheduledResources"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IList<IScheduledResource>>(connection.Object, GlobalConstants.ServerWorkspaceID));
            Assert.AreEqual(output, returned);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduleManagerProxy_FetchScheduleHistory")]
        public void ScheduleManagerProxy_FetchScheduleHistory_PassThrough_ExpectSuccess()
        {
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var outputFromService = new List<IResourceHistory>();
            //------------Setup for test--------------------------
            var scheduleManagerProxy = new ScheduleManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IList<IResourceHistory>>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(outputFromService);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("GetScheduledResourceHistoryService")).Returns(controller.Object);
            //------------Execute Test---------------------------
            var history =scheduleManagerProxy.FetchScheduleHistory("ScheduleName", "SchedulePath");
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("ScheduleName", "ScheduleName"), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("SchedulePath", "SchedulePath"), Times.Once());
            factory.Verify(a => a.CreateController("GetScheduledResourceHistoryService"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IList<IResourceHistory>>(connection.Object, GlobalConstants.ServerWorkspaceID));
            Assert.AreEqual(outputFromService, history);
        }
        // ReSharper restore InconsistentNaming
    }
}
