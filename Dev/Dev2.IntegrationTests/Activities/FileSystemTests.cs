
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.DirectoryServices.ActiveDirectory;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Activities
{
    [TestClass]
    public class FileSystemTests
    {
        private string _tmpfile1;
        private string _tmpfile2;

        private string _tmpdir1;
        private string _tmpdir2;
        private string _tmpdir3;

        private string _uncfile1;
        private string _uncfile2;

        private string _uncdir1;

        static bool _inDomain = true;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void CreateFiles()
        {
            _tmpfile1 = Path.GetTempFileName();
            _tmpfile2 = Path.GetTempFileName();

            FileStream fs1 = File.Create(_tmpfile1);
            string tmp = "abc";
            byte[] data = Encoding.UTF8.GetBytes(tmp);
            fs1.Write(data, 0, data.Length);
            fs1.Close();

            FileStream fs2 = File.Create(_tmpfile2);
            tmp = "def-hij";
            data = Encoding.UTF8.GetBytes(tmp);
            fs2.Write(data, 0, data.Length);
            fs2.Close();

            _tmpdir1 = Path.GetTempPath() + Path.GetRandomFileName();
            _tmpdir2 = Path.GetTempPath() + Path.GetRandomFileName();
            _tmpdir3 = Path.GetTempPath() + Path.GetRandomFileName();

            Directory.CreateDirectory(_tmpdir1);
            Directory.CreateDirectory(_tmpdir2);
            Directory.CreateDirectory(_tmpdir3);

            // populate directory with data
            string tmpFile = _tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();
            tmpFile = _tmpdir1 + "\\" + Guid.NewGuid();
            File.Create(tmpFile).Close();

            // create UNC reources
            _uncfile1 = TestResource.PathOperations_UNC_Path + Guid.NewGuid();
            _uncfile2 = TestResource.PathOperations_UNC_Path + Guid.NewGuid();
            File.WriteAllText(_uncfile1, @"abc");
            File.WriteAllText(_uncfile2, @"abc-def");

            _uncdir1 = TestResource.PathOperations_UNC_Path + Guid.NewGuid() + "_dir";

            Directory.CreateDirectory(_uncdir1);


        }


        private void CreateLocalPath(string path)
        {
            File.Create(path).Close();
        }

        private void DeleteLocalPath(string path)
        {
            File.Delete(path);
        }

        private void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        private void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        [TestCleanup]
        public void RemoveFiles()
        {
            File.Delete(_tmpfile1);
            File.Delete(_tmpfile2);

            // remove unc paths
            File.Delete(_uncfile1);
            File.Delete(_uncfile2);

            try
            {
                Directory.Delete(_tmpdir1, true);
                Directory.Delete(_tmpdir2, true);
                Directory.Delete(_tmpdir3, true);
                Directory.Delete(_uncdir1, true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception) { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        [ClassInitialize]
        public static void GetInDomain(TestContext testctx)
        {
            try
            {
                Domain.GetComputerDomain();
            }
            catch(ActiveDirectoryObjectNotFoundException)
            {
                _inDomain = false;
            }
        }

        #endregion

        #region Path Type Tests

        [TestMethod]
        public void IsFileSystemPathType()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpfile1, "", "");
            IActivityIOOperationsEndPoint fileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            fileSystemPro.Get(path, new List<string>());
            Assert.IsTrue(fileSystemPro is Dev2FileSystemProvider);
        }

        #endregion Path Type Tests

        #region Get Tests

        [TestMethod]
        public void GetWithNoUserName_ValidPath_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpfile1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path, new List<string>());

            int len = (int)result.Length;
            result.Close();

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithNoUserName_InvalidPath_Expected_NoStream()
        {
            try
            {
                IActivityIOPath path = ActivityIOFactory.CreatePathFromString("c:\abc", "", "");
                IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
                FileSystemPro.Get(path, new List<string>());
                Assert.Fail();
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void GetWithNoUserName_UNCValid_Expected_Stream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_uncfile1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path, new List<string>());

            int len = (int)result.Length;
            result.Close();

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithNoUserName_UNCInvalidPath_Expected_NoStream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(TestResource.PathOperations_UNC_Path + "abc.txt", "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                FileSystemPro.Get(path, new List<string>());
                Assert.Fail();
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void GetWithUserName_UNCValidPath_Expected_Stream()
        {
            string uncPath = TestResource.PathOperations_UNC_Path + "Secure\\" + Guid.NewGuid() + ".test";

            PathIOTestingUtils.CreateAuthedUNCPath(uncPath, false, _inDomain);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncPath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            Stream result = FileSystemPro.Get(path, new List<string>());

            int len = (int)result.Length;
            result.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void GetWithUserName_UNCInvalidPath_Expected_NoStream()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(TestResource.PathOperations_UNC_Path + "abc.txt", (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                FileSystemPro.Get(path, new List<string>());
                Assert.Fail();
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void GetWithInvalidUserName_UNCPath_Expected_NoStream()
        {

            string uncPath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";

            PathIOTestingUtils.CreateAuthedUNCPath(uncPath, false, _inDomain);

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(uncPath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username + "abc", TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                Stream result = FileSystemPro.Get(path, new List<string>());

                result.Close();

                PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);
                Assert.Fail();
            }
            catch(Exception)
            {
                PathIOTestingUtils.DeleteAuthedUNCPath(uncPath);
                Assert.IsTrue(true);
            }

        }

        #endregion Get Tests

        #region Put Tests

        [TestMethod]
        public void PutWithOverwriteFalse_File_Not_Present_Expected_NewFileCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = Path.GetTempFileName();
            File.Delete(tmp); // remove it is not there
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();
                File.Delete(tmp);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PutWithOverwriteFalse_File_Present_Expect_FileExists()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();
                File.Delete(tmp);

                Assert.IsTrue(len == -1);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PutWithOverwriteTrue_FileNot_Present_Expect_FileReadAndDataInputToStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();

                File.Delete(tmp);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public void PutWithOverwriteTrue_File_Present_Expect_FileDataReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();

                File.Delete(tmp);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PutWithOverwriteFalse_UNCPath_File_Not_Present_FileDataReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            File.Delete(_uncfile1); // remove it is not there
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(_uncfile1, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();
                File.Delete(_uncfile1);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PutWithOverwriteFalse_UNCPath_File_Present_Expect_LenNegative()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = Path.GetTempFileName();
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                File.Delete(tmp);

                Assert.IsTrue(len == -1);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PutWithOverwriteTrue_UNCPath_FileNot_Present_Expect_FileReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                File.Delete(tmp);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public void PutWithOverwriteTrue_UNCPath_File_Present_Expected_FileReadIntoStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, "", "");
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(_tmpfile2, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = FileSystemPro.Get(src, new List<string>());
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                File.Delete(tmp);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public void PutWithOverwriteFalse_UNCPathValidUser_File_Not_Present_Expected_NoDataFromStream()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(_tmpfile1));
            int len = FileSystemPro.Put(stream, dst, opTO, null, new List<string>());
            stream.Close();

            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);

            Assert.IsTrue(len > 0);
        }

        [TestMethod]
        public void PutWithOverwriteTrue_UNCPathValidUser_FileNot_Present_Expected_NewFileCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(_tmpfile1));
            int len = FileSystemPro.Put(stream, dst, opTO, null, new List<string>());
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
            PathIOTestingUtils.CreateAuthedUNCPath(tmp, false, _inDomain);
            IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOPath src = ActivityIOFactory.CreatePathFromString(tmp2, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
            Stream stream = new MemoryStream(File.ReadAllBytes(_tmpfile1));
            var directoryInfo = new FileInfo(src.Path).Directory;
            if(directoryInfo != null)
            {
                int len = FileSystemPro.Put(stream, dst, opTO, directoryInfo.ToString(), new List<string>());
                stream.Close();

                PathIOTestingUtils.DeleteAuthedUNCPath(tmp);
                PathIOTestingUtils.DeleteAuthedUNCPath(tmp2);

                Assert.IsTrue(len > 0);
            }
            else
            {
                Assert.Fail();
            }
        }

        #endregion Put Tests

        #region Delete Tests

        [TestMethod]
        public void DeleteWith_FilePresent_Expected_DeleteSucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpfile1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteWith_NoFilePresent_Expected_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpfile1, "", "");
            File.Delete(_tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteWith_DirPresent_Expected_DeleteSuccessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path) as Dev2FileSystemProvider;
            bool ok = FileSystemPro != null && FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }


        [TestMethod]
        public void DeleteWith_NoDirPresent_Expected_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            Directory.Delete(_tmpdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_FilePresent_Expectd_DeleteSucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_uncfile1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_NoFilePresent_DeleteUnsucessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_uncfile1 + "abc", "", "");
            File.Delete(_tmpfile1);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_DirPresent_Expected_DeleteSuccesful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_uncdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path) as Dev2FileSystemProvider;
            bool ok = FileSystemPro != null && FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteUNCWith_NoDirPresent_Expected_DeleteUnsuccessful()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_uncdir1, "", "");
            Directory.Delete(_uncdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_FilePresent_Expected_DeleteSuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp, false, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);
            PathIOTestingUtils.DeleteAuthedUNCPath(tmp);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_NoFilePresent_Expected_DeleteUnsuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + ".test";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void DeleteUNCValidUserWith_DirPresent_Expected_DeleteSuccessful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + "_dir";
            PathIOTestingUtils.CreateAuthedUNCPath(tmp, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsTrue(ok);
        }


        [TestMethod]
        public void DeleteUNCValidUserWith_NoDirPresent_DeleteUnsuccesful()
        {
            string tmp = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid() + "_dir";
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(tmp, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.Delete(path);

            Assert.IsFalse(ok);
        }

        #endregion Delete Tests

        #region ListDirectory Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2FileSystemProvider_ListFoldersInDirectory")]
        public void Dev2FileSystemProvider_ListFoldersInDirectory_Normal_AllFoldersInDirectoryReturned()
        {
            //------------Setup for test--------------------------
            var dev2FileSystemProvider = new Dev2FileSystemProvider();
            //string baseFolderDirectory = Path.GetTempPath() + @"\ListDirectoryTestFolder";
            string tmpFolderLocal = Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned\Folder1";
            string tmpFolderLocal2 = Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned\Folder2";
            string tmpFileLocal1 = Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned\File1.txt";
            string tmpFileLocal2 = Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned\File2.txt";
            CreateDirectory(tmpFolderLocal);
            CreateDirectory(tmpFolderLocal2);
            CreateLocalPath(tmpFileLocal1);
            CreateLocalPath(tmpFileLocal2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned", "", "");

            //------------Execute Test---------------------------

            IList<IActivityIOPath> folderList = dev2FileSystemProvider.ListFoldersInDirectory(path);


            //------------Assert Results-------------------------

            Assert.AreEqual(2, folderList.Count);
            Assert.IsTrue(folderList[0].Path.EndsWith("Folder1"));
            Assert.IsTrue(folderList[1].Path.EndsWith("Folder2"));

            DeleteDirectory(Path.GetTempPath() + @"\ListDirectoryTestFolderAllFoldersInDirectoryReturned");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2FileSystemProvider_ListFilesInDirectory")]
        public void Dev2FileSystemProvider_ListFilesInDirectory_Normal_AllFilesInDirectoryReturned()
        {
            //------------Setup for test--------------------------
            var dev2FileSystemProvider = new Dev2FileSystemProvider();
            //string baseFolderDirectory = Path.GetTempPath() + @"\ListDirectoryTestFolder";
            string tmpFolderLocal = Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned\Folder1";
            string tmpFolderLocal2 = Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned\Folder2";
            string tmpFileLocal1 = Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned\File1.txt";
            string tmpFileLocal2 = Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned\File2.txt";
            CreateDirectory(tmpFolderLocal);
            CreateDirectory(tmpFolderLocal2);
            CreateLocalPath(tmpFileLocal1);
            CreateLocalPath(tmpFileLocal2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned", "", "");

            //------------Execute Test---------------------------

            IList<IActivityIOPath> fileList = dev2FileSystemProvider.ListFilesInDirectory(path);


            //------------Assert Results-------------------------

            Assert.AreEqual(2, fileList.Count);
            Assert.IsTrue(fileList[0].Path.EndsWith("File1.txt"));
            Assert.IsTrue(fileList[1].Path.EndsWith("File2.txt"));

            DeleteDirectory(Path.GetTempPath() + @"\ListDirectoryTestFolderNormal_AllFilesInDirectoryReturned");
        }

        [TestMethod]
        public void ListDirectory_With_Contents_Expected_ListAllFiles()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectory_With_NoContents_Expected_NoContentsReturned()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            foreach(IActivityIOPath p in result)
            {
                File.Delete(p.Path);
            }

            result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectory_With_NotExist_Expected_ExceptionThrown()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            Directory.Delete(_tmpdir1, true);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                FileSystemPro.ListDirectory(path);
                Assert.Fail("Missing Directory Found Data");
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void ListDirectory_Star_With_Contents_ListAllFilesInDir()
        {
            string tmpFileLocal = Path.GetTempPath() + "1.testfile";
            string tmpFileLocal2 = Path.GetTempPath() + "2.testfile";
            CreateLocalPath(tmpFileLocal);
            CreateLocalPath(tmpFileLocal2);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(Path.GetTempPath() + "*.testfile", "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            DeleteLocalPath(tmpFileLocal);
            DeleteLocalPath(tmpFileLocal2);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectory_Star_With_NoContents_NoFilesReturned()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1 + "\\*.testfile", "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectory_StarDotStar_With_Contents_Expected_ListOfDirectoriesAndFiles()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(Path.GetTempPath() + "\\*.*", "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void ListDirectory_StarDotStar_With_NoContents_Expected_NoFilesInList()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir3 + "\\*.*", "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserWith_Contents_ListOfAllFilesInUNCPath()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            string uncFile1 = basePath + "\\" + Guid.NewGuid();
            string uncFile2 = basePath + "\\" + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1, false, _inDomain);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2, false, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUSerWith_NoContents_Expected_NoFilesListed()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUser_NotExist_Expected_ExceptionThrown()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            try
            {
                FileSystemPro.ListDirectory(path);
                Assert.Fail("Missing Directory Found Data");
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void ListDirectoryUNC_ValidUserStar_With_Contents_Expected_AllDirectoriesListFromUNCPath()
        {

            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            string uncFile1 = basePath + "\\1.testfile";
            string uncFile2 = basePath + "\\2.testfile";
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1, false, _inDomain);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2, false, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.testfile", (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserStar_With_NoContents_Expected_EmptyListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.testfile", (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUSerStarDotStar_With_Contents_Expected_DirectoryListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            string uncFile1 = basePath + "\\1.testfile";
            string uncFile2 = basePath + "\\2.testfile";
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile1, false, _inDomain);
            PathIOTestingUtils.CreateAuthedUNCPath(uncFile2, false, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.*", (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ListDirectoryUNC_ValidUserStarDotStar_With_NoContents_Expected_EmptyListReturned()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath + "\\*.*", (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> result = FileSystemPro.ListDirectory(path);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result.Count == 0);
        }

        #endregion ListDirectory Tests

        #region CreateDirectory Tests

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_NotPresent_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string dir = Path.GetTempPath() + Path.GetRandomFileName();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(dir, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Directory.Delete(dir);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteFalse_Present_Expected_DirectoryNotCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_NotPresent_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string dir = Path.GetTempPath() + Path.GetRandomFileName();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(dir, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Directory.Delete(dir);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryWithOverwriteTrue_Present_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(_tmpdir1, "", "");
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool ok = FileSystemPro.CreateDirectory(path, opTO);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUSer_WithOverwriteFalse_NotPresent_Expected_DirectoryCreated()
        {
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUser_WithOverwriteFalse_Present_Expected_DirectoryNotCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(false);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUser_WithOverwriteTrue_NotPresent_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateDirectoryUNCValidUser_WithOverwriteTrue_Present_Expected_DirectoryCreated()
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(true);
            string basePath = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            PathIOTestingUtils.CreateAuthedUNCPath(basePath, true, _inDomain);
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(basePath, (_inDomain ? "DEV2\\" : ".\\") + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FileSystemPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            bool result = FileSystemPro.CreateDirectory(path, opTO);

            PathIOTestingUtils.DeleteAuthedUNCPath(basePath, _inDomain);

            Assert.IsTrue(result);
        }

        #endregion CreateDirectory Tests
    }
}
