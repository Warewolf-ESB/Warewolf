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
using System.IO;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests
{
    [TestClass]
    public class DeleteHelperTest
    {

        #region Private Helpers

        static void Cleanup(string dir)
        {
            try
            {
                Directory.Delete(dir, true);
            }

            catch { }

        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenNullPath_ExpectFalse()
        {
            var mockFile = new Mock<IFile>();
            var mockDir = new Mock<IDirectory>();
            var result = new DeleteHelper(mockFile.Object, mockDir.Object).Delete(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenPathContainsStar_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var tmpPath = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(tmpPath);
            File.Create(tmpPath + "\\a.txt").Close();
            Directory.CreateDirectory(tmpPath + "\\b");

            //------------Execute Test---------------------------
            var result = new DeleteHelper().Delete(tmpPath + "\\*.*");

            var dirStillExit = Directory.Exists(tmpPath);
            var contents = Directory.GetFiles(tmpPath);

            Cleanup(tmpPath);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.IsTrue(dirStillExit);
            Assert.AreEqual(0, contents.Length);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenPathContainsJustFolder_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var tmpPath = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(tmpPath);
            File.Create(tmpPath + "\\a.txt").Close();
            Directory.CreateDirectory(tmpPath + "\\b");

            //------------Execute Test---------------------------
            var result = new DeleteHelper().Delete(tmpPath + "\\b");

            var dirStillExit = Directory.Exists(tmpPath);
            var contents = Directory.GetFiles(tmpPath);
            var bDirStillExit = Directory.Exists(tmpPath + "\\b");

            Cleanup(tmpPath);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.IsTrue(dirStillExit);
            Assert.AreEqual(1, contents.Length);
            Assert.IsFalse(bDirStillExit);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenPathContainsJustFile_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var tmpPath = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(tmpPath);
            File.Create(tmpPath + "\\a.txt").Close();
            Directory.CreateDirectory(tmpPath + "\\b");

            //------------Execute Test---------------------------
            var result = new DeleteHelper().Delete(tmpPath + "\\a.txt");

            var dirStillExit = Directory.Exists(tmpPath);
            var contents = Directory.GetFiles(tmpPath);
            var bDirStillExit = Directory.Exists(tmpPath + "\\b");

            Cleanup(tmpPath);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.IsTrue(dirStillExit);
            Assert.AreEqual(0, contents.Length);
            Assert.IsTrue(bDirStillExit);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenPathContainsJustFileWithStar_ExpectTrue()
        {
            var mockFile = new Mock<IFile>();
            var mockDir = new Mock<IDirectory>();
            //------------Setup for test--------------------------
            var tmpPath = Path.GetTempPath() + Guid.NewGuid();

            mockDir.Setup(o => o.GetFileSystemEntries(It.IsAny<string>(), It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(new string[] {
                @"c:\somedir\some.txt",
                @"c:\somedir\somedir",
                @"c:\somedir\some1.txt",
                @"c:\somedir\some2.txt"
            });
            mockFile.Setup(o => o.GetAttributes(@"c:\somedir\some.txt")).Returns(FileAttributes.Normal);
            mockFile.Setup(o => o.GetAttributes(@"c:\somedir\somedir")).Returns(FileAttributes.Directory);
            mockFile.Setup(o => o.GetAttributes(@"c:\somedir\some1.txt")).Returns(FileAttributes.Normal);
            mockFile.Setup(o => o.GetAttributes(@"c:\somedir\some2.txt")).Returns(FileAttributes.Normal);

            //------------Execute Test---------------------------
            var result = new DeleteHelper(mockFile.Object, mockDir.Object).Delete(tmpPath + "\\a.*");

            Cleanup(tmpPath);


            mockFile.Verify(o => o.Delete(@"c:\somedir\some.txt"), Times.Once);
            mockDir.Verify(o => o.Delete(@"c:\somedir\somedir", true), Times.Once);
            mockFile.Verify(o => o.Delete(@"c:\somedir\some1.txt"), Times.Once);
            mockFile.Verify(o => o.Delete(@"c:\somedir\some2.txt"), Times.Once);

            mockDir.VerifyAll();
            mockFile.VerifyAll();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }
}
