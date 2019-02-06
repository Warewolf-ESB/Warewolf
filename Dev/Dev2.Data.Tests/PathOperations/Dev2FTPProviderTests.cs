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
            var mockImplementation = new Mock<IImplementation>();
            var dev2FTPProvider = new Dev2FTPProvider(mockImplementation.Object);

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
    }
}
