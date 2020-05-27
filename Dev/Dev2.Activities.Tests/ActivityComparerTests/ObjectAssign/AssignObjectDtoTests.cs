using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.ObjectAssign
{
    [TestClass]
    public class AssignObjectDtoTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_EmptyTos_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new AssignObjectDTO();
            var multiAssign1 = new AssignObjectDTO();
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
        public void Equals_FieldNames_Object_Is_IsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new AssignObjectDTO("A","A",1);
            var multiAssign1 = new AssignObjectDTO("A", "A", 1);
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
        public void Equals_DiffentFieldNames_Object_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new AssignObjectDTO("A","A",1);
            var multiAssign1 = new AssignObjectDTO("a", "A", 1);
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
        public void Equals_DiffentFielValue_Object_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new AssignObjectDTO("A","A",1);
            var multiAssign1 = new AssignObjectDTO("A", "a", 1);
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
        public void Equals_DiffentindexNumber_Object_Is_NotIsEqual()
        {
            //---------------Set up test pack-------------------
            var multiAssign = new AssignObjectDTO("A","A",1);
            var multiAssign1 = new AssignObjectDTO("A", "A", 2);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
