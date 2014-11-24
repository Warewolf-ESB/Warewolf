
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2TaskSettingsTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2RepetitionPattern_PassThrough")]
        public void TaskShedulerWrapper_Dev2TaskSettings_PassThrough()
        {
            var service = new TaskService();
            var task =service.NewTask();
            var settings = new Dev2TaskSettings(task.Settings);
            settings.AllowDemandStart = true;
            settings.DeleteExpiredTaskAfter = new TimeSpan(2);
            settings.AllowHardTerminate = true;
            settings.DisallowStartOnRemoteAppSession = true;
            settings.Enabled = false;
            settings.ExecutionTimeLimit = new TimeSpan(3);
            settings.Hidden = true;
            settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            settings.Priority = ProcessPriorityClass.High;
            settings.RestartCount = 3;
            settings.StartWhenAvailable = false;
            settings.WakeToRun = true;
            var native = task.Settings;
            Assert.AreEqual(settings.AllowDemandStart,native.AllowDemandStart);
            Assert.AreEqual(settings.AllowHardTerminate,native.AllowHardTerminate);
            Assert.AreEqual(settings.DeleteExpiredTaskAfter,native.DeleteExpiredTaskAfter);
            Assert.AreEqual(settings.DisallowStartOnRemoteAppSession,native.DisallowStartIfOnBatteries);
            Assert.AreEqual(settings.Enabled,settings.Enabled);
            Assert.AreEqual(settings.ExecutionTimeLimit,native.ExecutionTimeLimit);
            Assert.AreEqual(settings.ExecutionTimeLimit,native.ExecutionTimeLimit);
            Assert.AreEqual(settings.Hidden,native.Hidden);
            Assert.AreEqual(settings.MultipleInstances,native.MultipleInstances);
            Assert.AreEqual(settings.RestartCount,native.RestartCount);
            Assert.AreEqual(settings.Priority,native.Priority);
            Assert.AreEqual(settings.RestartInterval,native.RestartInterval);
            Assert.AreEqual(settings.StartWhenAvailable,native.StartWhenAvailable);
            Assert.AreEqual(settings.WakeToRun,native.WakeToRun);



        }
    }
}
