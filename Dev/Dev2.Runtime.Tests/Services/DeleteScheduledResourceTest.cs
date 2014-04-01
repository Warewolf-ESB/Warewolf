using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteScheduledResourceTest
    {

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Delete")]
        [TestMethod]
        public void SaveScheduledResourceTest_ServiceName()
        {
            SchedulerTestBaseStaticMethods.SaveScheduledResourceTest_ServiceName("DeleteScheduledResourceService", new DeleteScheduledResource());
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Delete")]
        [TestMethod]
        public void GetScheduledResourcesReturnsDynamicService()
        {
            SchedulerTestBaseStaticMethods.GetScheduledResourcesReturnsDynamicService(new DeleteScheduledResource());

        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Delete")]
        [TestMethod]
        public void ScheduledResource_DeleteValid()
        {
            var output = RunOutput(true);
            Assert.AreEqual(false, output.HasError);

        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Delete")]
        [TestMethod]
        public void ScheduledResource_DeleteInValid()
        {
            var output = RunOutput(false);
            Assert.AreEqual(true, output.HasError);
            Assert.AreEqual("No Resource Selected", output.Message.ToString());
        }

        private ExecuteMessage RunOutput(bool expectCorrectInput)
        {
            var esbMethod = new DeleteScheduledResource();
            var factory = new Mock<IServerSchedulerFactory>();
            var model = new Mock<IScheduledResourceModel>();
            var ws = new Mock<IWorkspace>();
            var trigger = new ScheduleTrigger(TaskState.Disabled,
                                              new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger(21)),
                                              new Dev2TaskService(new TaskServiceConvertorFactory()),
                                              new TaskServiceConvertorFactory());
            var res = new ScheduledResource("a", SchedulerStatus.Enabled, DateTime.Now, trigger, "dave");
            Dictionary<string, StringBuilder> inp = new Dictionary<string, StringBuilder>();
            factory.Setup(
                a =>
                a.CreateModel(GlobalConstants.SchedulerFolderId)).Returns(model.Object);
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            if(expectCorrectInput)
            {

                model.Setup(a => a.DeleteSchedule(It.IsAny<ScheduledResource>())).Verifiable();
                inp.Add("Resource", serialiser.SerializeToBuilder(res));
            }

            esbMethod.SchedulerFactory = factory.Object;

            var output = esbMethod.Execute(inp, ws.Object);
            if(expectCorrectInput)
                model.Verify(a => a.DeleteSchedule(It.IsAny<ScheduledResource>()));
            return serialiser.Deserialize<ExecuteMessage>(output);

        }

    }
}
