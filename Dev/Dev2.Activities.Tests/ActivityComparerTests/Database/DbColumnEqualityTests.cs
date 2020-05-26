using System.Data;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Database
{
    [TestClass]
    public class DbColumnEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn();
            var listTo1 = new DbColumn();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_ColumnName__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn() { ColumnName = "a" };
            var listTo1 = new DbColumn() { ColumnName = "a" }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentColumnName_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "a" };
            var listTo1 = new DbColumn { ColumnName = "Adfdf" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentColumnName_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "a" };
            var listTo1 = new DbColumn { ColumnName = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_SqlDataType__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn() { ColumnName = "a", SqlDataType = SqlDbType.BigInt };
            var listTo1 = new DbColumn() { ColumnName = "a", SqlDataType = SqlDbType.BigInt }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentSqlDataType_Object_Is_Not_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "A", SqlDataType = SqlDbType.BigInt };
            var listTo1 = new DbColumn { ColumnName = "A", SqlDataType = SqlDbType.Binary };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentIsNullable_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "A", IsNullable = true };
            var listTo1 = new DbColumn { ColumnName = "A", IsNullable = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_IsNullable__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn() { ColumnName = "a", IsNullable = false };
            var listTo1 = new DbColumn() { ColumnName = "a", IsNullable = true }; ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentMaxLength_Object_Is_Not_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "A", MaxLength = 1 };
            var listTo1 = new DbColumn { ColumnName = "A", MaxLength = 2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_SameMaxLength_Object_Is_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new DbColumn { ColumnName = "A", MaxLength = 1 };
            var listTo1 = new DbColumn { ColumnName = "A", MaxLength = 1};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(listTo);
            //---------------Execute Test ----------------------
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

    }
}
