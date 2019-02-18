/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class PerformListOfIOPathOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_Path_Null_ExpectNullReferenceException()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            //-----------------------Act----------------------------
            //-----------------------Assert-------------------------
            Assert.ThrowsException<NullReferenceException>(()=> PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_With_CTOR_Path_Null_ExpectNullReferenceException()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            var performListOfIOPathOperation = new TestPerformListOfIOPathOperation((arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-----------------------Act----------------------------
            //-----------------------Assert-------------------------
            Assert.ThrowsException<NullReferenceException>(() => PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_Path_IsNotNull_ExpectNullReferenceException()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = "TestPath";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            //-----------------------Act----------------------------
            var appendBackSlashes = PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object);
            //-----------------------Assert-------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(path+"\\", appendBackSlashes);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_Path_IsNotDirectory_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger.log";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            //-----------------------Act----------------------------
            var appendBackSlashes = PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object);
            //-----------------------Assert-------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(path, appendBackSlashes);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_Path_IsDirectory_DirectoryExist_And_IsNotStarWildCard_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger.log";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockFileWrapper.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);
            mockFileWrapper.Setup(o => o.GetAttributes(It.IsAny<string>())).Returns(FileAttributes.Directory);
            //-----------------------Act----------------------------
            var appendBackSlashes = PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object);
            //-----------------------Assert-------------------------
            mockActivityIOPath.VerifyAll();
            mockFileWrapper.VerifyAll();
            Assert.AreEqual(path+"\\", appendBackSlashes);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AppendBackSlashes_Path_IsDirectory_DirectoryExist_And_IsNotStarWildCard_EndsWithBackSlash_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger.log\\";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            //-----------------------Act----------------------------
            var appendBackSlashes = PerformListOfIOPathOperation.AppendBackSlashes(mockActivityIOPath.Object, mockFileWrapper.Object, mockDirectory.Object);
            //-----------------------Assert-------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(path, appendBackSlashes);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AddDirsToResults_DirsToAdd_NoDirs_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var enumerableString = new List<string>();
            //-----------------------Act----------------------------
            var appendBackSlashes = PerformListOfIOPathOperation.AddDirsToResults(enumerableString, mockActivityIOPath.Object);
            //-----------------------Assert-------------------------
            Assert.AreEqual(0, appendBackSlashes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AddDirsToResults_DirsToAdd_WithInValidDirs_ExpectIOException()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var enumerableString = new List<string>();
            enumerableString.Add("testDir1");
            enumerableString.Add("testDir2");
            //-----------------------Act----------------------------
            //-----------------------Assert-------------------------
            Assert.ThrowsException<IOException>(() => PerformListOfIOPathOperation.AddDirsToResults(enumerableString, mockActivityIOPath.Object));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_AddDirsToResults_DirsToAdd_WithValidDirs_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var enumerableString = new List<string>();
            enumerableString.Add("ftp://testParth/logger1.log");
            enumerableString.Add("c://testParth/logger2.log");
            //-----------------------Act----------------------------
            var addDirsToResults = PerformListOfIOPathOperation.AddDirsToResults(enumerableString, mockActivityIOPath.Object);
            //-----------------------Assert-------------------------
            Assert.AreEqual(2,addDirsToResults.Count);
            Assert.AreEqual("ftp://testParth/logger1.log", addDirsToResults[0].Path);
            Assert.AreEqual("c://testParth/logger2.log", addDirsToResults[1].Path);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNullOrEmpty_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            var pattern = "testPattern";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, pattern, ReadTypes.Files, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNotNullOrEmpty_And_ReadTypesFolders_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            var pattern = "testPattern";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, pattern, ReadTypes.Folders, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNotNullOrEmpty_And_ReadTypesFilesAndFolders_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            var pattern = "testPattern";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, pattern, ReadTypes.FilesAndFolders, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNullOrEmpty_And_ReadTypesFiles_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, string.Empty, ReadTypes.Files, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNullOrEmpty_And_ReadTypesFolders_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, string.Empty, ReadTypes.Folders, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformListOfIOPathOperation))]
        public void PerformListOfIOPathOperation_GetDirectoriesForType_Pattern_IsNullOrEmpty_And_ReadTypesFilesAndFolders_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            var mockDirectory = new Mock<IDirectory>();

            var path = "ftp://testParth/logger1.log";
            //-----------------------Act----------------------------
            var getDirectoriesForType = PerformListOfIOPathOperation.GetDirectoriesForType(path, string.Empty, ReadTypes.FilesAndFolders, mockDirectory.Object);
            //-----------------------Assert-------------------------
            Assert.IsNotNull(getDirectoriesForType);
        }

        class TestPerformListOfIOPathOperation : PerformListOfIOPathOperation
        {
            public TestPerformListOfIOPathOperation(ImpersonationDelegate impersonationDelegate) : base(impersonationDelegate)
            {
            }

            public override IList<IActivityIOPath> ExecuteOperation()
            {
                throw new NotImplementedException();
            }

            public override IList<IActivityIOPath> ExecuteOperationWithAuth()
            {
                throw new NotImplementedException();
            }
        }
    }
}
