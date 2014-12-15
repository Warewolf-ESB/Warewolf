
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Scheduler;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetScheduledResourcesTest
    {


        private Mock<IServerSchedulerFactory> _factory;


        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Get")]
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetScheduledResources_Execute_ReturnsScheduledResources()

        {
            var output = RunOutput();

            var result = JsonConvert.DeserializeObject<ObservableCollection<ScheduledResource>>(output.ToString(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual("a", result.First().Name);
        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Get")]
        [TestMethod]
        public void GetScheduledResources_ServiceName()
        {
            var esbMethod = new GetScheduledResources();
            Assert.AreEqual("GetScheduledResources", esbMethod.HandlesType());


        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Get")]
        [TestMethod]
        public void GetScheduledResourcesReturnsDynamicService()
        {
            var esb = new GetScheduledResources();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }


        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_Get")]
        [TestMethod]
        public void GetScheduledResources_Execute_ReturnsTrigger()
        {
            var output = RunOutput();


            var result = JsonConvert.DeserializeObject<ObservableCollection<ScheduledResource>>(output.ToString(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            var first = result.First().Trigger;
            Assert.IsNotNull(first.Trigger);
            var dailyTrigger = first.Trigger.Instance as DailyTrigger;
            if(dailyTrigger != null)
            {
                Assert.AreEqual(21, dailyTrigger.DaysInterval);
            }
        }

        private StringBuilder RunOutput()
        {
            var esbMethod = new GetScheduledResources();
            _factory = new Mock<IServerSchedulerFactory>();
            var security = new Mock<ISecurityWrapper>();
            esbMethod.SecurityWrapper = security.Object;
            var model = new Mock<IScheduledResourceModel>();
            var ws = new Mock<IWorkspace>();
            var trigger = new ScheduleTrigger(TaskState.Disabled,
                                              new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger(21)),
                                              new Dev2TaskService(new TaskServiceConvertorFactory()),
                                              new TaskServiceConvertorFactory());
            var res = new ScheduledResource("a", SchedulerStatus.Enabled, DateTime.Now, trigger, "dave");
            _factory.Setup(
                a =>
                a.CreateModel(GlobalConstants.SchedulerFolderId, It.IsAny<ISecurityWrapper>())).Returns(model.Object);
            model.Setup(a => a.GetScheduledResources()).Returns(new ObservableCollection<IScheduledResource>() { res });

            esbMethod.SchedulerFactory = _factory.Object;
            var output = esbMethod.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            return output;
        }
    }
    // ReSharper restore InconsistentNaming
}
