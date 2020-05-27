using System;
using System.Linq;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class DsfUnZipComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfUnZip_GetState")]
        public void DsfUnZip_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputPath = @"c:\OldFile.txt";
            var username = "[[username]]";
            var privateKeyFile = "[[KeyFile]]";
            var outputPath = @"c:\OldFile_Zip.txt";
            var destinationUsername = "[[destinationUsername]]";
            var destinationPrivateKeyFile = "[[destinationPrivateKeyFile]]";
            var overwrite = false;
            var result = "[[result]]";

            var zip = new DsfUnZip()
            {
                UniqueID = uniqueId,
                InputPath = inputPath,
                Username = username,
                PrivateKeyFile = privateKeyFile,
                OutputPath = outputPath,
                DestinationUsername = destinationUsername,
                DestinationPrivateKeyFile = destinationPrivateKeyFile,
                Overwrite = overwrite,
                Result = result
            };

            //------------Execute Test---------------------------
            var stateItems = zip.GetState();
            Assert.AreEqual(8, stateItems.Count());

            var expectedResults = new[]
            {
               new StateVariable
                {
                    Name = "InputPath",
                    Value = inputPath,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Username",
                    Value = username,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "PrivateKeyFile",
                    Value = privateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "OutputPath",
                    Value = outputPath,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "DestinationUsername",
                    Value = destinationUsername,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "DestinationPrivateKeyFile",
                    Value = destinationPrivateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Overwrite",
                    Value = overwrite.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = result,
                    Type = StateVariable.StateType.Output
                }
            };

            var iter = zip.GetState().Select(
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