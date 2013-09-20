using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    // This is for testing against the actual fileSystem which may vary
    public class FileSystemQueryTest
    {

        [TestMethod]
       
        public void QueryListWhereNothingPassedExpectListOfDrives()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList("");
            //------------Assert Results-------------------------
            Assert.AreEqual(8, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereDrivePassedExpectFoldersAndFilesOnDrive()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList("C:");
            //------------Assert Results-------------------------
            Assert.AreEqual(31, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereDriveAndFolderPassedNoSlashExpectFolder()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users");
            //------------Assert Results-------------------------
            Assert.AreEqual(9, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereDriveAndFolderWithStartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users\des");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereDriveAndFolderWithPartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users\skt");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereNoNetworkExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\");
            //------------Assert Results-------------------------
            Assert.AreEqual(40, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereNetworkPathExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\");
            //------------Assert Results-------------------------
            Assert.AreEqual(8, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWherePartialNetworkPathExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\TFS");
            //------------Assert Results-------------------------
            Assert.AreEqual(3, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereNetworkPathHasFilesExpectFolderWithFilesNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff");
            //------------Assert Results-------------------------
            Assert.AreEqual(16, fileSystemQuery.QueryCollection.Count);
        }


        [TestMethod]
       
        public void QueryListWhereNetworkPathHasFolderExpectFolderInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\_Arch");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereNetworkPathHasFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\LoadTest");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
       
        public void QueryListWhereNetworkPathHasMiddleOfFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Runt");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

    }

}