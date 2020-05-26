using System;
using System.Activities;
using System.Linq;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.SelectAndApply
{
    [TestClass]
    public class DsfSelectAndApplyActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySelectAndApply_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySelectAndApply_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity();
            var selectAndApplyActivity = new DsfSelectAndApplyActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        DsfMultiAssignActivity CommonAssign(Guid? uniqueId=null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity() { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ApplyActivityFunc_SameAssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var commonAssign = CommonAssign();
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign
                }
            };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ApplyActivityFunc_Equalsssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(newGuid);
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign
                }
            };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ApplyActivityFunc_DifferentAssigns_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(Guid.NewGuid());
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign
                }
            };
            var selectAndApplyActivity = new DsfSelectAndApplyActivity()
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>()
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfSelectAndApplyActivity_GetState")]
        public void DsfSelectAndApplyActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfSelectAndApplyActivity
            {
                DataSource = "[[DataSource]]",
                Alias = "[[Alias]]"
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(2, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "DataSource",
                    Type = StateVariable.StateType.Input,
                    Value = "[[DataSource]]"
                },
                new StateVariable
                {
                    Name = "Alias",
                    Type = StateVariable.StateType.InputOutput,
                    Value = "[[Alias]]"
                }
            };

            var iter = act.GetState().Select(
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
