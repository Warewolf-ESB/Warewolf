using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Assigns
{
    [TestClass]
    public class ActivityDtoEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new ActivityDTO();
            var multiAssign1 = new ActivityDTO();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_FieldNames_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new ActivityDTO("A","A",1);
            var multiAssign1 = new ActivityDTO("A", "A", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFieldNames_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new ActivityDTO("A","A",1);
            var multiAssign1 = new ActivityDTO("a", "A", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentFielValue_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new ActivityDTO("A","A",1);
            var multiAssign1 = new ActivityDTO("A", "a", 1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_DiffentindexNumber_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new ActivityDTO("A","A",1);
            var multiAssign1 = new ActivityDTO("A", "A", 2);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
