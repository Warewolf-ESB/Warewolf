using System;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxDownloadActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FromPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FromPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FromPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A"};
            var dropBoxDeleteActivity1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource()
            {
                ResourceID = Guid.NewGuid()
            };
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A"};
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource();
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OverwriteFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverwriteFile = true;
            rabbitMqActivity1.OverwriteFile = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OverwriteFile_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverwriteFile = true;
            rabbitMqActivity1.OverwriteFile = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
       
    }
}