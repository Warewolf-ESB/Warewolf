using Dev2.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    // This is for testing against the actual fileSystem which may vary
    public class FileSystemQueryTest
    {
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
        public void QueryListWhereNetworkPathHasFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Installations");
            //------------Assert Results-------------------------
            var count = fileSystemQuery.QueryCollection.Count;

            Assert.IsTrue(count > 0, "No items listed");
        }
    }
}