using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Servers;

namespace Tu.Server.Tests.Servers
{
    [TestClass]
    public class FtpServerTests
    {
        public const string DemoServerUri = "ftp://rsaklfsvrsbspdc:1001";
        public const string DemoRelativeUri = "TestData.txt";
        public readonly static string DemoData = "Test Data";

        [TestMethod]
        [TestCategory("FtpServer_Constructor")]
        [Description("FtpServer Constructor with null server uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_ConstructorWithNullServerUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Constructor")]
        [Description("FtpServer Constructor with invalid server uri throws UriFormatException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(UriFormatException))]
        public void FtpServer_UnitTest_ConstructorWithInvalidServerUri_ThrowsUriFormatException()
        {
            var server = new FtpServer("xxxxxx");
        }

        [TestMethod]
        [TestCategory("FtpServer_Upload")]
        [Description("FtpServer Upload with null relative uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_UploadWithNullRelativeUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Upload(null, null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Upload")]
        [Description("FtpServer Upload with null data throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_UploadWithNullData_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Upload("xxxx", null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Upload")]
        [Description("FtpServer Upload with invalid relative uri throws UriFormatException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(UriFormatException))]
        public void FtpServer_UnitTest_UploadWithInvalidRelativeUri_ThrowsUriFormatException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Upload("http://www.google.com", DemoData);
        }

        [TestMethod]
        [TestCategory("FtpServer_Upload")]
        [Description("FtpServer Upload with valid relative uri uploads file.")]
        [Owner("Trevor Williams-Ros")]
        public void FtpServer_UnitTest_UploadWithValidParams_UploadsFile()
        {
            var fileName = Guid.NewGuid() + "_" + DemoRelativeUri;
            var server = new FtpServer(DemoServerUri);

            var status = server.Upload(fileName, DemoData);
            Assert.IsTrue(status, "Upload did not upload file.");

            server.Delete(fileName);
        }

        [TestMethod]
        [TestCategory("FtpServer_Download")]
        [Description("FtpServer Download with null relative uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_DownloadWithNullRelativeUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Download(null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Download")]
        [Description("FtpServer Download with invalid relative uri throws UriFormatException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(UriFormatException))]
        public void FtpServer_UnitTest_DownloadWithInvalidRelativeUri_ThrowsUriFormatException()
        {
            var server = new FtpServer(DemoServerUri);
            var data = server.Download("http://www.google.com");
        }

        [TestMethod]
        [TestCategory("FtpServer_Download")]
        [Description("FtpServer Download with valid relative uri downloads file.")]
        [Owner("Trevor Williams-Ros")]
        public void FtpServer_UnitTest_DownloadWithValidParams_DownloadsFile()
        {
            var fileName = Guid.NewGuid() + "_" + DemoRelativeUri;

            var server = new FtpServer(DemoServerUri);
            server.Upload(fileName, DemoData);

            var actualData = server.Download(fileName);

            Assert.IsNotNull(actualData, "Download did not download file.");

            var expectedData = DemoData;
            Assert.AreEqual(expectedData, actualData, "Download did not download the contents correctly.");

            server.Delete(fileName);
        }


        [TestMethod]
        [TestCategory("FtpServer_Delete")]
        [Description("FtpServer Delete with null relative uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_DeleteWithNullRelativeUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Delete(null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Delete")]
        [Description("FtpServer Delete with invalid relative uri throws UriFormatException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(UriFormatException))]
        public void FtpServer_UnitTest_DeleteWithInvalidRelativeUri_ThrowsUriFormatException()
        {
            var server = new FtpServer(DemoServerUri);
            var data = server.Delete("http://www.google.com");
        }

        [TestMethod]
        [TestCategory("FtpServer_Delete")]
        [Description("FtpServer Delete with valid relative uri deletes file.")]
        [Owner("Trevor Williams-Ros")]
        public void FtpServer_UnitTest_DeleteWithValidParams_DeletesFile()
        {
            var fileName = Guid.NewGuid() + "_" + DemoRelativeUri;

            var server = new FtpServer(DemoServerUri);
            server.Upload(fileName, DemoData);

            var status = server.Delete(fileName);

            Assert.IsTrue(status, "Delete did not delete file.");
        }


        [TestMethod]
        [TestCategory("FtpServer_Rename")]
        [Description("FtpServer Rename with null to relative uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_RenameWithNullFromRelativeUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Rename(null, null);
        }


        [TestMethod]
        [TestCategory("FtpServer_Rename")]
        [Description("FtpServer Rename with null from relative uri throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FtpServer_UnitTest_RenameWithNullToRelativeUri_ThrowsArgumentNullException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Rename(DemoRelativeUri, null);
        }

        [TestMethod]
        [TestCategory("FtpServer_Rename")]
        [Description("FtpServer Rename with invalid from relative uri throws UriFormatException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(UriFormatException))]
        public void FtpServer_UnitTest_RenameWithInvalidFromRelativeUri_ThrowsUriFormatException()
        {
            var server = new FtpServer(DemoServerUri);
            server.Rename("http://www.google.com", "http://www.google.com");
        }

        [TestMethod]
        [TestCategory("FtpServer_Rename")]
        [Description("FtpServer Rename with valid relative uri deletes file.")]
        [Owner("Trevor Williams-Ros")]
        public void FtpServer_UnitTest_RenameWithValidParams_RenamesFile()
        {
            var fromFileName = "Transunion/Auto Output/" + Guid.NewGuid() + "_" + DemoRelativeUri;
            var toFileName = "Transunion/Auto Output/Complete/" + Guid.NewGuid() + "_" + DemoRelativeUri;

            var server = new FtpServer(DemoServerUri);
            server.Upload(fromFileName, DemoData);

            var status = server.Rename(fromFileName, toFileName);

            Assert.IsTrue(status, "Rename did not rename file.");

            status = server.Delete(toFileName);
            Assert.IsTrue(status, "Rename did not rename file to target file name.");
        }
    }
}
