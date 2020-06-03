using System;
using System.Linq;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.PathOperations
{
    [TestClass]
    public class DsfZipComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_Emptyzip_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId };
            var DsfZip = new DsfZip() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(DsfZip);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_Emptyzip_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip();
            var zip1 = new DsfZip();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Result = "A", };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(zip.Equals(zip1));
            //---------------Execute Test ----------------------
            zip.IsNotCertVerifiable = true;
            zip1.IsNotCertVerifiable = false;
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Result = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(zip.Equals(zip1));
            //---------------Execute Test ----------------------
            zip.IsNotCertVerifiable = true;
            zip1.IsNotCertVerifiable = true;
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, InputPath = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, InputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, InputPath = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, InputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, InputPath = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, InputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchivePassword = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ArchiveName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, ArchiveName = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchiveName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ArchiveName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, ArchiveName = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchiveName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ArchiveName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, ArchiveName = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, ArchiveName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CompressionRatio_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CompressionRatio_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CompressionRatio_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var zip = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, CompressionRatio = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, DisplayName = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, DisplayName = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, DisplayName = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Username = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Username = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Username = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Username = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Username = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Username = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, PrivateKeyFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Password = "a" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Password = "A" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
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
            var zip = new DsfZip() { UniqueID = uniqueId, Password = "AAA" };
            var zip1 = new DsfZip() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(zip);
            //---------------Execute Test ----------------------
            var @equals = zip.Equals(zip1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        public void DsfZip_GetState()
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
            var archiveName = "[[archiveName]]";
            var compressionRatio = "[[compressionRatio]]";
            var result = "[[result]]";

            var zip = new DsfZip()
            {
                UniqueID = uniqueId,
                InputPath = inputPath,
                Username = username,
                PrivateKeyFile = privateKeyFile,
                OutputPath = outputPath,
                DestinationUsername = destinationUsername,
                DestinationPrivateKeyFile = destinationPrivateKeyFile,
                Overwrite = overwrite,
                ArchiveName = archiveName,
                CompressionRatio = compressionRatio,
                Result = result
            };

            //------------Execute Test---------------------------
            var stateItems = zip.GetState();
            Assert.AreEqual(10, stateItems.Count());

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
                    Name = "ArchiveName",
                    Value = archiveName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "CompressionRatio",
                    Value = compressionRatio,
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