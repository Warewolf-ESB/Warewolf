
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
using System.IO;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class DeleteHelperTest
    {

        #region Private Helpers

        private static void Cleanup(string dir)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeleteHelper_Delete")]
        public void DeleteHelper_Delete_WhenNullPath_ExpectFalse()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var result = DeleteHelper.Delete(null);

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
            var result = DeleteHelper.Delete(tmpPath + "\\*.*");

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
            var result = DeleteHelper.Delete(tmpPath + "\\b");

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
            var result = DeleteHelper.Delete(tmpPath + "\\a.txt");

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
            //------------Setup for test--------------------------
            var tmpPath = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(tmpPath);
            File.Create(tmpPath + "\\a.txt").Close();
            Directory.CreateDirectory(tmpPath + "\\b");

            //------------Execute Test---------------------------
            var result = DeleteHelper.Delete(tmpPath + "\\a.*");

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
    }
}
