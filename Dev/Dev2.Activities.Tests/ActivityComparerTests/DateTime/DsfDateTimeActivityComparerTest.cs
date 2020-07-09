using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.DateTime
{
    [TestClass]
    public class DsfDateTimeActivityComparerTest
    {
        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DisplayName_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DisplayName_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Result_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_OutputFormat_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, OutputFormat = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, OutputFormat = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_OutputFormat_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, OutputFormat = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, OutputFormat = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_InputFormat_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_InputFormat_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeModifierAmount_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmount = 1 };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmount = 1 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TimeModifierAmount_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmount = 2 };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmount = 3 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeModifierType_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierType = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierType = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TimeModifierType_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierType = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierType = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DateTime_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, InputFormat = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DateTime_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, DateTime = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, DateTime = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeModifierAmountDisplay_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmountDisplay = "a" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmountDisplay = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TimeModifierAmountDisplay_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmountDisplay = "A" };
            var activity = new DsfDateTimeActivity() { UniqueID = uniqueId, TimeModifierAmountDisplay = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfDateTimeActivity_GetState")]
        public void DsfDateTimeActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var calculateActivity = new DsfDateTimeActivity
            {
                DateTime = "2018/07/20",
                InputFormat = "yyyy/MM/dd",
                OutputFormat = "yyyy/MM/dd",
                TimeModifierType = "days",
                TimeModifierAmountDisplay = "days",
                TimeModifierAmount = 1,
                Result = "TimeChanged"
            };
            //------------Execute Test---------------------------
            var stateItems = calculateActivity.GetState();
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="DateTime",
                    Type = StateVariable.StateType.Input,
                    Value = "2018/07/20"
                },
                new StateVariable
                {
                    Name="InputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="OutputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="TimeModifierType",
                    Type = StateVariable.StateType.Input,
                    Value = "days"
                },
                new StateVariable
                {
                    Name="TimeModifierAmountDisplay",
                    Type = StateVariable.StateType.Input,
                    Value = "days"
                },
                new StateVariable
                {
                    Name="TimeModifierAmount",
                    Type = StateVariable.StateType.Input,
                    Value = "1"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "TimeChanged"
                }
            };

            var iter = calculateActivity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}