using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.UnitTest.Framework.PathOperationTests;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Activities
{
    /// <summary>
    /// Summary description for FtpTests
    /// </summary>
    [TestClass]
    public class FtpTests
    {

        private string tmpfile1;
        private string tmpfile2;

        private string tmpdir1;
        private string tmpdir2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void CreateFiles()
        {
            tmpfile1 = Path.GetTempFileName();
            tmpfile2 = Path.GetTempFileName();

            FileStream fs1 = File.Create(tmpfile1);
            string tmp = "abc";
            byte[] data = Encoding.UTF8.GetBytes(tmp);
            fs1.Write(data, 0, data.Length);
            fs1.Close();

            FileStream fs2 = File.Create(tmpfile2);
            tmp = "def-hij";
            data = Encoding.UTF8.GetBytes(tmp);
            fs2.Write(data, 0, data.Length);
            fs2.Close();

            tmpdir1 = Path.GetTempPath() + Path.GetRandomFileName();
            tmpdir2 = Path.GetTempPath() + Path.GetRandomFileName();

            Directory.CreateDirectory(tmpdir1);
            Directory.CreateDirectory(tmpdir2);

            // populate directory with data
            string tmpFile = tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();
            tmpFile = tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();

            // populate the delete directory

        }

        [TestCleanup]
        public void RemoveFiles()
        {
            File.Delete(tmpfile1);
            File.Delete(tmpfile2);

            try
            {
                Directory.Delete(tmpdir1, true);
                Directory.Delete(tmpdir2, true);
            }
            catch (Exception) { }
        }



        private void RemoveCreatedDir(string path, string userName, string password, bool ftps)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(path.Replace("FTPS:", "FTP:").Replace("ftps:", "ftp:"));
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = ftps;
                if (userName != string.Empty)
                {
                    request.Credentials = new NetworkCredential(userName, password);
                }

                response = (FtpWebResponse)request.GetResponse();

                if (response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new Exception("File delete did not complete successfully");
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        private void CreateDirectory(string path, string userName, string password, bool ftps)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(path.Replace("FTPS:", "FTP:").Replace("ftps:", "ftp:"));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = ftps;

                if (userName != string.Empty)
                {
                    request.Credentials = new NetworkCredential(userName, password);
                }

                response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.PathnameCreated)
                {
                    throw new Exception("Directory was not created");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        #endregion

        #region Get Tests

        [TestMethod]
        public void GetWithNoUserName_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "GET_DATA/file1.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FTPPro.Get(path);

            int len = (int)result.Length;

            result.Close();

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithNoUserName_FileNotPresent_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "GET_DATA/file99.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                Stream result = FTPPro.Get(path);

                int len = (int)result.Length;

                result.Close();

                Assert.Fail("Got a stream for no file?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void GetWithCorrectUserName_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "GET_DATA/file1.txt", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            Stream result = FTPPro.Get(path);

            int len = (int)result.Length;

            result.Close();

            Assert.IsTrue(len > 0);

        }

        [TestMethod]
        public void GetWithWrongUserName_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "GET_DATA/file1.txt", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                Stream result = FTPPro.Get(path);

                int len = (int)result.Length;

                result.Close();

                Assert.Fail("Got a stream with wrong auth?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_Get")]
        [ExpectedException(typeof(Exception))]
        public void Dev2FTPProvider_Get_SFTPWrongFile_Exception()
        {
            //------------Setup for test--------------------------
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path + "/testing/ThisIsATest.txt", ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            FTPPro.Get(path);
            //------------Assert Results-------------------------
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_Get")]
        public void Dev2FTPProvider_Get_SFTPCorrectFile_DataReturned()
        {
            //------------Setup for test--------------------------
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path + "/testing/ThisIsATestFile.txt", ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            var textReader = new StreamReader(FTPPro.Get(path));
            //------------Assert Results-------------------------
            Assert.AreEqual("this is my test data", textReader.ReadToEnd());
        }

        [TestMethod]
        public void GetWithCorrectUserName_WrongFile_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "GET_DATA/file99.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                Stream result = FTPPro.Get(path);

                int len = (int)result.Length;

                result.Close();

                Assert.Fail("Got a stream for no file?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }

        #endregion Get Tests

        #region Put Tests

        [TestMethod]
        public void PutWithOverwriteFalse_NoUserPassword_FileNotExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string path = ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/" + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(path, "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_Put")]
        public void Dev2FTPProvider_Put_SFTP_OverwriteFalse_FileNotExist_ExpectedStream()
        {
            //------------Setup for test--------------------------
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path + string.Format("/testing/{0}.txt", Guid.NewGuid()), ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);
            const string data = "this is my test data";
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            Stream dataStream = new MemoryStream(byteArray);
            //------------Execute Test---------------------------
            var lengthOfWrittenBytes = FTPPro.Put(dataStream, path, args, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(lengthOfWrittenBytes>0);
            Assert.AreEqual(byteArray.Length,lengthOfWrittenBytes);
            FTPPro.Delete(path);
        }


        [TestMethod]
        public void PutWithOverwriteFalse_NoUserPassword_FileExist_Expected_NoStream()
        {
            //------------Setup for test--------------------------
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/file1.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);
            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);
            //------------Execute Test---------------------------
            int len = FTPPro.Put(s, dst, args, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(-1,len);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_Put")]
        public void Dev2FTPProvider_Put_SFTP_OverwriteFalse_FileExist_ExpectNothingReturned()
        {
            //------------Setup for test--------------------------
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path + string.Format("/testing/{0}.txt", "ThisIsATestFile"), ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);
            const string data = "this is my test data";
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            Stream dataStream = new MemoryStream(byteArray);
            //------------Execute Test---------------------------
            var lengthOfWrittenBytes = FTPPro.Put(dataStream, path, args, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(lengthOfWrittenBytes > 0);
            Assert.AreEqual(-1, lengthOfWrittenBytes);
        }


        [TestMethod]
        public void PutWithOverwriteTrue_NoUserPassword_FileNotExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/" + Guid.NewGuid() + ".test", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(true);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_NoUserPassword_FileExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/file1.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(true);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_CorrectUserPassword_FileNotExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string path = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/" + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(path, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_CorrectUserPassword_FileExist_Expected_NoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string path = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/file1.txt";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(path, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);

            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);


            try
            {
                Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

                int len = FTPPro.Put(s, dst, args, null);

                Assert.Fail("Overwrote a file when overwrite set to false?!");
            }
            catch (Exception)
            {
                s.Close();
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void PutWithOverwriteTrue_CorrectUserPassword_FileNotExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/" + Guid.NewGuid() + ".test", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(true);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_CorrectUserPassword_FileExist_Expected_Stream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/file1.txt", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(true);

            int len = FTPPro.Put(s, dst, args, null);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_WrongUserPassword_FileNotExist_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string path = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/" + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(path, ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            try
            {
                Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

                int len = FTPPro.Put(s, dst, args, null);

                Assert.Fail("Error");
            }
            catch (Exception)
            {
                s.Close();
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void PutWithOverwriteFalse_WrongUserPassword_FileExist_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string path = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/file1.txt";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(path, ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);

            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);


            try
            {
                Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

                int len = FTPPro.Put(s, dst, args, null);

                Assert.Fail("Overwrote a file when overwrite set to false?!");
            }
            catch (Exception)
            {
                s.Close();
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void PutWithOverwriteTrue_CorrectUserPassword_FileNotExist_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/" + Guid.NewGuid() + ".test", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            try
            {
                Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

                int len = FTPPro.Put(s, dst, args, null);

                Assert.Fail("Overwrote a file when overwrite set to false?!");
            }
            catch (Exception)
            {
                s.Close();
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void PutWithOverwriteTrue_WrongUserPassword_FileExist_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/file1.txt", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            byte[] data = File.ReadAllBytes(tmpfile2);
            Stream s = new MemoryStream(data);

            try
            {
                Dev2CRUDOperationTO args = new Dev2CRUDOperationTO(false);

                int len = FTPPro.Put(s, dst, args, null);

                Assert.Fail("Overwrote a file when overwrite set to false?!");
            }
            catch (Exception)
            {
                s.Close();
                Assert.IsTrue(1 == 1);
            }
        }


        #endregion Put Tests

        #region Delete Tests

        [TestMethod]
        public void DeleteWithNoUsername_FilePresent_Expected_Ok()
        {

            string delFile = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_NoAuth + "DEL_DATA/", string.Empty, string.Empty, false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(delFile, "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            Assert.IsTrue(FTPPro.Delete(path));
        }

        [TestMethod]
        public void DeleteWithNoUsername_FileNotPresent_Expected_Error()
        {

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DEL_DATA/abc.txt", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.Delete(path);
                Assert.Fail("Delete of non-existence path?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void DeleteWithValidUsername_FilePresent_Expected_Ok()
        {

            string delFile = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth + "DEL_DATA/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(delFile, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            Assert.IsTrue(FTPPro.Delete(path));
        }

        [TestMethod]
        public void DeleteWithValidUsername_FileNotPresent_Expected_Error()
        {

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "DEL_DATA/abc.txt", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.Delete(path);
                Assert.Fail("Delete of non-existence path?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void DeleteWithInValidUsername_FilePresent_Expected_Error()
        {

            string delFile = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth + "DEL_DATA/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(delFile, ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.Delete(path);
                Assert.Fail("Error");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void DeleteWithInValidUsername_FileNotPresent_Expected_Error()
        {

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "DEL_DATA/abc.txt", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.Delete(path);
                Assert.Fail("Delete of non-existence path?!");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        #endregion Delete Tests

        #region ListDirectory Tests

        [TestMethod]
        public void ListDirectoryWithNoUsername_ValidPath_Expected_List()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DIR_LIST/", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);

            Assert.IsTrue(tmp.Count == 3);
        }

        [TestMethod]
        public void ListDirectoryWithNoUsername_InValidPath_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DIR_LIST2/", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void ListDirectoryWithValidUsername_ValidPath_Expected_List()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "DIR_LIST/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);

            Assert.IsTrue(tmp.Count == 3);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_ListFoldersInDirectory")]
        public void Dev2FTPProvider_ListFoldersInDirectory_WithValidFTPDirectory_ReturnsListOfFoldersOnly()
        {
            //------------Setup for test--------------------------
            var path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth, "", "");
            var FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            var folderList = FTPPro.ListFoldersInDirectory(path);
            //------------Assert Results-------------------------
            Assert.AreEqual(7,folderList.Count);            
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_ListFoldersInDirectory")]
        public void Dev2FTPProvider_ListFoldersInDirectory_WithValidSFTPDirectory_ReturnsListOfFoldersOnly()
        {
            //------------Setup for test--------------------------
            var path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path, ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            var FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            var folderList = FTPPro.ListFoldersInDirectory(path);
            //------------Assert Results-------------------------
            Assert.IsTrue(folderList.Count > 10, "Only found " + folderList.Count + " directories on valid sFTP server");            
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_ListFilesInDirectory")]
        public void Dev2FTPProvider_ListFilesInDirectory_WithValidFTPDirectory_ReturnsListOfFoldersOnly()
        {
            //------------Setup for test--------------------------
            var path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth, "", "");
            var FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            var folderList = FTPPro.ListFilesInDirectory(path);
            //------------Assert Results-------------------------
            Assert.AreEqual(1,folderList.Count);            
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_ListFilesInDirectory")]
        public void Dev2FTPProvider_ListFilesInDirectory_WithValidSFTPDirectory_ReturnsListOfFoldersOnly()
        {
            //------------Setup for test--------------------------
            var path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_SFTP_Path, ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password);
            var FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            //------------Execute Test---------------------------
            var folderList = FTPPro.ListFilesInDirectory(path);
            //------------Assert Results-------------------------
            Assert.IsTrue(folderList.Count > 10, "List files in ftp directory returnd less than 10 files");
        }

        [TestMethod]
        public void ListDirectoryWithValidUsername_InValidPath_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DIR_LIST2/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void ListDirectoryWithInValidUsername_ValidPath_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "DIR_LIST/", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);


            try
            {
                IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);
                Assert.Fail("Error");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void ListDirectoryWithInValidUsername_InValidPath_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DIR_LIST2/", ParserStrings.PathOperations_Correct_Username + "abc", ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);
                Assert.Fail("Error");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }

        }

        #endregion ListDirectory Tests

        #region CreateDirectory Tests

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_NoUser_Expected_Ok()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string newDir = Guid.NewGuid() + "_test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            bool ok = FTPPro.CreateDirectory(path, opTO);

            // clean up
            RemoveCreatedDir(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir + "/", "", "", false);

            Assert.IsTrue(ok);

        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_NoUser_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string newDir = Guid.NewGuid() + "_test";

            CreateDirectory(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, "", "", false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.CreateDirectory(path, opTO);
                Assert.Fail("error");
            }
            catch (Exception)
            {
                // clean up
                RemoveCreatedDir(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir + "/", "", "", false);
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_NoUser_Expected_Ok()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string newDir = Guid.NewGuid() + "_test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            bool ok = FTPPro.CreateDirectory(path, opTO);

            // clean up
            RemoveCreatedDir(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir + "/", "", "", false);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_ValidUser_Expected_Ok()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string newDir = Guid.NewGuid() + "_test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            bool ok = FTPPro.CreateDirectory(path, opTO);

            // clean up
            RemoveCreatedDir(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.IsTrue(ok);

        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_ValidUser_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string newDir = Guid.NewGuid() + "_test";

            CreateDirectory(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                bool ok = FTPPro.CreateDirectory(path, opTO);
                Assert.Fail("error");
            }
            catch (Exception)
            {
                // clean up
                RemoveCreatedDir(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir + "/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
                Assert.IsTrue(1 == 1);
            }

        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_ValidUser_Expected_Ok()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string newDir = Guid.NewGuid() + "_test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            bool ok = FTPPro.CreateDirectory(path, opTO);

            // clean up
            RemoveCreatedDir(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir + "/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.IsTrue(ok);

        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_ValidUser_Expected_Error()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string newDir = Guid.NewGuid() + "_test";

            CreateDirectory(ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            bool ok = FTPPro.CreateDirectory(path, opTO);

            // clean up
            RemoveCreatedDir(ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/DIR_CREATE/" + newDir + "/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.IsTrue(ok);
        }

        #endregion CreateDirectory Tests
    }
}
