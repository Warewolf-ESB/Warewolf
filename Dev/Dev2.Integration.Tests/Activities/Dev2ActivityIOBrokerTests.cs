using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Unlimited.UnitTest.Framework.PathOperationTests;

namespace Dev2.Integration.Tests.Activities
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests
    {
        private string tmpfile1;
        private string tmpfile2;

        private string tmpdir1;
        private string tmpdir2;

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

        #endregion

        /// <summary>
        /// WriteFile file system invalid credentials no overwrite.
        /// </summary>
        [TestMethod]
        public void WriteFile_FileSystem_InvalidCredentials_NoOverwrite_Expected_ExceptionThrownByBroker()
        {
            try
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(true, "XYZ", false);
                IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, TestResource.PathOperations_Incorrect_Username, TestResource.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

                string result = broker.PutRaw(endPoint, opTO);

                Assert.Fail(@"Exception not thrown when it should have been thrown");
            }
            catch (Exception ex)
            {
                string expected = string.Format(@"Failed to authenticate with user [ {0} ] for resource [ {1} ] ", TestResource.PathOperations_Incorrect_Username, tmpfile1);
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Movefile file system to file system invalid credentials overwrite expected exception thrown for invalid credentials.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MoveFile_FileSystemToFileSystem_InvalidCredentials_Overwrite_Expected_ExceptionThrownForInvalidCredentials()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp1 = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            string tmp2 = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp1);
            PathIOTestingUtils.CreateAuthedUNCPath(tmp2);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp1, "DEV2\\" + TestResource.PathOperations_Incorrect_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp2, "DEV2\\" + TestResource.PathOperations_Incorrect_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(tmp1);
            PathIOTestingUtils.DeleteAuthedUNCPath(tmp2);

            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        public void CopyFromFileSystem_WithFilePresent_InvalidCrendentials_Overwrite_FileAndOutputExists()
        {
            try
            {
                Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2,"","");
                IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1, TestResource.PathOperations_Incorrect_Username, TestResource.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

                File.Delete(tmpfile2);

                Assert.Fail("Exception was not thrown when it was ment to be thrown");
            }
            catch (Exception)
            {
                Assert.AreEqual(1, 1);
            }
        }

        /// <summary>
        /// WriteFile file system invalid credentials overwrite expected exception thrown by broker.
        /// </summary>
        [TestMethod]
        public void WriteFile_FileSystem_InvalidCredentials_Overwrite_Expected_ExceptionThrownByBroker()
        {
            try
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
                IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile2, TestResource.PathOperations_Incorrect_Username, TestResource.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

                string result = broker.PutRaw(endPoint, opTO);

                Assert.Fail(@"Exception not thrown when it should have been thrown");
            }
            catch (Exception ex)
            {
                string expected = string.Format(@"Failed to authenticate with user [ {0} ] for resource [ {1} ] ", TestResource.PathOperations_Incorrect_Username, tmpfile2);
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Movefile file system to file system no overwrite expected_ file copied.
        /// </summary>
        [TestMethod]
        public void MoveFile_FileSystemToFileSystem_NoOverwrite_Expected_FileCopied()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmp);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Movefile file system to file system overwrite expected file copied and overwritten in new location.
        /// </summary>
        [TestMethod]
        public void MoveFile_FileSystemToFileSystem_Overwrite_Expected_FileCopiedAndOverwrittenInNewLocation()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmp);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Movefile file system to file system overwrite no file specified expected file not found exception thrown by broker.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void MoveFile_FileSystemToFileSystem_Overwrite_NoFile_Expected_FileNotFoundExceptionThrownByBroker()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(@"C://NoFile.txt", "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmp);
        }

        /// <summary>
        /// Movefile FTP to FTP invalid credentials overwrite exception thrown by broker.
        /// </summary>
        [TestMethod]
        public void MoveFile_FTPToFTP_InvalidCredentials_Overwrite_Expected_ExceptionThrownByBroker()
        {
            try
            {
                Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);

                string basePath = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";

                string tmp1 = PathIOTestingUtils.CreateFileFTP(basePath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
                string tmp2 = basePath + Guid.NewGuid() + ".test";

                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp2, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp1, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

                PathIOTestingUtils.DeleteFTP(tmp2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);


                Assert.Fail("Exception not thrown when it should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(@"The remote server returned an error: (530) Not logged in.", ex.Message);
            }
        }

        /// <summary>
        /// MoveFile FTP to FTP invalid credentials no overwrite file present.
        /// </summary>
        [TestMethod]
        public void MoveFile_FTPToFTP_InvalidCredentials_NoOverwrite_FilePresent_ExpectedExceptionThrownByBroker()
        {
            try
            {
                Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);

                string basePath = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";

                string tmp1 = PathIOTestingUtils.CreateFileFTP(basePath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
                string tmp2 = PathIOTestingUtils.CreateFileFTP(basePath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp2, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp1, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

                PathIOTestingUtils.DeleteFTP(tmp2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

                Assert.Fail("Exception not thrown when it should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(@"The remote server returned an error: (530) Not logged in.", ex.Message);
            }
        }


        /// <summary>
        /// Movefile FTP to FTP valid user overwrite file present expected file overwritten.
        /// </summary>
        [TestMethod]
        public void MoveFile_FTPToFTP_ValidUser_Overwrite_FilePresent_Expected_FileOverwritten()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);

            string basePath = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";

            string tmp1 = PathIOTestingUtils.CreateFileFTP(basePath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            string tmp2 = PathIOTestingUtils.CreateFileFTP(basePath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp1, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteFTP(tmp2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// Movefile FTP to FTP invalid user overwrite file not present.
        /// </summary>
        [TestMethod]
        public void MoveFile_FTPToFTP_InvalidUser_Overwrite_FileNotPresent_ExceptionThrownByBroker()
        {
            try
            {
                Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
                string tmp = System.IO.Path.GetTempFileName();
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOPath src = ActivityIOFactory.CreatePathFromString(@"ftp://NoFile.txt", "", "");
                IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                string result = broker.Move(scrEndPoint, dstEndPoint, opTO);

                PathIOTestingUtils.DeleteFTP(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
                Assert.Fail(@"Exception wasnt thrown when it was ment to be thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(@"The remote name could not be resolved: 'nofile.txt'", ex.Message);
            }

        }

        /// <summary>
        /// Movefile file sysytem to FTPS valid credentials accept invalid cert overwrite.
        /// </summary>
        [TestMethod]
        public void MoveFile_FileSysytemToFTPS_ValidCredentials_AcceptInvalidCert_Overwrite_Expected_SuccessfulMove()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(string.Concat(ParserStrings.PathOperations_FTPS_AuthPath, "/TestDirectory/Testing.txt"), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, true);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOOperationsEndPoint srcEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Move(srcEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteFTP(string.Concat(ParserStrings.PathOperations_FTPS_AuthPath, "/TestDirectory/Testing.txt"), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, true);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Movefile file sysytem to FTPS valid credentials reject invalid cert overwrite Exception thrown by Broker.
        /// </summary>
        [TestMethod]
        public void MoveFile_FileSysytemToFTPS_ValidCredentials_RejectInvalidCert_Overwrite_Expecte_ExceptionThrownByBroker()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(string.Concat(ParserStrings.PathOperations_FTPS_AuthPath, "/TestDirectory/Testing.txt"), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOOperationsEndPoint srcEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            try
            {
                string result = broker.Move(srcEndPoint, dstEndPoint, opTO);

                Assert.Fail("There are no valid certs");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);

            }
        }


        #region Write Unit Test

        /// <summary>
        /// Writefile append false overwrite true file system expected file overwritten.
        /// </summary>
        [TestMethod]
        public void WriteFile_AppendFalse_OverWriteTrue_FileSystem_Expected_FileOverwritten()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile append true overwrite false system credentials file system.
        /// </summary>
        [TestMethod]
        public void WriteFile_AppendTrue_OverWriteFalse_SystemCredentials_FileSystem_Expected_FileAppended()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(true, "XYZ", false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile append true overwrite true system credentials file system expected file is overwritten.
        /// </summary>
        [TestMethod]
        public void WriteFile_AppendTrue_OverWriteTrue_SystemCredentials_FileSystem_Expected_FileOverwritten()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(true, "XYZ", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }


        //Moved to Integration Tests
        ///// <summary>
        ///// WriteFile file system invalid credentials no overwrite.
        ///// </summary>
        //[TestMethod]
        //public void WriteFile_FileSystem_InvalidCredentials_NoOverwrite_Expected_ExceptionThrownByBroker() {
        //    try {
        //        IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
        //        Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(true, "XYZ", false);
        //IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
        //        IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

        //        string result = broker.PutRaw(endPoint, opTO);

        //        Assert.Fail(@"Exception not thrown when it should have been thrown");
        //    }
        //    catch (Exception ex) {
        //        string expected = string.Format(@"Failed to authenticate with user [ {0} ] for resource [ {1} ] ", ParserStrings.PathOperations_Incorrect_Username, tmpfile1);
        //        Assert.AreEqual(expected, ex.Message);
        //    }
        //}

        /// <summary>
        /// WritesFile file system system credentials overwrite expected file overwritten.
        /// </summary>
        [TestMethod]
        public void WriteFile_FileSystem_SystemCredentials_Overwrite_Expected_FileOverwritten()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile file system system credentials overwrite no file expected file created.
        /// </summary>
        [TestMethod]
        public void WriteFile_FileSystem_SystemCredentials_Overwrite_NoFile_Expected_FileCreated()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        //Moved to Integration Tests
        ///// <summary>
        ///// WriteFile file system invalid credentials overwrite expected exception thrown by broker.
        ///// </summary>
        //[TestMethod]
        //public void WriteFile_FileSystem_InvalidCredentials_Overwrite_Expected_ExceptionThrownByBroker() {
        //    try {
        //        IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
        //        Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
        //        IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile2, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
        //        IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

        //        string result = broker.PutRaw(endPoint, opTO);

        //        Assert.Fail(@"Exception not thrown when it should have been thrown");
        //    }
        //    catch (Exception ex) {
        //        string expected = string.Format(@"Failed to authenticate with user [ {0} ] for resource [ {1} ] ", ParserStrings.PathOperations_Incorrect_Username, tmpfile2);
        //        Assert.AreEqual(expected, ex.Message);
        //    }
        //}

        /// <summary>
        /// WriteFile FTP valid credentials overwrite.
        /// </summary>

        [TestMethod]
        public void WriteFile_FTP_ValidCredentials_Overwrite_Expected_FileOverwritten()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);
            opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "abc", false);
            result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile FTP valid credentials no overwrite file present file appended.
        /// </summary>

        [TestMethod]
        public void WriteFile_FTP_ValidCredentials_NoOverwrite_FilePresent_FileAppended()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);
            opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "abc", false);
            result = broker.PutRaw(endPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile_ FTP invalid credentials overwrite expected_ exception thrown by broker.
        /// </summary>
        [TestMethod]
        public void WriteFile_FTP_InvalidCredentials_Overwrite_Expected_ExceptionThrownByBroker()
        {
            try
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", true);
                IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

                string result = broker.PutRaw(endPoint, opTO);

                Assert.Fail(@"Exception not thrown when it should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(@"The remote server returned an error: (530) Not logged in.", ex.Message);
            }
        }


        /// <summary>
        /// WriteFile FTP valid credentials overwrite file present expected file overwritten.
        /// </summary>
        [TestMethod]
        public void WriteFile_FTP_ValidCredentials_Overwrite_FilePresent_Expected_FileOverwritten()
        {

            string ftpPath = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "abc", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            PathIOTestingUtils.DeleteFTP(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile FTP correct user overwrite file not present expects the file to be created.
        /// </summary>
        [TestMethod]
        public void WriteFile_FTP_CorrectUser_Overwrite_FileNotPresent_Expected_FileCreated()
        {

            string ftpPath = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "abc", true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);

            PathIOTestingUtils.DeleteFTP(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.AreEqual(@"Success", result);
        }

        /// <summary>
        /// WriteFile FTP wrong user no overwrite expected exception thrown by broker.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void WriteFile_FTP_WrongUser_NoOverwrite_Expected_ExceptionThrownByBroker()
        {
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            Dev2PutRawOperationTO opTO = ActivityIOFactory.CreatePutRawOperationTO(false, "XYZ", false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            string result = broker.PutRaw(endPoint, opTO);
        }

        #endregion

        #region Zip Unit Test

        /// <summary>
        /// Zip from file system to file system no authentication dir.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FileSystem_NoAuth_Dir_Expected_ZipFileCreatedInDestination()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);

        }

        /// <summary>
        /// Zip from_ file systen to file system no authentication directory password protection enabled expected password protected 
        /// zip file created in destination.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FileSystem_NoAuth_Dir_PasswordProtection_Expected_PasswordProtectedZipFileCreatedInDestination()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, "test", "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);

        }

        /// <summary>
        /// Zip from file system to file system no authentication directory file not exist.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FileSystem_NoAuth_Dir_FileNotExist_Expected_FileCreatedInDestination()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir,"","");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);

        }


        /// <summary>
        /// Zip from file system to file system no authentication dir compression level best expected zip file 
        /// created in destination.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FileSystem_NoAuth_Dir_CompressionLevelBest_Expected_ZipFileCreatedInDestination()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();
            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2ZipOperationTO args = new Dev2ZipOperationTO(Ionic.Zlib.CompressionLevel.BestCompression.ToString(), string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);

        }

        /// <summary>
        /// Zip from file system to file system no authentication file expected file created in destination.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FileSystem_NoAuth_File_Expected_FileCreatedInDestination()
        {
            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(file1, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip from file system to FTP dir.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FTP_Dir_Expected_ZipFileCreatedInFTP()
        {
            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, (tmpID + "TestArchive.zip"));

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip the from file system to FTP file.
        /// </summary>
        [TestMethod]
        public void Zip_From_FileSystem_To_FTP_File_ZipFileCreatedInFTP()
        {
            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(file1, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, (tmpID + "TestArchive.zip"));

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(srcDir);
            PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip the from FTP to FTP directory expected zip file created in FTP.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FTP_Dir_Expected_ZipFileCreatedInFTP()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/";
            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, (tmpID + "TestArchive.zip"));

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

            Assert.AreEqual("Success", result);

        }

        /// <summary>
        /// Zip from FTP to FTP file no authentication server expected zip file create in FTP server.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FTP_File_NoAuthServer_Expected_ZipFileCreateInFTPServer()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/file1.txt";
            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, (tmpID + "TestArchive.zip"));

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip from FTP to file system directory expected zip file created on file system.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FileSystem_Dir_Expected_ZipFileCreatedOnFileSystem()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(dstDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(dstDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip from FTP to file system file no authentication server.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FileSystem_File_NoAuthServer_Expected_ZipFileCreatedOnFileSystem()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/file1.txt";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(dstDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(dstDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip from FTP to File System file with valid FTP credentials expected zip file created on file system.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FileSystem_File_ValidFTPCredentials_ExpectedZipFileCreatedOnFileSystem()
        {
            string srcDir = ParserStrings.PathOperations_FTP_Auth + "/GET_DATA/file1.txt";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(dstDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(dstDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");

            string result = broker.Zip(srcEP, dstEP, args);

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(dstDir);

            Assert.AreEqual("Success", result);
        }

        /// <summary>
        /// Zip from FTP to file system file with invalid FTP credentials expected_ exception thrown by broker.
        /// </summary>
        [TestMethod]
        public void Zip_From_FTP_To_FileSystem_File_InvalidFTPCredentials_Expected_ExceptionThrownByBroker()
        {
            string srcDir = ParserStrings.PathOperations_FTP_Auth + "/GET_DATA/file1.txt";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(dstDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(dstDir);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Incorrect_Password);
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2ZipOperationTO args = new Dev2ZipOperationTO(string.Empty, string.Empty, "TestArchive.zip");
            try
            {
                string result = broker.Zip(srcEP, dstEP, args);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The remote server returned an error: (530) Not logged in.", ex.Message);
            }
        }


        #endregion

        #region Unzip Unit Test


        #region FileSystem

        /// <summary>
        /// Unzip from file system to file system no auth file expected file unzipped to file system.
        /// </summary>
        [TestMethod]
        public void UnZip_From_FileSystem_To_FileSystem_NoAuth_File_Expected_FileUnzippedToFileSystem()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string[] files = new string[] { file1 };
            string ZipFileName = PathIOTestingUtils.ZipFile(srcDir, files);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ZipFileName, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath UnzipDestinationPath = ActivityIOFactory.CreatePathFromString(dstDir + "\\Users", "", "");
            Dev2UnZipOperationTO unzipOpTO = ActivityIOFactory.CreateUnzipTO(null);
            broker.UnZip(srcEP, dstEP, unzipOpTO);
            string fileName = file1.Remove(0, @"C:\".Length);
            bool unzippedFileExists = File.Exists(dstDir + @"\" + Path.GetFileName(file1));

            // Clean up
            PathIOTestingUtils.DeleteTmpDir(dstDir);
            PathIOTestingUtils.DeleteTmpDir(srcDir);

            Assert.IsTrue(unzippedFileExists);

        }

        /// <summary>
        /// Unzip from file system to file system invalid credentials expected_ file created.
        /// </summary>
        [TestMethod]
        public void UnZip_From_FileSystem_To_FileSystem_InvalidCredentials_Expected_FileCreated()
        {

            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string[] files = new string[] { file1 };
            string ZipFileName = PathIOTestingUtils.ZipFile(srcDir, files);

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ZipFileName, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Incorrect_Password);
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Incorrect_Password);
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath UnzipDestinationPath = ActivityIOFactory.CreatePathFromString(dstDir + "\\Users", "", "");
            Dev2UnZipOperationTO unzipOpTO = ActivityIOFactory.CreateUnzipTO(null);

            string result = broker.UnZip(srcEP, dstEP, unzipOpTO);

            string fileName = file1.Remove(0, @"C:\".Length);
            bool unzippedFileExists = File.Exists(dstDir + @"\" + Path.GetFileName(file1));

            // Clean up
            PathIOTestingUtils.DeleteTmpDir(dstDir);
            PathIOTestingUtils.DeleteTmpDir(srcDir);

            Assert.IsTrue(unzippedFileExists);

        }

        //BUILD
        /// <summary>
        /// Unzip from file system to FTP no authentication server 
        /// directory not exist expected directory created on FTP and file created in directory.
        /// </summary>
        //[TestMethod]
        //public void UnZip_From_FileSystem_To_FTP_NoAuthServer_Dir_DirNotExists_Expected_DirectoryCreatedOnFTPAndFileCreatedInDirectory() {
        //    string srcDir = PathIOTestingUtils.CreateTmpDirectory();
        //    string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "PUT_DATA/";
        //    string zipDirectory = string.Concat(srcDir, @"\TestZipFile.zip");

        //    string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
        //    string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);
        //    string[] files = new string[] { file1, file2 };

        //    //string ZipFileName = PathIOTestingUtils.ZipFile(srcDir, files); 
        //    IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
        //    IActivityIOPath tmpSrc = ActivityIOFactory.CreatePathFromString(srcDir);
        //    IActivityIOPath tmpDst = ActivityIOFactory.CreatePathFromString(zipDirectory);
        //    Dev2ZipOperationTO argsZip = ActivityIOFactory.CreateZipTO("", "", "");
        //    IActivityIOOperationsEndPoint tmpsrcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpSrc);
        //    IActivityIOOperationsEndPoint tmpdstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpDst);
        //    string ZipFileName = broker.Zip(tmpsrcEP, tmpdstEP, argsZip);

        //    IActivityIOPath src = ActivityIOFactory.CreatePathFromString(zipDirectory);
        //    IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
        //    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
        //    IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);



        //    Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("");

        //    string result = broker.UnZip(srcEP, dstEP, args);


        //    // retrieve all the FTP data
        //    StreamReader reader = new StreamReader(PathIOTestingUtils.FileExistsFTP(dstDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false));

        //    string fileData = reader.ReadToEnd();

        //    string file1FileName = Path.GetFileName(file1);
        //    string file2FileName = Path.GetFileName(file2);
        //    bool truth = fileData.Contains(file1FileName);
        //    Assert.IsTrue(fileData.Contains(file1FileName));
        //    Assert.IsTrue(fileData.Contains(file2FileName));

        //    reader.Close();
        //    reader.Dispose();
        //    broker.Delete(tmpsrcEP);
        //    broker.Delete(tmpdstEP);
        //    string file1name = Dev2ActivityIOPathUtils.ExtractFileName(file1);
        //    dst = ActivityIOFactory.CreatePathFromString(string.Concat(dstDir, file1name), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
        //    dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
        //    broker.Delete(dstEP);
        //    string file2name = Dev2ActivityIOPathUtils.ExtractFileName(file2);
        //    dst = ActivityIOFactory.CreatePathFromString(string.Concat(dstDir, file2name), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
        //    dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
        //    broker.Delete(dstEP);



        //}

        /// <summary>
        /// Unzip from file system to FTP file authentication invalid crendentials files and output exists
        /// expected exception thrown by broker.
        /// </summary>
        [TestMethod]
        public void UnZip_From_FileSystem_To_FTP_File_Auth_InvalidCrendentials_FilesAndOutputExists_Expected_ExceptionThrownByBroker()
        {
            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string dstDir = ParserStrings.PathOperations_FTP_Auth + "/PUT_DATA/";

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string[] files = new string[] { file1, file2 };

            string ZipFileName = PathIOTestingUtils.ZipFile(srcDir, files);
            string ZipPath = Path.GetFileName(ZipFileName);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ZipFileName, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Incorrect_Password);
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();

            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO(null);
            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception ex)
            {
                StringAssert.Contains(ex.Message, @"Failed to authenticate with user [ Dev2\frank.williams ]");
            }
            src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            broker.Delete(srcEP);
        }

        [TestMethod]
        public void UnZip_From_File_To_FTP_Dir_NoAuthServer_InvalidArchivePassword_Expected()
        {
            string srcDir = PathIOTestingUtils.CreateTmpDirectory();
            string tmpID = Guid.NewGuid().ToString();

            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            string file1 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string file2 = PathIOTestingUtils.CreateTmpFile(srcDir);
            string[] files = new string[] { file1, file2 };

            string ZipFileName = PathIOTestingUtils.ZipFile(srcDir, files, "test");
            string ZipPath = Path.GetFileName(ZipFileName);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ZipFileName, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("test123");


            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception exception)
            {
                Assert.AreEqual("The password did not match.", exception.Message);
            }
        }

        #endregion FileSystem

        #region FTP

        [TestMethod]
        public void UnZip_From_FTP_To_FTP_Dir_NoAuthServer_NoArchivePassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/";
            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/Test/";

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir + "TestZipFile.zip", "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO(null);

            string result = broker.UnZip(srcEP, dstEP, args);

            // clean-up
            // PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

            StreamReader reader = new StreamReader(PathIOTestingUtils.FileExistsFTP(dstDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false));

            string fileData = reader.ReadToEnd();
            string file1FileName = "af3c3362-40e0-41c0-a958-be371ad323f3.test";
            string file2FileName = "a7f17db0-1d17-4007-bbf1-e1951f1a37f8.test";

            Assert.IsTrue(fileData.Contains(file2FileName));

            reader.Close();
            reader.Dispose();
            PathIOTestingUtils.DeleteFTP(dstDir + "/" + file1FileName, string.Empty, string.Empty, false);
            PathIOTestingUtils.DeleteFTP(dstDir + "/" + file2FileName, string.Empty, string.Empty, false);
            PathIOTestingUtils.DeleteFTP(dstDir, string.Empty, string.Empty, false);
        }

        [TestMethod]
        public void UnZip_From_FTP_To_FileSystem_Dir_NoAuthServer_ArchivePassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/TestZipFilePw.zip";
            string tmpID = Guid.NewGuid().ToString();

            string dstDir = PathIOTestingUtils.CreateTmpDirectory();


            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("test");


            string result = broker.UnZip(srcEP, dstEP, args);

            string file1FileName = "e0da0d9d-5564-48c5-bbac-c2c7da120100.test";
            string[] files = Directory.GetFiles(dstDir);
            string resultFile1 = Path.GetFileName(files.Last());
            Assert.AreEqual(file1FileName, resultFile1);

            // clean up after we're done
            PathIOTestingUtils.DeleteTmpDir(dstDir);

        }

        [TestMethod]
        public void UnZip_From_FTP_To_FileSystem_Dir_NoAuthServer_InvalidArchivePassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/TestZipFilePw.zip";
            string tmpID = Guid.NewGuid().ToString();

            string dstDir = PathIOTestingUtils.CreateTmpDirectory();


            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("tester");

            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The password did not match.", ex.Message);
            }

            // clean up after we're done
            PathIOTestingUtils.DeleteTmpDir(dstDir);

        }

        //BUILD
        //[TestMethod]
        //public void UnZip_From_FTP_To_FTP_Dir_NoAuthServer_ArchivePassword() {
        //    string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/TestZipFilePw.zip";

        //    string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";


        //    IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir);
        //    IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
        //    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir);
        //    IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

        //    IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

        //    Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("test");


        //    string result = broker.UnZip(srcEP, dstEP, args);


        //    // retrieve all the FTP data - this will 
        //    StreamReader reader = new StreamReader(PathIOTestingUtils.FileExistsFTP(dstDir, string.Empty, string.Empty, false));

        //    string fileData = reader.ReadToEnd();

        //    string file1FileName = "e0da0d9d-5564-48c5-bbac-c2c7da120100.test";
        //    string file2FileName = "42f38bcd-de3b-4a31-8c80-5d6672ac265b.test";
        //    Assert.IsTrue(fileData.Contains(file1FileName));

        //    // clean up after we're done
        //    reader.Close();
        //    reader.Dispose();
        //    PathIOTestingUtils.DeleteFTP(dstDir + dstEP.PathSeperator() + file1FileName, string.Empty, string.Empty, false);
        //    PathIOTestingUtils.DeleteFTP(dstDir + dstEP.PathSeperator() + file2FileName, string.Empty, string.Empty, false);

        //}

        [TestMethod]
        public void UnZip_From_FTP_To_FTP_Dir_NoAuthServer_InvalidArchivePassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/TestZipFilePw.zip";
            string tmpID = Guid.NewGuid().ToString();

            string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir, "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("Incorrect");


            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception exception)
            {
                Assert.AreEqual("The password did not match.", exception.Message);
            }
        }

        //BUILD
        //[TestMethod]
        //public void UnZip_From_FTP_To_FTP_File_ValidPassword() {
        //    string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/";
        //    string dstDir = ParserStrings.PathOperations_FTP_NoAuth + "/PUT_DATA/";

        //    IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir + "TestZipFilePw.zip");
        //    IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
        //    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir);
        //    IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

        //    IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

        //    string tmpID = Guid.NewGuid().ToString();
        //    Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO("test");

        //    string result = broker.UnZip(srcEP, dstEP, args);

        //    // clean-up
        //    // PathIOTestingUtils.DeleteFTP(dstDir + (tmpID + "TestArchive.zip"), string.Empty, string.Empty, false);

        //    StreamReader reader = new StreamReader(PathIOTestingUtils.FileExistsFTP(dstDir, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false));

        //    string fileData = reader.ReadToEnd();
        //    string file1FileName = "42f38bcd-de3b-4a31-8c80-5d6672ac265b.test";
        //    string file2FileName = "e0da0d9d-5564-48c5-bbac-c2c7da120100.test";

        //    Assert.IsTrue(fileData.Contains(file2FileName));

        //    reader.Close();
        //    reader.Dispose();
        //    PathIOTestingUtils.DeleteFTP(dstDir + "/" + file1FileName, string.Empty, string.Empty, false);
        //    PathIOTestingUtils.DeleteFTP(dstDir + "/" + file2FileName, string.Empty, string.Empty, false);
        //}

        [TestMethod]
        public void UnZip_From_FTP_To_FileSystem_Dir_ValidArchivePassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/GET_DATA/";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir + "TestZipFile.zip", "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO(null);

            string result = broker.UnZip(srcEP, dstEP, args);

            // Verify that the data is actually the data that was in the zip file
            string[] fileData = Directory.GetFiles(dstDir);
            string data = string.Empty;
            foreach (string unzipData in fileData)
            {
                data += Path.GetFileName(unzipData) + ",";
            }

            string file1FileName = "a7f17db0-1d17-4007-bbf1-e1951f1a37f8.test";
            string file2FileName = "af3c3362-40e0-41c0-a958-be371ad323f3.test";

            Assert.AreEqual(data, file1FileName + "," + file2FileName + ",");

            // clean-up
            PathIOTestingUtils.DeleteTmpDir(dstDir);
        }


        [TestMethod]
        public void Unzip_FTP_To_FileSystem_FileNotExists_ValidPassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/testssdfs/";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir + "TestZipFile.zip", "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO(null);

            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The remote server returned an error: (550) File unavailable (e.g., file not found, no access).", ex.Message);
            }
        }

        [TestMethod]
        public void Unzip_FTP_To_FileSystem_DestinationNotgExists_ValidPassword()
        {
            string srcDir = ParserStrings.PathOperations_FTP_NoAuth + "/testssdfs/";
            string dstDir = PathIOTestingUtils.CreateTmpDirectory();

            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcDir + "TestZipFile.zip", "", "");
            IActivityIOOperationsEndPoint srcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(dstDir, "", "");
            IActivityIOOperationsEndPoint dstEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string tmpID = Guid.NewGuid().ToString();
            Dev2UnZipOperationTO args = ActivityIOFactory.CreateUnzipTO(null);

            try
            {
                string result = broker.UnZip(srcEP, dstEP, args);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The remote server returned an error: (550) File unavailable (e.g., file not found, no access).", ex.Message);
            }
        }

        #endregion FTP

        #endregion Unzip Unit Test

        #region Copy Unit Test

        [TestMethod]
        public void CopyFrom_FileSystem_Overwrite_SystemCredentials_FileExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmpfile2);

            Assert.AreEqual(@"Success", result);
        }

        [TestMethod]
        public void CopyFrom_FileToFolder_Overwrite_SystemCredentials_FileExists_FolderExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpdir1, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        [TestMethod]
        public void CopyFrom_FileToFile_Expected_Create_New_File_Paste_Content()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string fileName = "\\TestFile.txt";


            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpdir1 + fileName, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);
            string preFileContents = broker.Get(scrEndPoint);
            string postFileContents = broker.Get(dstEndPoint);

            broker.Delete(dstEndPoint);

            Assert.AreEqual(@"Success", result);
            Assert.AreEqual(postFileContents, preFileContents);
        }

        [TestMethod]
        public void CopyFrom_FileToFolderRecursiveCreate_Overwrite_SystemCredentials_FileExistsFolderNotExist()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string theDst = tmpdir1 + "\\" + Guid.NewGuid() + "_dir\\" + Guid.NewGuid() + "\\";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(theDst, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            Assert.AreEqual(@"Success", result);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void CopyFromFileSystem_WithNoFilePresent_Overwrite_SystemCredentials_OutputExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            string srcFile = tmpdir1 + "abc.txt";
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(srcFile, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmpfile2);
        }

        [TestMethod]
        public void CopyFromFileSystem_WithFilePresent_Credentials_Correct_Overwrite_FilesAndOutputExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string file1 = ParserStrings.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            string file2 = ParserStrings.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            PathIOTestingUtils.CreateAuthedUNCPath(file1);

            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(file2, "DEV2\\" + ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(file1, "DEV2\\" + ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(file1);
            PathIOTestingUtils.DeleteAuthedUNCPath(file2);

            Assert.AreEqual(@"Success", result);
        }

        //Moved to Integration Tests
        //[TestMethod]
        //public void CopyFromFileSystem_WithFilePresent_InvalidCrendentials_Overwrite_FileAndOutputExists() {
        //try {
        //        Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
        //        IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2);
        //        IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile1,ParserStrings.PathOperations_Incorrect_Username,ParserStrings.PathOperations_Correct_Password);
        //        IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
        //        IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
        //        IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

        //        string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

        //        File.Delete(tmpfile2);

        //        Assert.Fail("Exception was not thrown when it was ment to be thrown");
        //    }
        //    catch (Exception ) {                
        //        Assert.AreEqual(1,1);
        //    }
        //}

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void CopyFromFTP_WithNOFilePresent_Overwrite_SystemCredentials()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(@"ftp://wrongFile.txt", "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmpfile2);
        }

        [TestMethod]
        public void CopyFromFTP_WithFilePresent_ValidCredentials_OverwriteTrue()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string ftpPath = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            string ftpPath2 = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ftpPath2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteFTP(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            PathIOTestingUtils.DeleteFTP(ftpPath2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);

            Assert.AreEqual(@"Success", result);
        }

        [TestMethod]       
        public void CopyFromFTP_WithFilePresent_ValidCredentials_OverwriteFalse()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string ftpPath = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            string ftpPath2 = PathIOTestingUtils.CreateFileFTP(ParserStrings.PathOperations_FTP_Auth, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ftpPath2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            PathIOTestingUtils.DeleteFTP(ftpPath, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            PathIOTestingUtils.DeleteFTP(ftpPath2, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, false);
            Assert.AreEqual("Failure", result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void CopyFromFTP_WithFilePresent_InvalidCredentials_OverwriteFalse()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmpfile2, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_AuthPath, ParserStrings.PathOperations_Incorrect_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmpfile2);
        }

        [TestMethod]
        public void CopyFromFTPSToFileSystem_WithFilePresent_ValidCredentials_Overwrite()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = Path.GetTempFileName();
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(string.Concat(ParserStrings.PathOperations_FTPS_AuthPath, "/TestDirectory/file1.txt"), ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password, true);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

            string result = broker.Copy(scrEndPoint, dstEndPoint, opTO);

            File.Delete(tmp);

            Assert.AreEqual("Success", result);
        }

        #endregion
    }
}
