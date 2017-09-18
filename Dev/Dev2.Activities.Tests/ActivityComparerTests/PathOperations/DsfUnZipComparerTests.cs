using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class DsfUnZipComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyFolderRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId };
            var DsfUnZip = new DsfUnZip() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(DsfUnZip);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyFolderRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip();
            var folderRead1 = new DsfUnZip();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNotCertVerifiable_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfUnZip() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfUnZip() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsNotCertVerifiable = true;
            rabbitMqActivity1.IsNotCertVerifiable = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNotCertVerifiable_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfUnZip() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfUnZip() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsNotCertVerifiable = true;
            rabbitMqActivity1.IsNotCertVerifiable = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

      

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, InputPath = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, InputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, InputPath = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, InputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, InputPath = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, InputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArchivePassword_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArchivePassword_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArchivePassword_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, ArchivePassword = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Username = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Username = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Username = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Username = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Username = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Username = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Password = "a" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Password = "A" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfUnZip() { UniqueID = uniqueId, Password = "AAA" };
            var folderRead1 = new DsfUnZip() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}