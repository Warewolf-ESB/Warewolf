
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Scheduler;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetScheduledResourceHistoryTest
    {
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void SaveScheduledResourceTest_ServiceName()
        {
            SchedulerTestBaseStaticMethods.SaveScheduledResourceTest_ServiceName("GetScheduledResourceHistoryService", new GetScheduledResourceHistory());
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [TestMethod]
        public void Services_ScheduledResource_ReturnsDynamicService()
        {
            SchedulerTestBaseStaticMethods.GetScheduledResourcesReturnsDynamicService(new GetScheduledResourceHistory());

        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [TestMethod]
        public void Services_ScheduledResourceHistory_GetValid()
        {
            var output = RunOutput(true);


            var lst = output;
            Assert.AreEqual(1, lst.Count);
            Assert.AreEqual(TimeSpan.MaxValue, lst.First().TaskHistoryOutput.Duration);
            Assert.AreEqual(DateTime.MinValue, lst.First().TaskHistoryOutput.StartDate);
            Assert.AreEqual(DateTime.MaxValue, lst.First().TaskHistoryOutput.EndDate);
            Assert.IsNotNull(lst.First().DebugOutput);
        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [TestMethod]
        public void Services_ScheduledResource_GetIncorrectResources()
        {
            var output = RunOutput(false);
            Assert.AreEqual(0, output.Count);

        }

        private List<IResourceHistory> RunOutput(bool expectCorrectInput)
        {
            var esbMethod = new GetScheduledResourceHistory();
            var security = new Mock<ISecurityWrapper>();
            esbMethod.SecurityWrapper = security.Object;
            var factory = new Mock<IServerSchedulerFactory>();
            var model = new Mock<IScheduledResourceModel>();
            var ws = new Mock<IWorkspace>();
            var history = new List<IResourceHistory>
                {
                    new ResourceHistory("", new List<IDebugState> {new DebugState()},
                                        new EventInfo(DateTime.MinValue, TimeSpan.MaxValue, DateTime.MaxValue, ScheduleRunStatus.Error, "115"),
                                        "leon")
                };
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

                model.Setup(a => a.CreateHistory(It.IsAny<ScheduledResource>())).Returns(history).Verifiable();
                inp.Add("Resource", serialiser.SerializeToBuilder(res));
            }

            esbMethod.SchedulerFactory = factory.Object;

            var output = esbMethod.Execute(inp, ws.Object);

            return serialiser.Deserialize<List<IResourceHistory>>(output);

        }

        // ReSharper restore InconsistentNaming

    }
}
