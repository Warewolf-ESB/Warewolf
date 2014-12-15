
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.TaskScheduler.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class ClientSchedulerFactoryTests
    {
        ClientSchedulerFactory _fact;
        [TestInitialize]
        public void Init()
        {
            _fact = new ClientSchedulerFactory(new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientSchedulerFactory_Trigger")]
        public void AssertTriggerCreation()
        {
            var trig = _fact.CreateTrigger(new DailyTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Daily);
            trig = _fact.CreateTrigger(new BootTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Boot);
            trig = _fact.CreateTrigger(new EventTrigger("a", "b", 122));
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Event);
            trig = _fact.CreateTrigger(new IdleTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Idle);
            trig = _fact.CreateTrigger(new LogonTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Logon);
            trig = _fact.CreateTrigger(new MonthlyTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Monthly);
            trig = _fact.CreateTrigger(new MonthlyDOWTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.MonthlyDOW);
            trig = _fact.CreateTrigger(new RegistrationTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Registration);
            trig = _fact.CreateTrigger(new TimeTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Time);
            trig = _fact.CreateTrigger(new WeeklyTrigger());
            Assert.AreEqual(trig.Trigger.TriggerType, TaskTriggerType.Weekly);
        }



    }
}
