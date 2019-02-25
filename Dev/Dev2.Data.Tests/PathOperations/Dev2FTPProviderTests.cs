/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using Dev2.Data.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using static Dev2.Data.PathOperations.Dev2FTPProvider;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2FTPProviderTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Validate_Defaults()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockImplementation = new Mock<IImplementation>();
            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object)
            {
                IOPath = mockActivityIOPath.Object
            };

            Assert.AreEqual(mockActivityIOPath.Object, dev2FTPProvider.IOPath);
            Assert.AreEqual(@"/", dev2FTPProvider.PathSeperator());
            Assert.IsTrue(dev2FTPProvider.RequiresLocalTmpStorage());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Get_IsStandardFtp_ReadFromFtp()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FTP);

            Stream streamResult = new MemoryStream(new byte[0]);
            Stream streamNullResult = null;
            var filesToCleanup = new List<string>();

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.ReadFromFtp(mockActivityIOPath.Object, ref streamResult));

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);

            var stream = dev2FTPProvider.Get(mockActivityIOPath.Object, filesToCleanup);

            Assert.AreEqual(null, stream);
            mockImplementation.Verify(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.ReadFromFtp(mockActivityIOPath.Object, ref streamNullResult), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Get_ExpectedException()
        {
            const string path = "path";
            var filesToCleanup = new List<string>();
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);

            var dev2FTPProvider = new Dev2FTPProvider(null);

            try
            {
                dev2FTPProvider.Get(mockActivityIOPath.Object, filesToCleanup);
                Assert.Fail("Code should have caused an exception to be thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Object reference not set to an instance of an object. ,  [path]", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Get_IsStandardFtp_ReadFromSftp()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            Stream streamResult = new MemoryStream(new byte[0]);
            Stream streamNullResult = null;
            var filesToCleanup = new List<string>();

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.ReadFromSftp(mockActivityIOPath.Object, ref streamResult, filesToCleanup));

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);

            var stream = dev2FTPProvider.Get(mockActivityIOPath.Object, filesToCleanup);

            Assert.AreEqual(null, stream);
            mockImplementation.Verify(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.ReadFromSftp(mockActivityIOPath.Object, ref streamNullResult, filesToCleanup), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Put_OverWrite_WriteToFtp()
        {
            Stream streamResult = new MemoryStream(new byte[0]);
            var filesToCleanup = new List<string>();
            var whereToPut = string.Empty;

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(true);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.WriteToFtp(streamResult, mockActivityIOPath.Object)).Returns(1);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.Put(streamResult, mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, whereToPut, filesToCleanup);

            Assert.AreEqual(1, result);
            mockImplementation.Verify(implementation => implementation.WriteToFtp(streamResult, mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Put_ExpectedException_using()
        {
            Stream streamResult = new MemoryStream(new byte[0]);
            var filesToCleanup = new List<string>();
            var whereToPut = string.Empty;

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(false);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.WriteToFtp(streamResult, mockActivityIOPath.Object)).Returns(1);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);

            var result = dev2FTPProvider.Put(streamResult, null, mockDev2CRUDOperationTO.Object, whereToPut, filesToCleanup);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Put_ExpectedException_All()
        {
            Stream streamResult = new MemoryStream(new byte[0]);
            var filesToCleanup = new List<string>();
            var whereToPut = string.Empty;

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(false);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.WriteToFtp(streamResult, mockActivityIOPath.Object)).Returns(1);

            var dev2FTPProvider = new Dev2FTPProvider(null);

            try
            {
                dev2FTPProvider.Put(streamResult, mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, whereToPut, filesToCleanup);
                Assert.Fail("Code should have caused an exception to be thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Object reference not set to an instance of an object.", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Put_OverWrite_WriteToSftp()
        {
            Stream streamResult = new MemoryStream(new byte[0]);
            var filesToCleanup = new List<string>();
            var whereToPut = string.Empty;

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(true);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.WriteToSftp(streamResult, mockActivityIOPath.Object)).Returns(1);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.Put(streamResult, mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, whereToPut, filesToCleanup);

            Assert.AreEqual(1, result);
            mockImplementation.Verify(implementation => implementation.WriteToSftp(streamResult, mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Put_No_OverWrite_IsStandardFtp()
        {
            Stream streamResult = new MemoryStream(new byte[0]);
            var filesToCleanup = new List<string>();
            var whereToPut = string.Empty;

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(false);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.Put(streamResult, mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, whereToPut, filesToCleanup);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Delete_PathType_Directory_DeleteHandler()
        {
            const string path = "path";
            const string userName = "userName";
            const string password = "password";
            const string privateKeyFile = "privateKeyFile";
            var pathStack = new List<string> { path };

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Password).Returns(password);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PrivateKeyFile).Returns(privateKeyFile);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.PathIs(mockActivityIOPath.Object)).Returns(Interfaces.Enums.enPathType.Directory);
            mockImplementation.Setup(implementation => implementation.DeleteHandler(pathStack, userName, password, privateKeyFile));

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.Delete(mockActivityIOPath.Object);

            Assert.IsTrue(result);
            mockImplementation.Verify(implementation => implementation.DeleteHandler(pathStack, userName, password, privateKeyFile), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Delete_ExpectedException()
        {
            const string path = "path";
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);

            var dev2FTPProvider = new Dev2FTPProvider(null);

            try
            {
                dev2FTPProvider.Delete(mockActivityIOPath.Object);
                Assert.Fail("Code should have caused an exception to be thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Object reference not set to an instance of an object.", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_Delete_PathType_File_DeleteOp()
        {
            const string path = ".path";
            const string userName = "userName";
            const string password = "password";
            const string privateKeyFile = "privateKeyFile";

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FileSystem);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Password).Returns(password);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PrivateKeyFile).Returns(privateKeyFile);

            var activityIOPathList = new List<IActivityIOPath> { mockActivityIOPath.Object };

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.PathIs(mockActivityIOPath.Object)).Returns(Interfaces.Enums.enPathType.File);
            mockImplementation.Setup(implementation => implementation.DeleteOp(activityIOPathList));

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.Delete(mockActivityIOPath.Object);

            Assert.IsTrue(result);
            mockImplementation.Verify(implementation => implementation.DeleteOp(activityIOPathList), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_ListDirectory_ListDirectoryStandardFtp()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FTP);

            var activityIOPathList = new List<IActivityIOPath> { mockActivityIOPath.Object };

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.ListDirectoryStandardFtp(mockActivityIOPath.Object)).Returns(activityIOPathList);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var listDirectory = dev2FTPProvider.ListDirectory(mockActivityIOPath.Object);

            Assert.AreEqual(activityIOPathList, listDirectory);
            Assert.AreEqual(1, listDirectory.Count);
            mockImplementation.Verify(implementation => implementation.ListDirectoryStandardFtp(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_ListDirectory_ListDirectorySftp()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.Invalid);

            var activityIOPathList = new List<IActivityIOPath> { mockActivityIOPath.Object };

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.ListDirectorySftp(mockActivityIOPath.Object)).Returns(activityIOPathList);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var listDirectory = dev2FTPProvider.ListDirectory(mockActivityIOPath.Object);

            Assert.AreEqual(activityIOPathList, listDirectory);
            Assert.AreEqual(1, listDirectory.Count);
            mockImplementation.Verify(implementation => implementation.ListDirectorySftp(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_CreateDirectory_Overwrite()
        {
            const string path = "path";
            const string userName = "userName";
            const string password = "password";
            const string privateKeyFile = "privateKeyFile";
            var pathStack = new List<string> { path };

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FTP);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Password).Returns(password);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PrivateKeyFile).Returns(privateKeyFile);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(true);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.PathIs(mockActivityIOPath.Object)).Returns(Interfaces.Enums.enPathType.Directory);
            mockImplementation.Setup(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object)).Returns(true);
            mockImplementation.Setup(implementation => implementation.DeleteHandler(pathStack, userName, password, privateKeyFile));
            mockImplementation.Setup(implementation => implementation.CreateDirectoryStandardFtp(mockActivityIOPath.Object)).Returns(true);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.CreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object);

            Assert.IsTrue(result);

            mockImplementation.Verify(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.DeleteHandler(pathStack, userName, password, privateKeyFile), Times.Once);
            mockImplementation.Verify(implementation => implementation.CreateDirectoryStandardFtp(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_CreateDirectory_Not_Overwrite()
        {
            const string path = "path";
            const string userName = "userName";
            const string password = "password";
            const string privateKeyFile = "privateKeyFile";

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FTP);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Password).Returns(password);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PrivateKeyFile).Returns(privateKeyFile);

            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(dev2CRUDOperationTO => dev2CRUDOperationTO.Overwrite).Returns(false);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.CreateDirectorySftp(mockActivityIOPath.Object)).Returns(true);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var result = dev2FTPProvider.CreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object);

            Assert.IsTrue(result);

            mockImplementation.Verify(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.CreateDirectorySftp(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_PathExist_Directory()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.PathIs(mockActivityIOPath.Object)).Returns(Interfaces.Enums.enPathType.Directory);
            mockImplementation.Setup(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object)).Returns(true);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var pathExists = dev2FTPProvider.PathExist(mockActivityIOPath.Object);

            Assert.IsTrue(pathExists);
            mockImplementation.Verify(implementation => implementation.PathIs(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.IsDirectoryAlreadyPresent(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_PathExist_File()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.PathIs(mockActivityIOPath.Object)).Returns(Interfaces.Enums.enPathType.File);
            mockImplementation.Setup(implementation => implementation.IsFilePresent(mockActivityIOPath.Object)).Returns(true);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var pathExists = dev2FTPProvider.PathExist(mockActivityIOPath.Object);

            Assert.IsTrue(pathExists);
            mockImplementation.Verify(implementation => implementation.PathIs(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.IsFilePresent(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_HandlesType()
        {
            var mockImplementation = new Mock<IImplementation>();

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);

            Assert.IsFalse(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.FileSystem));
            Assert.IsFalse(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.Invalid));
            Assert.IsTrue(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.FTPS));
            Assert.IsTrue(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.SFTP));
            Assert.IsTrue(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.FTP));
            Assert.IsTrue(dev2FTPProvider.HandlesType(Interfaces.Enums.enActivityIOPathType.FTPES));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_ListFoldersInDirectory()
        {
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var activityIOPathList = new List<IActivityIOPath> { mockActivityIOPath.Object };

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.IsStandardFtp(mockActivityIOPath.Object)).Returns(false);
            mockImplementation.Setup(implementation => implementation.ListFoldersInDirectory(mockActivityIOPath.Object)).Returns(activityIOPathList);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var foldersInDirectory = dev2FTPProvider.ListFoldersInDirectory(mockActivityIOPath.Object);

            Assert.AreEqual(activityIOPathList, foldersInDirectory);
            Assert.AreEqual(1, foldersInDirectory.Count);
            mockImplementation.Verify(implementation => implementation.ListFoldersInDirectory(mockActivityIOPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_ListFilesInDirectory()
        {
            const string path = "path";
            const string userName = "userName";
            const string password = "password";
            const bool isNotCertVerifiable = false;
            const bool enableSsl = false;
            const string privateKeyFile = "privateKeyFile";
            const string tmpDirData = "tmpDirData";
            const string path1 = "\\test";
            const string path2 = "\\testnew";
            var extractList = new List<string> { path1, path2 };

            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PathType).Returns(Interfaces.Enums.enActivityIOPathType.FTP);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Password).Returns(password);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.IsNotCertVerifiable).Returns(isNotCertVerifiable);
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.PrivateKeyFile).Returns(privateKeyFile);

            var mockImplementation = new Mock<IImplementation>();
            mockImplementation.Setup(implementation => implementation.EnableSsl(mockActivityIOPath.Object)).Returns(enableSsl);
            mockImplementation.Setup(implementation => implementation.ExtendedDirList(path, userName, password, enableSsl, isNotCertVerifiable, privateKeyFile)).Returns(tmpDirData);
            mockImplementation.Setup(implementation => implementation.ExtractList(tmpDirData, It.IsAny<Func<string, bool>>()))
                .Callback<string, Func<string, bool>>((string payload, Func<string, bool> matchFunc) => {
                    Assert.IsFalse(matchFunc("<dir>"));
                    Assert.IsTrue(matchFunc("ftp:\\testfile.txt"));
                    matchFunc(payload);
                })
                .Returns(extractList);
            mockImplementation.Setup(implementation => implementation.BuildValidPathForFtp(mockActivityIOPath.Object, path1)).Returns(path1);
            mockImplementation.Setup(implementation => implementation.BuildValidPathForFtp(mockActivityIOPath.Object, path2)).Returns(path2);

            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);
            var foldersInDirectory = dev2FTPProvider.ListFilesInDirectory(mockActivityIOPath.Object);

            Assert.AreEqual(2, foldersInDirectory.Count);

            Assert.IsFalse(foldersInDirectory[0].IsNotCertVerifiable);
            Assert.AreEqual(password, foldersInDirectory[0].Password);
            Assert.AreEqual(path1, foldersInDirectory[0].Path);
            Assert.AreEqual(Interfaces.Enums.enActivityIOPathType.FileSystem, foldersInDirectory[0].PathType);
            Assert.AreEqual(privateKeyFile, foldersInDirectory[0].PrivateKeyFile);
            Assert.AreEqual(userName, foldersInDirectory[0].Username);

            Assert.IsFalse(foldersInDirectory[1].IsNotCertVerifiable);
            Assert.AreEqual(password, foldersInDirectory[1].Password);
            Assert.AreEqual(path2, foldersInDirectory[1].Path);
            Assert.AreEqual(Interfaces.Enums.enActivityIOPathType.FileSystem, foldersInDirectory[1].PathType);
            Assert.AreEqual(privateKeyFile, foldersInDirectory[1].PrivateKeyFile);
            Assert.AreEqual(userName, foldersInDirectory[1].Username);

            mockImplementation.Verify(implementation => implementation.EnableSsl(mockActivityIOPath.Object), Times.Once);
            mockImplementation.Verify(implementation => implementation.ExtendedDirList(path, userName, password, enableSsl, isNotCertVerifiable, privateKeyFile), Times.Once);
            mockImplementation.Verify(implementation => implementation.ExtractList(tmpDirData, It.IsAny<Func<string, bool>>()), Times.Once);
            mockImplementation.Verify(implementation => implementation.BuildValidPathForFtp(mockActivityIOPath.Object, path1), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2FTPProvider))]
        public void Dev2FTPProvider_ListFilesInDirectory_ExpectedException()
        {
            const string path = "path";
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Path).Returns(path);

            var dev2FTPProvider = new Dev2FTPProvider(null);

            try
            {
                dev2FTPProvider.ListFilesInDirectory(mockActivityIOPath.Object);
                Assert.Fail("Code should have caused an exception to be thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Object reference not set to an instance of an object. : [path]", ex.Message);
            }
        }
    }
}
