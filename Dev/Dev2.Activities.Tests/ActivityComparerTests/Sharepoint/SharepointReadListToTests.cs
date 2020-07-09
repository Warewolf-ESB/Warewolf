using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointReadListToEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo();
            var listTo1 = new SharepointReadListTo();
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
        public void Equals_FieldNames__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo(){FieldName = "a"};
            var listTo1 = new SharepointReadListTo() { FieldName = "a" }; ;
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
        public void Equals_DiffentFieldNames_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "Adfdf" };
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
        public void Equals_DiffentFieldNames_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "A" };
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
        public void Equals_VariableName__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo() { FieldName = "a", VariableName="A" };
            var listTo1 = new SharepointReadListTo() { FieldName = "a", VariableName="A" }; ;
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
        public void Equals_DiffentVariableName_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", VariableName="sss" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", VariableName="dfdf" };
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
        public void Equals_DiffentVariableName_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", VariableName = "a"};
            var listTo1 = new SharepointReadListTo { FieldName = "A" , VariableName ="mkmk"};
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
        public void Equals_InternalName__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo() { FieldName = "a", InternalName = "A" };
            var listTo1 = new SharepointReadListTo() { FieldName = "a", InternalName = "A" }; ;
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
        public void Equals_DiffentInternalName_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", InternalName = "sss" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", InternalName = "dfdf" };
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
        public void Equals_DiffentInternalName_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", InternalName = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", InternalName = "mkmk" };
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
        public void Equals_Type__Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo() { FieldName = "a", Type = "A" };
            var listTo1 = new SharepointReadListTo() { FieldName = "a", Type = "A" }; ;
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
        public void Equals_DiffentType_Object_Is_Not_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "sss" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "dfdf" };
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
        public void Equals_DiffentType_Object_Is_Not_IsEqua()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "mkmk" };
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
        public void IsRequired_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "A" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsRequired = true;
            listTo1.IsRequired = false;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IsRequired_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "a" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.IsRequired = true;
            listTo1.IsRequired = true;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Inserted_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "A" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.Inserted = true;
            listTo1.Inserted = false;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Inserted_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var listTo = new SharepointReadListTo { FieldName = "A", Type = "a" };
            var listTo1 = new SharepointReadListTo { FieldName = "A", Type = "a" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(listTo.Equals(listTo1));
            //---------------Execute Test ----------------------
            listTo.Inserted = true;
            listTo1.Inserted = true;
            var @equals = listTo.Equals(listTo1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
    }
}