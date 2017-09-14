using System;
using System.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.ForEach
{
    [TestClass]
    public class DsfForEachActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySelectAndApply_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySelectAndApply_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity();
            var forEach = new DsfForEachActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachType_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InRange };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachType_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void From_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void From_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void From_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void To_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void To_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void To_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CsvIndexes_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CsvIndexes_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        DsfMultiAssignActivity CommonAssign(Guid? uniqueId=null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_SameAssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var commonAssign = CommonAssign();
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_Equalsssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(newGuid);
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_DifferentAssigns_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(Guid.NewGuid());
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


    }
}