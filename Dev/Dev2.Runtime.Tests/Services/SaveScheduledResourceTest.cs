/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveScheduledResourceTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveScheduledResource = new SaveScheduledResource();

            //------------Execute Test---------------------------
            var resId = saveScheduledResource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveScheduledResource = new SaveScheduledResource();

            //------------Execute Test---------------------------
            var resId = saveScheduledResource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

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
            const string username = "user";
            const string password = "pass";
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
            var res = new ScheduledResource("a", SchedulerStatus.Enabled, DateTime.Now, trigger, "dave", Guid.NewGuid().ToString());
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
