/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Wrappers
{
    [TestClass]
    public class FilePathWrapperTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_GetFileName_AreEqual_ToLastStringAfterDevider_ExpectTrue()
        {
            //------------------------Arrange------------------------
            var filePathWrapper = new FilePathWrapper();
            //------------------------Act----------------------------
            var fileName = filePathWrapper.GetFileName(@"O:\Test\testFileName.txt");
            //------------------------Assert-------------------------
            Assert.AreEqual("testFileName.txt", fileName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_Combine_AreEqual_ToArrayOfString_ExpectTrue()
        {
            //------------------------Arrange------------------------
            var paths = new[] { @"O:\TestPath1", "TestPath2", "testFileName.txt" };

            var filePathWrapper = new FilePathWrapper();
            //------------------------Act----------------------------
            var combinedTestPaths = filePathWrapper.Combine(paths);
            //------------------------Assert-------------------------
            Assert.AreEqual(@"O:\TestPath1\TestPath2\testFileName.txt", combinedTestPaths);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_IsPathRooted_HasRoot_LocalDir_ExpectTrue()
        {
            //------------------------Arrange------------------------
            var path = @"O:\TestPath1\TestPath2\testFileName.txt";

            var filePathWrapper = new FilePathWrapper();
            //------------------------Act----------------------------
            var isPathRooted = filePathWrapper.IsPathRooted(path);
            //------------------------Assert-------------------------
            Assert.IsTrue(isPathRooted);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_IsPathRooted_HasRoot_UncPath_ExpectTrue()
        {
            //------------------------Arrange------------------------
            var UncPath = @"\\myPc\mydir\myfile";

            var filePathWrapper = new FilePathWrapper();
            //------------------------Act----------------------------
            var isPathRooted = filePathWrapper.IsPathRooted(UncPath);
            //------------------------Assert-------------------------
            Assert.IsTrue(isPathRooted);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_IsPathRooted_HasNoRoot_relativePath_ExpectFalse()
        {
            //------------------------Arrange------------------------
            var relativePath = @"mydir\sudir\";

            var filePathWrapper = new FilePathWrapper();
            //------------------------Act----------------------------
            var isPathRooted = filePathWrapper.IsPathRooted(relativePath);
            //------------------------Assert-------------------------
            Assert.IsFalse(isPathRooted);
        }
    }
}
