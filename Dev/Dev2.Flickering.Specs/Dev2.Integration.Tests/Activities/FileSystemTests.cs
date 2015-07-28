
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Text;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        #endregion Get Tests

        #region Put Tests

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
            if (directoryInfo != null)
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

        #endregion Put Tests
    }
}
