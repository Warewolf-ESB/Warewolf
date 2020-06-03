using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class DsfFileWriteComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyFileRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId };
            var DsfFileWrite = new DsfFileWrite() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(DsfFileWrite);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyFileRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite();
            var fileRead1 = new DsfFileWrite();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void FileContents_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void FileContents_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void FileContents_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, FileContents = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void OutputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, OutputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Username = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Username = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Username = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Username = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Username_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Username = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Username = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void PrivateKeyFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Password = "a" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Password = "A" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new DsfFileWrite() { UniqueID = uniqueId, Password = "AAA" };
            var fileRead1 = new DsfFileWrite() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Append_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Append = true;
            rabbitMqActivity1.Append = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Append_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Append = true;
            rabbitMqActivity1.Append = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Overwrite = true;
            rabbitMqActivity1.Overwrite = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Overwrite = true;
            rabbitMqActivity1.Overwrite = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void AppendTop_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AppendTop = true;
            rabbitMqActivity1.AppendTop = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void AppendTop_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AppendTop = true;
            rabbitMqActivity1.AppendTop = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void AppendBottom_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AppendBottom = true;
            rabbitMqActivity1.AppendBottom = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void AppendBottom_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfFileWrite() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AppendBottom = true;
            rabbitMqActivity1.AppendBottom = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}