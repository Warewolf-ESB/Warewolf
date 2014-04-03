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
    public class SaveScheduledResourceTest
    {
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void SaveScheduledResourceTest_ServiceName()
        {
            SchedulerTestBaseStaticMethods.SaveScheduledResourceTest_ServiceName("SaveScheduledResourceService", new SaveScheduledResource());
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void GetScheduledResourcesReturnsDynamicService()
        {
            SchedulerTestBaseStaticMethods.GetScheduledResourcesReturnsDynamicService(new SaveScheduledResource());

        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void ScheduledResource_Save_Valid()
        {
            var output = RunOutput(true, true, false);
            Assert.AreEqual(false, output.HasError);

        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void ScheduledResource_Save_InValid()
        {
            var output = RunOutput(false, true, false);
            Assert.AreEqual(true, output.HasError);
            Assert.AreEqual("No Resource Selected", output.Message.ToString());
        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void ScheduledResource_Save_InValidUserCred()
        {
            var output = RunOutput(true, false, false);
            Assert.AreEqual(true, output.HasError);

        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Save")]
        [TestMethod]
        public void ScheduledResource_Save_InValidDeleteExisting()
        {
            var output = RunOutput(true, true, true);
            Assert.AreEqual(false, output.HasError);
        }

        private ExecuteMessage RunOutput(bool expectCorrectInput, bool hasUserNameAndPassword, bool delete)
        {
            string username = "user";
            string password = "pass";
            var esbMethod = new SaveScheduledResource();
            var security = new Mock<ISecurityWrapper>();
            esbMethod.SecurityWrapper = security.Object;
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
                a.CreateModel(GlobalConstants.SchedulerFolderId, It.IsAny<ISecurityWrapper>())).Returns(model.Object);
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            if(expectCorrectInput)
            {

                model.Setup(a => a.Save(It.IsAny<ScheduledResource>(), username, password)).Verifiable();
                inp.Add("Resource", serialiser.SerializeToBuilder(res));
            }

            if(hasUserNameAndPassword)
            {


                inp.Add("UserName", new StringBuilder("user"));
                inp.Add("Password", new StringBuilder("pass"));
            }
            if(delete)
            {
                model.Setup(a => a.DeleteSchedule(It.IsAny<IScheduledResource>())).Verifiable();
                inp.Add("PreviousResource", serialiser.SerializeToBuilder(res));
            }
            esbMethod.SchedulerFactory = factory.Object;

            var output = esbMethod.Execute(inp, ws.Object);
            if(expectCorrectInput && hasUserNameAndPassword)
                model.Verify(a => a.Save(It.IsAny<ScheduledResource>(), username, password));
            if(delete)
            {
                model.Verify(a => a.DeleteSchedule(It.IsAny<IScheduledResource>()));
            }

            return serialiser.Deserialize<ExecuteMessage>(output);

        }



    }
}
