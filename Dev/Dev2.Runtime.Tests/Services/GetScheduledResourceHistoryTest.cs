/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
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
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var scheduledResourceHistory = new GetScheduledResourceHistory();

            //------------Execute Test---------------------------
            var resId = scheduledResourceHistory.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var scheduledResourceHistory = new GetScheduledResourceHistory();

            //------------Execute Test---------------------------
            var resId = scheduledResourceHistory.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [TestMethod]
        
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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Inconclusive("Windows Task Scheduler is not available on this platform");
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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Inconclusive("Windows Task Scheduler is not available on this platform");
            var output = RunOutput(false);
            Assert.AreEqual(0, output.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        public void GetScheduledResourceHistory_Execute_NoResource_ReturnsEmptyHistory()
        {
            var esbMethod = new GetScheduledResourceHistory();
            var serializer = new Dev2JsonSerializer();

            var output = esbMethod.Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);

            var result = serializer.Deserialize<List<IResourceHistory>>(output);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("Services_ScheduledResource_GetHistory")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetScheduledResourceHistory_Execute_WhenFactoryThrows_Rethrows()
        {
            var esbMethod = new GetScheduledResourceHistory();
            var serializer = new Dev2JsonSerializer();
            var res = new ScheduledResource("a", SchedulerStatus.Enabled, DateTime.Now, null, "dave", Guid.NewGuid().ToString());
            var factory = new Mock<IServerSchedulerFactory>();
            factory.Setup(a => a.CreateModel(It.IsAny<string>(), It.IsAny<ISecurityWrapper>()))
                   .Throws(new InvalidOperationException("boom"));
            esbMethod.SchedulerFactory = factory.Object;
            esbMethod.SecurityWrapper = new Mock<ISecurityWrapper>().Object;

            esbMethod.Execute(new Dictionary<string, StringBuilder> { { "Resource", serializer.SerializeToBuilder(res) } }, new Mock<IWorkspace>().Object);
        }

        List<IResourceHistory> RunOutput(bool expectCorrectInput)
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
            var res = new ScheduledResource("a", SchedulerStatus.Enabled, DateTime.Now, trigger, "dave", Guid.NewGuid().ToString());
            var inp = new Dictionary<string, StringBuilder>();
            factory.Setup(
                a =>
                a.CreateModel(GlobalConstants.SchedulerFolderId, It.IsAny<ISecurityWrapper>())).Returns(model.Object);
            var serialiser = new Dev2JsonSerializer();
            if (expectCorrectInput)
            {

                model.Setup(a => a.CreateHistory(It.IsAny<ScheduledResource>())).Returns(history).Verifiable();
                inp.Add("Resource", serialiser.SerializeToBuilder(res));
            }

            esbMethod.SchedulerFactory = factory.Object;

            var output = esbMethod.Execute(inp, ws.Object);

            return serialiser.Deserialize<List<IResourceHistory>>(output);

        }



    }
}
