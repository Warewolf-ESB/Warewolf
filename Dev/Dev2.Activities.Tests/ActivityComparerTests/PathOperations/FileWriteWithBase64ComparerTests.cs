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
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations.WithBase64;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class FileWriteWithBase64ComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_UniqueIDEquals_EmptyFileRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId };
            var DsfFileWrite = new FileWriteWithBase64() { UniqueID = uniqueId };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_UniqueIDDifferent_EmptyFileRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var fileRead = new FileWriteWithBase64();
            var fileRead1 = new FileWriteWithBase64();
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_FileContents_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_FileContents_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_FileContents_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, FileContents = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, DisplayName = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_OutputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_OutputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_OutputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, OutputPath = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Username_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Username_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Username_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Username = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_PrivateKeyFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_PrivateKeyFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_PrivateKeyFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "a" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "a" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Password_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "A" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "ass" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "AAA" };
            var fileRead1 = new FileWriteWithBase64() { UniqueID = uniqueId, Password = "aaa" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Append_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Append_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Overwrite_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_Overwrite_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_AppendTop_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_AppendTop_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_AppendBottom_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_IsContentBase64_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sut_one = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A", };
            var sut_two = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_AppendBottom_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileWriteWithBase64))]
        public void FileWriteWithBase64_IsContentBase64_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sut_one = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
            var sut_two = new FileWriteWithBase64() { UniqueID = uniqueId, Result = "A" };
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