using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class DsfFolderReadComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyFolderRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId };
            var dsfFolderRead = new DsfFolderRead() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(dsfFolderRead);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyFolderRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfFolderRead();
            var folderRead1 = new DsfFolderRead();
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
            var rabbitMqActivity = new DsfFolderRead() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
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
            var rabbitMqActivity = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
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
        public void IsFilesAndFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFolderRead() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesAndFoldersSelected = true;
            rabbitMqActivity1.IsFilesAndFoldersSelected = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesAndFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFolderRead() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesAndFoldersSelected = true;
            rabbitMqActivity1.IsFilesAndFoldersSelected = true;
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "a" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "a" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "A" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "ass" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "AAA" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, InputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "a" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "A" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "AAA" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, DisplayName = "aaa" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Username = "a" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Username = "a" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Username = "A" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Username = "ass" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Username = "AAA" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Username = "aaa" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "a" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Password = "a" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Password = "a" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Password = "A" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Password = "ass" };
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
            var folderRead = new DsfFolderRead() { UniqueID = uniqueId, Password = "AAA" };
            var folderRead1 = new DsfFolderRead() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(folderRead);
            //---------------Execute Test ----------------------
            var @equals = folderRead.Equals(folderRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}