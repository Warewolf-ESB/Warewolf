using System;
using System.Collections;
using System.Collections.Generic;
using Dev2.Scheduler.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Scheduler;
using Moq;
namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class ScheduledResourceTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResource_Properties")]
        public void TaskSheduler_ScheduledResource_Constructor()
        {
            IList< IResourceHistory> hist = new List<IResourceHistory>();
            IScheduleTrigger trigger = new Mock<IScheduleTrigger>().Object;
            ScheduledResource res = new ScheduledResource("bob",SchedulerStatus.Enabled, DateTime.MaxValue,trigger,"rory");
            Assert.AreEqual("bob",res.Name);
            Assert.AreEqual(SchedulerStatus.Enabled,res.Status);
            Assert.AreEqual(DateTime.MaxValue,res.NextRunDate);
            Assert.AreEqual(trigger,res.Trigger);
         
            Assert.AreEqual("rory",res.WorkflowName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResource_Properties")]
        public void TaskSheduler_Properties()
        {
            IList<IResourceHistory> hist = new List<IResourceHistory>();
            IScheduleTrigger trigger = new Mock<IScheduleTrigger>().Object;
            var res = new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, trigger, "rory");
            res.Password = "PWD";
            Assert.AreEqual( "PWD",res.Password);
            res.UserName = "User";
            Assert.AreEqual("User",res.UserName);
            res.RunAsapIfScheduleMissed = false;
            Assert.IsFalse(res.RunAsapIfScheduleMissed);
            res.NumberOfHistoryToKeep = 25;
            Assert.AreEqual(25,res.NumberOfHistoryToKeep);
            res.AllowMultipleIstances = true;
            Assert.AreEqual(true,res.AllowMultipleIstances);
            res.OldName = "bob";
            Assert.AreEqual("bob", res.OldName);

        }
    }
}
