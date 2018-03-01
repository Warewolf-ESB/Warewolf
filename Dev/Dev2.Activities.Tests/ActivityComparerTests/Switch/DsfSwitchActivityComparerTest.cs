﻿using Dev2.Activities;
using Dev2.Tests.Activities.ActivityTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Switch
{
    [TestClass]
    public class DsfSwitchActivityComparerTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Empty_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfSwitch();
            var activity1 = new DsfSwitch();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentResult_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Result = "A" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Result = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResult_Different_Casing_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Result = "A" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameSwitch_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Switch = "" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switch = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentSwitch_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Switch = "A" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switch = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameSwitch_Different_Casing_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Switch = "A" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switch = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResult_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfSwitch { UniqueID = uniqId, Result = "" };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameSwitches_DifferentArmCount_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var assign = new DsfMultiAssignActivity();
            var switches = new Dictionary<string, IDev2Activity> { { "Arm", assign }, { "Arm1", assign }, { "Arm2", assign } };
            var switches2 = new Dictionary<string, IDev2Activity> { { "Arm", assign }, { "Arm2", assign } };
            var activity = new DsfSwitch { UniqueID = uniqId, Switches = switches };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switches = switches2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameSwitches_DifferentIndexes_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var assign = new DsfMultiAssignActivity();
            var switches = new Dictionary<string, IDev2Activity> { { "Arm2", assign }, { "Arm", assign } };
            var switches2 = new Dictionary<string, IDev2Activity> { { "Arm", assign }, { "Arm2", assign } };
            var activity = new DsfSwitch { UniqueID = uniqId, Switches = switches };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switches = switches2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentSwitches_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var switches = new Dictionary<string, IDev2Activity>();
            var switches2 = new Dictionary<string, IDev2Activity> { { "", new TestActivity() } };
            var activity = new DsfSwitch { UniqueID = uniqId, Switches = switches };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switches = switches2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameSwitches_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var switches = new Dictionary<string, IDev2Activity>();
            var activity = new DsfSwitch { UniqueID = uniqId, Switches = switches };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Switches = switches };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInner_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var newUniqueId = Guid.NewGuid().ToString();
            var inner = new DsfFlowSwitchActivity
            {
                UniqueID = newUniqueId,
                ExpressionText = "",
                DisplayName = ""
            };

            var activity = new DsfSwitch { UniqueID = uniqId, Inner = inner };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Inner = inner };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentInner_SwitchActivity_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var inner = new DsfFlowSwitchActivity
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "A",
                DisplayName = "A"
            };
            var inner2 = new DsfFlowSwitchActivity
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "B",
                DisplayName = "B"
            };

            var activity = new DsfSwitch { UniqueID = uniqId, Inner = inner };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Inner = inner2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentDefault_SwitchActivity_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var defaults2 = new List<IDev2Activity> { new TestActivity() };
            var activity = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameDefault_DifferentIndexes_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var newUniqueId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity> { new TestActivity { UniqueID = newUniqueId }, new TestActivity() };
            var defaults2 = new List<IDev2Activity> { new TestActivity(), new TestActivity { UniqueID = newUniqueId } };
            var activity = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameDefault_SwitchActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var activity = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void Equals_Given_DifferentErrorVariable_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var activity = new DsfSwitch
            {
                UniqueID = uniqId,
                Default = defaults,
                OnErrorVariable = "[[err]]",
            };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void Equals_Given_DifferentErrorWorkflow_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var activity = new DsfSwitch
            {
                UniqueID = uniqId,
                Default = defaults,
                OnErrorWorkflow = "https://host:4321/err",
            };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void Equals_Given_DifferentIsEndedOnError_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var activity = new DsfSwitch
            {
                UniqueID = uniqId,
                Default = defaults,
                IsEndedOnError = true,
            };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void Equals_Given_DifferentDisplayName_SwitchActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var defaults = new List<IDev2Activity>();
            var activity = new DsfSwitch
            {
                UniqueID = uniqId,
                Default = defaults,
                DisplayName = "The Switch Display Name",
            };
            var activity1 = new DsfSwitch { UniqueID = uniqId, Default = defaults };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}