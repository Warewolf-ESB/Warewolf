/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetFilesTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void HandlesType_Returns_GetFiles()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("GetFiles", getFiles.HandlesType());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_Returns_EmptyGuid()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var resourceID = getFiles.GetResourceID(new Dictionary<string, StringBuilder>());
            Assert.AreEqual(Guid.Empty, resourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_Returns_Any()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var authorizationContextForService = getFiles.GetAuthorizationContextForService();
            Assert.AreEqual(AuthorizationContext.Any, authorizationContextForService);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildFileListing_Given_FileInfo_Returns_NewFileListing()
        {
            //------------Setup for test-------------------------
            FileSystemInfo fileSystemInfo = new FileSytemInfoMock();
            var files = new GetFiles();
            //------------Execute Test---------------------------
            var results = files.BuildFileListing(fileSystemInfo);
            //------------Assert Results-------------------------
            Assert.IsNotNull(results);
            Assert.IsNotNull(results.GetType() == typeof(FileListing));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Name, fileSystemInfo.Name);
            Assert.AreEqual(results.FullName, fileSystemInfo.FullName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void Execute_GivenNoArguments_ReturnsDrives()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            var serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder>();
            //------------Execute Test---------------------------
            var result = getFiles.Execute(values, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var executeMessage = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsFalse(executeMessage.HasError);
            Assert.IsNotNull(executeMessage.Message);
            var drives = serializer.Deserialize<List<IFileListing>>(executeMessage.Message.ToString());
            Assert.IsNotNull(drives);
            Assert.IsTrue(drives.Count > 0, "Expected at least one drive to be returned");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void Execute_GivenFileListingArgument_DeserializesAndReturnsDirectoryContents()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            var serializer = new Dev2JsonSerializer();

            // Create a temp directory with a test file to ensure we have content
            var tempDir = Path.Combine(Path.GetTempPath(), "GetFilesTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var testFilePath = Path.Combine(tempDir, "testfile.txt");
            File.WriteAllText(testFilePath, "test content");

            try
            {
                var fileListing = new FileListing
                {
                    Name = Path.GetFileName(tempDir),
                    FullName = tempDir,
                    IsDirectory = true
                };

                var values = new Dictionary<string, StringBuilder>
                {
                    { "fileListing", new StringBuilder(serializer.Serialize(fileListing)) }
                };
                //------------Execute Test---------------------------
                var result = getFiles.Execute(values, null);
                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                var executeMessage = serializer.Deserialize<ExecuteMessage>(result);
                Assert.IsFalse(executeMessage.HasError, "Execute should not have error");
                Assert.IsNotNull(executeMessage.Message, "Message should not be null");
                Assert.IsTrue(executeMessage.Message.Length > 0, "Message should not be empty - this was the bug where fileListing deserialization failed");

                var files = serializer.Deserialize<List<IFileListing>>(executeMessage.Message.ToString());
                Assert.IsNotNull(files, "Files list should deserialize correctly");
                Assert.IsTrue(files.Count > 0, "Should return at least one file");
                Assert.IsTrue(files.Any(f => f.Name == "testfile.txt"), "Should contain the test file we created");
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir);
                }
            }
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void Execute_GivenFileListingArgument_WithSubdirectories_ReturnsFilesAndFolders()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            var serializer = new Dev2JsonSerializer();

            // Create a temp directory with subdirectory and file
            var tempDir = Path.Combine(Path.GetTempPath(), "GetFilesTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var subDir = Path.Combine(tempDir, "SubFolder");
            Directory.CreateDirectory(subDir);
            var testFilePath = Path.Combine(tempDir, "document.txt");
            File.WriteAllText(testFilePath, "test content");

            try
            {
                var fileListing = new FileListing
                {
                    Name = Path.GetFileName(tempDir),
                    FullName = tempDir,
                    IsDirectory = true
                };

                var values = new Dictionary<string, StringBuilder>
                {
                    { "fileListing", new StringBuilder(serializer.Serialize(fileListing)) }
                };
                //------------Execute Test---------------------------
                var result = getFiles.Execute(values, null);
                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                var executeMessage = serializer.Deserialize<ExecuteMessage>(result);
                Assert.IsFalse(executeMessage.HasError);

                var files = serializer.Deserialize<List<IFileListing>>(executeMessage.Message.ToString());
                Assert.IsNotNull(files);
                Assert.AreEqual(2, files.Count, "Should return both the subdirectory and the file");

                var folder = files.FirstOrDefault(f => f.Name == "SubFolder");
                Assert.IsNotNull(folder, "Should contain the subdirectory");
                Assert.IsTrue(folder.IsDirectory, "SubFolder should be marked as directory");

                var file = files.FirstOrDefault(f => f.Name == "document.txt");
                Assert.IsNotNull(file, "Should contain the document file");
                Assert.IsFalse(file.IsDirectory, "document.txt should not be marked as directory");
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
                if (Directory.Exists(subDir))
                {
                    Directory.Delete(subDir);
                }
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir);
                }
            }
        }
    }
    public class FileSytemInfoMock : FileSystemInfo
    {
        public override void Delete()
        {
        }

        public override string Name => "DummyFileName.extension";
        public override bool Exists => true;

        public override string FullName => @"SomeDirectory\DummyFileName.extension";
    }
}


