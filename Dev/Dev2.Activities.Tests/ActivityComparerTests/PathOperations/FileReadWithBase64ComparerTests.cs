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
using System.Linq;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class FileReadWithBase64ComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UniqueIDEquals_EmptyFileRead_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId };
            var dsfFileRead = new FileReadWithBase64 { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(dsfFileRead);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UniqueIDDifferent_EmptyFileRead_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64();
            var fileRead1 = new FileReadWithBase64();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_IsNotCertVerifiable_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileReadWithBase64 { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new FileReadWithBase64 { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_IsNotCertVerifiable_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new FileReadWithBase64 { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new FileReadWithBase64 { UniqueID = uniqueId, Result = "A" };
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
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_InputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "a" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_InputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "A" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_InputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "AAA" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, InputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "a" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "A" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "AAA" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Username_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Username = "a" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Username = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Username_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Username = "A" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Username = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Username_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Username = "AAA" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Username = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_PrivateKeyFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_PrivateKeyFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_PrivateKeyFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Password = "a" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Password_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Password = "A" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var fileRead = new FileReadWithBase64 { UniqueID = uniqueId, Password = "AAA" };
            var fileRead1 = new FileReadWithBase64 { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fileRead);
            //---------------Execute Test ----------------------
            var @equals = fileRead.Equals(fileRead1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var inputPath = "/Path";
            var result = "[result]";

            //------------Setup for test--------------------------
            var activity = new FileReadWithBase64 { InputPath = inputPath, Result = result };
            //------------Execute Test---------------------------
            var stateItems = activity.GetState();
            Assert.AreEqual(2, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "InputPath",
                    Type = StateVariable.StateType.Input,
                    Value = inputPath
                },
                new StateVariable
                {
                    Name = "Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };

            var iter = activity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
