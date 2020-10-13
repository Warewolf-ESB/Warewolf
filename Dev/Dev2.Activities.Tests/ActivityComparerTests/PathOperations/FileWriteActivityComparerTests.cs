/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class FileWriteActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_UniqueIDEquals_EmptyFileRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId };
            var DsfFileWrite = new FileWriteActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(DsfFileWrite);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_UniqueIDDifferent_EmptyFileRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var fileRead = new FileWriteActivity();
            var fileRead1 = new FileWriteActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_FileContents_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_FileContents_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_FileContents_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, FileContents = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_OutputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_OutputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_OutputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, OutputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Username_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Username = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Username = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Username_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Username = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Username = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Username_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Username = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Username = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_PrivateKeyFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_PrivateKeyFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_PrivateKeyFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Password = "a" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Password_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Password = "A" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteActivity() { UniqueID = uniqueId, Password = "AAA" };
            var fileRead1 = new FileWriteActivity() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Append_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Append_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Overwrite_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_Overwrite_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_AppendTop_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_AppendTop_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_AppendBottom_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_IsContentBase64_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sut_one = new FileWriteActivity() { UniqueID = uniqueId, Result = "A", };
            var sut_two = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sut_one.Equals(sut_two));
            //---------------Execute Test ----------------------
            sut_one.FileContentsAsBase64 = true;
            sut_two.FileContentsAsBase64 = false;
            var @equals = sut_one.Equals(sut_two);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_AppendBottom_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AppendBottom = true;
            rabbitMqActivity1.AppendBottom = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteActivity))]
        public void FileWriteActivity_IsContentBase64_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sut_one = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            var sut_two = new FileWriteActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sut_one.Equals(sut_two));
            //---------------Execute Test ----------------------
            sut_one.FileContentsAsBase64 = true;
            sut_two.FileContentsAsBase64 = true;
            var @equals = sut_one.Equals(sut_two);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}