using System;
using System.Collections.Generic;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.ObjectAssign

{
    [TestClass]
    public class DsfMultiAssignObjectActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity();
            var multiAssign1 = new DsfMultiAssignObjectActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "a" };
            var multiAssign1 = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "A" };
            var multiAssign1 = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "AAA" };
            var multiAssign1 = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId,  DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FieldsCollectionSame_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId ,FieldsCollection = new List<AssignObjectDTO>() };
            var multiAssign1 = new DsfMultiAssignObjectActivity() { UniqueID = uniqueId ,FieldsCollection = new List<AssignObjectDTO>() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FieldsCollectionSame_EmptyAssigns_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
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
        public void FieldsCollectionSame_EmptyAssigns_IsNotEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("A","a",1)
                }
            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateAllOccurrences_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
              
            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.UpdateAllOccurrences = true;
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateAllOccurrences_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
              
            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.UpdateAllOccurrences = true;
            multiAssign1.UpdateAllOccurrences = true;
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateBookmark_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false

            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.UpdateAllOccurrences = true;
            multiAssign1.UpdateAllOccurrences = true;
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceHost_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false

            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.ServiceHost = "host1";
            multiAssign1.ServiceHost = "host1";
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceHost_Same_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false

            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.ServiceHost = "host1";
            multiAssign1.ServiceHost = "host";
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateBookmark_Same_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false

            };
            var multiAssign1 = new DsfMultiAssignObjectActivity()
            {
                UniqueID = uniqueId,
                FieldsCollection = new List<AssignObjectDTO>()
                {
                    new AssignObjectDTO("a","a",1)
                },
                CreateBookmark = false
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(multiAssign.Equals(multiAssign1));
            //---------------Execute Test ----------------------
            multiAssign.CreateBookmark = true;
            var equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
