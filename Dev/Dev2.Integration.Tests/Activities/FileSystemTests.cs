using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.UnitTest.Framework.PathOperationTests;

namespace Dev2.Integration.Tests.Activities
{
    [TestClass]
    public class FileSystemTests
    {
        private string tmpfile1;
        private string tmpfile2;

        private string tmpdir1;
        private string tmpdir2;
        private string tmpdir3;

        private string uncfile1;
        private string uncfile2;

        private string uncdir1;

        public FileSystemTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        

        #region Additional test attributes

        [TestInitialize]
        public void CreateFiles()
        {
            tmpfile1 = System.IO.Path.GetTempFileName();
            tmpfile2 = System.IO.Path.GetTempFileName();

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

            tmpdir1 = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
            tmpdir2 = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
            tmpdir3 = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();

            Directory.CreateDirectory(tmpdir1);
            Directory.CreateDirectory(tmpdir2);
            Directory.CreateDirectory(tmpdir3);

            // populate directory with data
            string tmpFile = tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();
            tmpFile = tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();

            // create UNC reources
            uncfile1 = TestResource.PathOperations_UNC_Path + Guid.NewGuid();
            uncfile2 = TestResource.PathOperations_UNC_Path + Guid.NewGuid();
            File.WriteAllText(uncfile1, "abc");
            File.WriteAllText(uncfile2, "abc-def");

            uncdir1 = TestResource.PathOperations_UNC_Path + Guid.NewGuid() + "_dir";

            Directory.CreateDirectory(uncdir1);


        }


        private void CreateLocalPath(string path)
        {
            File.Create(path).Close();
        }

        private void DeleteLocalPath(string path)
        {
            File.Delete(path);
        }



        [TestCleanup]
        public void RemoveFiles()
        {
            File.Delete(tmpfile1);
            File.Delete(tmpfile2);

            // remove unc paths
            File.Delete(uncfile1);
            File.Delete(uncfile2);

            try
            {
                Directory.Delete(tmpdir1, true);
                Directory.Delete(tmpdir2, true);
                Directory.Delete(tmpdir3, true);
                Directory.Delete(uncdir1, true);
            }
            catch (Exception) { }
        }

        #endregion

        #region Path Type Tests

        [TestMethod]
        public void IsFileSystemPathType()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path);
            Assert.IsTrue(FileSystemPro is Dev2FileSystemProvider);
        }

        #endregion Path Type Tests

        #region Get Tests

        [TestMethod]
        public void GetWithNoUserName_ValidPath_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path);

            int len = (int)result.Length;
            result.Close();

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithNoUserName_InvalidPath_Expected_NoStream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString("abc");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                Stream result = FileSystemPro.Get(path);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void GetWithNoUserName_UNCValid_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path);

            int len = (int)result.Length;
            result.Close();

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithNoUserName_UNCInvalidPath_Expected_NoStream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(TestResource.PathOperations_UNC_Path + "abc.txt");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                Stream result = FileSystemPro.Get(path);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void GetWithUserName_UNCValidPath_Expected_Stream()
        {
            string uncPath = TestResource.PathOperations_UNC_Path + "Secure\\" + Guid.NewGuid() + ".test";

            PathIOTestingUtils.CreateAuthedUNCPath(uncPath);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncPath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path);

            int len = (int)result.Length;
            result.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithUserName_UNCInvalidPath_Expected_NoStream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(TestResource.PathOperations_UNC_Path + "abc.txt", "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                Stream result = FileSystemPro.Get(path);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }

        [TestMethod]
        public void GetWithInvalidUserName_UNCPath_Expected_NoStream()
        {

            string uncPath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";

            PathIOTestingUtils.CreateAuthedUNCPath(uncPath);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncPath, "DEV2\\" + TestResource.PathOperations_Correct_Username + "abc", TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                Stream result = FileSystemPro.Get(path);

                int len = (int)result.Length;
                result.Close();

                PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);
                Assert.Fail();
            }
            catch (Exception)
            {
                PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);
                Assert.IsTrue(1 == 1);
            }

        }    

        #endregion Get Tests

        #region Put Tests

        [TestMethod]
        public void PutWithOverwriteFalse_File_Not_Present_Expected_NewFileCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = System.IO.Path.GetTempFileName();
            File.Delete(tmp); // remove it is not there
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();
            File.Delete(tmp);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_File_Present_Expect_FileExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();
            File.Delete(tmp);

            Assert.IsTrue(len == -1);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_FileNot_Present_Expect_FileReadAndDataInputToStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();

            File.Delete(tmp);

            Assert.IsTrue(len > 0);
        }


        [TestMethod]
        public void PutWithOverwriteTrue_File_Present_Expect_FileDataReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();

            File.Delete(tmp);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_UNCPath_File_Not_Present_FileDataReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            File.Delete(uncfile1); // remove it is not there
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(uncfile1);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();
            File.Delete(uncfile1);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteFalse_UNCPath_File_Present_Expect_LenNegative()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = System.IO.Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            File.Delete(tmp);

            Assert.IsTrue(len == -1);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_UNCPath_FileNot_Present_Expect_FileReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            File.Delete(tmp);

            Assert.IsTrue(len > 0);
        }


        [TestMethod]
        public void PutWithOverwriteTrue_UNCPath_File_Present_Expected_FileReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = System.IO.Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmpfile2);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src);
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            File.Delete(tmp);

            Assert.IsTrue(len > 0);
        }


        [TestMethod]
        public void PutWithOverwriteFalse_UNCPathValidUser_File_Not_Present_Expected_NoDataFromStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(tmpfile1));
            int len = FileSystemPro.Put(stream, dst, opTO, null);
            stream.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);

            Assert.IsTrue(len > 0);
        }

        //BUILD
        //[TestMethod]
        //public void PutWithOverwriteFalse_UNCPathValidUser_File_Present_Expected_Error() {
        //    Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
        //    string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
        //    string tmp2 = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
        //    PathIOTestingUtils.CreateAuthedUNCPath(tmp);
        //    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
        //    IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp2, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
        //    IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
        //    Stream stream = new MemoryStream(File.ReadAllBytes(tmpfile1));
        //    int len = FileSystemPro.Put(stream, dst, opTO);
        //    stream.Close();

        //    PathIOTestingUtils.DeleteAuthedUNCPath(tmp);
        //    PathIOTestingUtils.DeleteAuthedUNCPath(tmp2);

        //    Assert.IsTrue(len == -1);
        //}


        [TestMethod]
        public void PutWithOverwriteTrue_UNCPathValidUser_FileNot_Present_Expected_NewFileCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(tmpfile1));
            int len = FileSystemPro.Put(stream, dst, opTO, null);
            stream.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_UNCPathValidUser_File_Present_FileInStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            string tmp2 = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp2, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(tmpfile1));
            int len = FileSystemPro.Put(stream, dst, opTO, new FileInfo(src.Path).Directory);
            stream.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);
            PathIOTestingUtils.DeleteAuthedUNCPath(tmp2);

            Assert.IsTrue(len > 0);
        }

        #endregion Put Tests

        #region Delete Tests

        [TestMethod]
        public void DeleteWith_FilePresent_Expected_DeleteSucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteWith_NoFilePresent_Expected_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpfile1);
            File.Delete(tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteWith_DirPresent_Expected_DeleteSuccessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path) as Dev2FileSystemProvider;
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }


        [TestMethod]
        public void DeleteWith_NoDirPresent_Expected_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            Directory.Delete(tmpdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_FilePresent_Expectd_DeleteSucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_NoFilePresent_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncfile1 + "abc");
            File.Delete(tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_DirPresent_Expected_DeleteSuccesful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path) as Dev2FileSystemProvider;
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }


        [TestMethod]
        public void DeleteUNCWith_NoDirPresent_Expected_DeleteUnsuccessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncdir1);
            Directory.Delete(uncdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_FilePresent_Expected_DeleteSuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);
            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_NoFilePresent_Expected_DeleteUnsuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_DirPresent_Expected_DeleteSuccessful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + "_dir";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp, true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }


        [TestMethod]
        public void DeleteUNCValidUserWith_NoDirPresent_DeleteUnsuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + "_dir";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        #endregion Delete Tests

        #region ListDirectory Tests

        [TestMethod]
        public void ListDirectory_With_Contents_Expected_ListAllFiles()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectory_With_NoContents_Expected_NoContentsReturned()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            foreach (IActivityIOPath p in result)
            {
                File.Delete(p.Path);
            }

            result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectory_With_NotExist_Expected_ExceptionThrown()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            Directory.Delete(tmpdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);
                Assert.Fail("Missing Directory Found Data");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }


        [TestMethod]
        public void ListDirectory_Star_With_Contents_ListAllFilesInDir()
        {
            string tmpFileLocal = System.IO.Path.GetTempPath() + "1.testfile";
            string tmpFileLocal2 = System.IO.Path.GetTempPath() + "2.testfile";
            CreateLocalPath(tmpFileLocal);
            CreateLocalPath(tmpFileLocal2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(System.IO.Path.GetTempPath() + "*.testfile");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            DeleteLocalPath(tmpFileLocal);
            DeleteLocalPath(tmpFileLocal2);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectory_Star_With_NoContents_NoFilesReturned()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1 + "\\*.testfile");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectory_StarDotStar_With_Contents_Expected_ListOfDirectoriesAndFiles()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(System.IO.Path.GetTempPath() + "\\*.*");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void ListDirectory_StarDotStar_With_NoContents_Expected_NoFilesInList()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir3 + "\\*.*");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserWith_Contents_ListOfAllFilesInUNCPath()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            string uncFile1 = basePath + "\\" + Guid.NewGuid();
            string uncFile2 = basePath + "\\" + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUSerWith_NoContents_Expected_NoFilesListed()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUser_NotExist_Expected_ExceptionThrown()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);
                Assert.Fail("Missing Directory Found Data");
            }
            catch (Exception)
            {
                Assert.IsTrue(1 == 1);
            }
        }


        [TestMethod]
        public void ListDirectoryUNC_ValidUserStar_With_Contents_Expected_AllDirectoriesListFromUNCPath()
        {

            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            string uncFile1 = basePath + "\\1.testfile";
            string uncFile2 = basePath + "\\2.testfile";
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.testfile", "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserStar_With_NoContents_Expected_EmptyListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.testfile", "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUSerStarDotStar_With_Contents_Expected_DirectoryListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            string uncFile1 = basePath + "\\1.testfile";
            string uncFile2 = basePath + "\\2.testfile";
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.*", "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserStarDotStar_With_NoContents_Expected_EmptyListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.*", "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result.Count == 0);
        }

        #endregion ListDirectory Tests

        #region CreateDirectory Tests

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_NotPresent_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string dir = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(dir);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Directory.Delete(dir);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_Present_Expected_DirectoryNotCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_NotPresent_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string dir = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(dir);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Directory.Delete(dir);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_Present_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmpdir1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUSer_WithOverwriteFalse_NotPresent_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result);
        }


        // Mark To Fix
        //BUILD
        //[TestMethod]
        //public void CreateDirectoryUNCValidUser_WithOverwriteFalse_Present_Expected_DirectoryNotCreated() {
        //    Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
        //    string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
        //    PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
        //    IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
        //    IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
        //    bool result = FileSystemPro.CreateDirectory(path, opTO);

        //    PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

        //    Assert.IsFalse(result);
        //}

        // Mark To Fix

        [TestMethod]
        public void CreateDirectoryUNCValidUser_WithOverwriteTrue_NotPresent_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUser_WithOverwriteTrue_Present_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath);

            Assert.IsTrue(result);
        }

        #endregion CreateDirectory Tests
    }
}
