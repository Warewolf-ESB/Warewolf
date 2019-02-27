/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxDeleteActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxDeleteActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "aaa" };
            //---------------Assert DsfDropBoxDeleteActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource()
            {
                ResourceID = Guid.NewGuid()
            };
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource();
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            //------------Setup for test--------------------------
            var dropBoxDeleteActivity = new DsfDropBoxDeleteActivity { SelectedSource = selectedSource, DeletePath = "Path", Result = "Deleted" };
            {
                //------------Execute Test---------------------------
                var stateItems = dropBoxDeleteActivity.GetState();
                Assert.AreEqual(3, stateItems.Count());

                var expectedResults = new[]
                {
                new StateVariable
                {
                    Name = "SelectedSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = uniqueId.ToString()
                },
                new StateVariable
                {
                    Name = "DeletePath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Deleted"
                }
            };

                var iter = dropBoxDeleteActivity.GetState().Select(
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
}
