using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.DeleteRecord
{
    [TestClass]
    public class DsfDateTimeDifferenceActivityComparerTest
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
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId };
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
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, DisplayName = "ass" };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Result = "a" };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Result = "ass" };
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
        public void Equals_Given_Same_Input1_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input1 = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input1 = "a" };
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
        public void Equals_Given_Different_Input1_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input1 = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input1 = "ass" };
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
        public void Equals_Given_Same_Input2_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input2 = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input2 = "a" };
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
        public void Equals_Given_Different_Input2_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input2 = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, Input2 = "ass" };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, InputFormat = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, InputFormat = "a" };
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
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, InputFormat = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, InputFormat = "ass" };
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
        public void Equals_Given_Same_OutputType_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, OutputType = "a" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, OutputType = "a" };
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
        public void Equals_Given_Different_OutputType_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, OutputType = "A" };
            var activity = new DsfDateTimeDifferenceActivity() { UniqueID = uniqueId, OutputType = "ass" };
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
        [TestCategory("DsfDateTimeDifferenceActivity_GetState")]
        public void DsfDateTimeDifferenceActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var dateTimeDifferenceActivity = new DsfDateTimeDifferenceActivity
            {
                Input1 = "date1",
                Input2 = "date2",
                InputFormat = "yyyy/MM/dd",
                OutputType = "Years",
                Result = "DateChanged"
            };
            //------------Execute Test---------------------------
            var stateItems = dateTimeDifferenceActivity.GetState();
            Assert.AreEqual(5, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Input1",
                    Type = StateVariable.StateType.Input,
                    Value = "date1"
                },
                new StateVariable
                {
                    Name = "Input2",
                    Type = StateVariable.StateType.Input,
                    Value = "date2"
                },
                new StateVariable
                {
                    Name="InputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="OutputType",
                    Type = StateVariable.StateType.Input,
                    Value = "Years"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "DateChanged"
                }
            };

            var iter = dateTimeDifferenceActivity.GetState().Select(
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