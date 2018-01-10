using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Activities;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Database
{
    [TestClass]
    public class DsfSqlBulkInsertActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepoint_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId };
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSqlBulkInsertActivity);
            //---------------Execute Test ----------------------
            var equals = dsfSqlBulkInsertActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepoint_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlBulkInsertActivity();
            var andApplyActivity = new DsfSqlBulkInsertActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(andApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputMappingsSame_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, InputMappings = new List<DataColumnMapping>() };
            var multiAssign1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, InputMappings = new List<DataColumnMapping>() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputMappingsSame_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping(){ InputColumn = "a"}
                }
            };
            var multiAssign1 = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping(){ InputColumn = "a"}
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputMappings_DbColsSame_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping()
                    {
                        InputColumn = "a",
                        OutputColumn = new DbColumn()
                        {
                            ColumnName = "a",
                            DataType = typeof(string),
                            MaxLength = 1,
                            IsNullable = false,
                            SqlDataType = SqlDbType.BigInt,
                            IsAutoIncrement = false
                        }
                    }
                }
            };
            var multiAssign1 = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping()
                    {
                        InputColumn = "a",
                        OutputColumn = new DbColumn()
                        {
                            ColumnName = "a",
                            DataType = typeof(string),
                            MaxLength = 1,
                            IsNullable = false,
                            SqlDataType = SqlDbType.BigInt,
                            IsAutoIncrement = false
                        }
                    }
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputMappings_DbColsSame_DifferentSqlDataType_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var bulkInsertActivity = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping()
                    {
                        InputColumn = "a",
                        OutputColumn = new DbColumn()
                        {
                            ColumnName = "a",
                            DataType = typeof(string),
                            MaxLength = 1,
                            IsNullable = false,
                            SqlDataType = SqlDbType.BigInt,
                            IsAutoIncrement = false
                        }
                    }
                }
            };
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping()
                    {
                        InputColumn = "b",
                        OutputColumn = new DbColumn()
                        {
                            ColumnName = "a",
                            DataType = typeof(string),
                            MaxLength = 1,
                            IsNullable = false,
                            SqlDataType = SqlDbType.BigInt,
                            IsAutoIncrement = false
                        }
                    }
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(bulkInsertActivity);
            //---------------Execute Test ----------------------
            var @equals = bulkInsertActivity.Equals(dsfSqlBulkInsertActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputMappingsSameDifferentIndexNumbers_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var bulkInsertActivity = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping(){ InputColumn = "a", IndexNumber = 1},
                    new DataColumnMapping(){ InputColumn = "b", IndexNumber = 2},
                }
            };
            var sqlBulkInsertActivity = new DsfSqlBulkInsertActivity()
            {
                UniqueID = uniqueId,
                InputMappings = new List<DataColumnMapping>()
                {
                    new DataColumnMapping(){InputColumn = "b", IndexNumber = 1},
                    new DataColumnMapping(){InputColumn = "a", IndexNumber = 2}
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(bulkInsertActivity);
            //---------------Execute Test ----------------------
            var @equals = bulkInsertActivity.Equals(sqlBulkInsertActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BatchSize_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "a" };
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BatchSize_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "A" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BatchSize_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "AAA" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, BatchSize = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Timeout_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "a" };
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Timeout_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "A" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Timeout_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "AAA" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Timeout = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TableName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "a" };
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TableName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "A" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TableName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "AAA" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, TableName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "A" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "AAA" };
            var activity1 = new DsfSqlBulkInsertActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CheckConstraints_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, CheckConstraints = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, CheckConstraints = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.CheckConstraints = true;
            activity1.CheckConstraints = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CheckConstraints_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, CheckConstraints = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, CheckConstraints = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.CheckConstraints = true;
            sharepoint.CheckConstraints = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FireTriggers_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, FireTriggers = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, FireTriggers = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.FireTriggers = true;
            activity1.FireTriggers = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FireTriggers_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, FireTriggers = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, FireTriggers = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.FireTriggers = true;
            sharepoint.FireTriggers = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UseInternalTransaction_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, UseInternalTransaction = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, UseInternalTransaction = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.UseInternalTransaction = true;
            activity1.UseInternalTransaction = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UseInternalTransaction_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, UseInternalTransaction = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, UseInternalTransaction = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.UseInternalTransaction = true;
            sharepoint.UseInternalTransaction = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void KeepIdentity_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepIdentity = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepIdentity = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.KeepIdentity = true;
            activity1.KeepIdentity = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void KeepIdentity_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepIdentity = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepIdentity = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.KeepIdentity = true;
            sharepoint.KeepIdentity = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void KeepTableLock_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepTableLock = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepTableLock = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.KeepTableLock = true;
            activity1.KeepTableLock = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void KeepTableLock_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepTableLock = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, KeepTableLock = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.KeepTableLock = true;
            sharepoint.KeepTableLock = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IgnoreBlankRows_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, IgnoreBlankRows = true };
            var activity1 = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, IgnoreBlankRows = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.IgnoreBlankRows = true;
            activity1.IgnoreBlankRows = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IgnoreBlankRows_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, IgnoreBlankRows = true };
            var sharepoint = new DsfSqlBulkInsertActivity { UniqueID = uniqueId, IgnoreBlankRows = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.IgnoreBlankRows = true;
            sharepoint.IgnoreBlankRows = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}
