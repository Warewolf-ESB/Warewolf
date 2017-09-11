using System;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.BaseConvert
{
    [TestClass]
    public class BaseConvertToEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO();
            var multiAssign1 = new BaseConvertTO();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Values_Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "", "", 1);
            var multiAssign1 = new BaseConvertTO("A", "A", "", "", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFieldNames_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "", "", 1);
            var multiAssign1 = new BaseConvertTO("a", "A", "", "", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFromType_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "", "", 1);
            var multiAssign1 = new BaseConvertTO("A", "v", "", "", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentindexNumber_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "", "", 1);
            var multiAssign1 = new BaseConvertTO("A", "A", "", "", 2);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentExpression_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "", "", 1);
            var multiAssign1 = new BaseConvertTO("A", "A", "", "a", 2);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentToType_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new BaseConvertTO("A", "A", "a", "a", 1);
            var multiAssign1 = new BaseConvertTO("A", "A", "", "a", 2);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
