using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Servers;

namespace Tu.Server.Tests.Servers
{
    [TestClass]
    public class FileServerTests
    {
        static string _testDir;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _testDir = context.TestDeploymentDir;
        }

        [TestMethod]
        [TestCategory("FileServer_Constructor")]
        [Description("FileServer Constructor with null path throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileServer_UnitTest_ConstructorWithNullPath_ThrowsArgumentNullException()
        {
            var server = new FileServer(null);
        }

        [TestMethod]
        [TestCategory("FileServer_Constructor")]
        [Description("FileServer Constructor with no parameters initializes path to current assembly path.")]
        [Owner("Trevor Williams-Ros")]
        public void FileServer_UnitTest_ConstructorWithNoParameters_PathIsCurrentAssemblyPath()
        {
            var server = new FileServer();

            Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, server.Path);
        }

        [TestMethod]
        [TestCategory("FileServer_WriteRead")]
        [Description("FileServer Write/ReadAllText writes/reads contents to/from file.")]
        [Owner("Trevor Williams-Ros")]
        public void FileServer_UnitTest_WriteRead_Done()
        {
            const string Contents = "Test abcd";
            var fileName = string.Format("{0}.txt", Guid.NewGuid());
            var path = Path.Combine(_testDir, fileName);

            Assert.IsFalse(File.Exists(path));

            var server = new FileServer(_testDir);
            server.WriteAllText(fileName, Contents);

            Assert.IsTrue(File.Exists(path));

            var actualContents = server.ReadAllText(fileName);
            Assert.IsNotNull(actualContents);
            Assert.AreEqual(Contents, actualContents);
        }

        [TestMethod]
        [TestCategory("FileServer_Read")]
        [Description("FileServer ReadAllText with non existent file returns empty string.")]
        [Owner("Trevor Williams-Ros")]
        public void FileServer_UnitTest_ReadNonExistentFile_EmptyString()
        {
            var fileName = string.Format("{0}.txt", Guid.NewGuid());
            var path = Path.Combine(_testDir, fileName);

            Assert.IsFalse(File.Exists(path));

            var server = new FileServer(_testDir);
            var actualContents = server.ReadAllText(fileName);

            Assert.IsFalse(File.Exists(path));
            Assert.IsNotNull(actualContents);
            Assert.AreEqual(string.Empty, actualContents);
        }

        [TestMethod]
        [TestCategory("FileServer_Delete")]
        [Description("FileServer Delete deletes file.")]
        [Owner("Trevor Williams-Ros")]
        public void FileServer_UnitTest_Delete_Done()
        {
            const string Contents = "Test abcd";
            var fileName = string.Format("{0}.txt", Guid.NewGuid());
            var path = Path.Combine(_testDir, fileName);

            Assert.IsFalse(File.Exists(path));

            var server = new FileServer(_testDir);
            server.WriteAllText(fileName, Contents);

            Assert.IsTrue(File.Exists(path));

            server.Delete(fileName);

            Assert.IsFalse(File.Exists(path));
        }
    }
}
