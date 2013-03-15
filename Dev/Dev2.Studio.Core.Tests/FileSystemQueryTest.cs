using System;
using System.Collections.Generic;
using System.Threading;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [Ignore] // This is for testing against the actual fileSystem which may vary
    public class FileSystemQueryTest
    {

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(DataListSingletonTest.DataListSingletonTestGuard);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTest.DataListSingletonTestGuard);
        }

        #endregion Test Initialization
        
        [TestMethod]
        public void QueryListWhereNothingPassedExpectListOfDrives()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList("");
            //------------Assert Results-------------------------
            Assert.AreEqual(8, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereDrivePassedExpectFoldersAndFilesOnDrive()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList("C:");
            //------------Assert Results-------------------------
            Assert.AreEqual(31, fileSystemQuery.QueryCollection.Count);
        }       
        
        [TestMethod]
        public void QueryListWhereDriveAndFolderPassedNoSlashExpectFolder()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users");
            //------------Assert Results-------------------------
            Assert.AreEqual(9, fileSystemQuery.QueryCollection.Count);
        }
        
        [TestMethod]
        public void QueryListWhereDriveAndFolderWithStartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users\des");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereDriveAndFolderWithPartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"C:\Users\skt");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereNoNetworkExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\");
            //------------Assert Results-------------------------
            Assert.AreEqual(40, fileSystemQuery.QueryCollection.Count);
        } 
        
        [TestMethod]
        public void QueryListWhereNetworkPathExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\");
            //------------Assert Results-------------------------
            Assert.AreEqual(6, fileSystemQuery.QueryCollection.Count);
        }

        [TestMethod]
        public void QueryListWhereNetworkPathHasFilesExpectFolderWithFilesNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff");
            //------------Assert Results-------------------------
            Assert.AreEqual(16, fileSystemQuery.QueryCollection.Count);
        }        
                
        
        [TestMethod]
        public void QueryListWhereNetworkPathHasFolderExpectFolderInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\_Arch");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }        
        
        [TestMethod]
        public void QueryListWhereNetworkPathHasFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\LoadTest");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        } 
        
        [TestMethod]
        public void QueryListWhereNetworkPathHasMiddleOfFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var fileSystemQuery = new FileSystemQuery();
            //------------Execute Test---------------------------
            fileSystemQuery.QueryList(@"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Runt");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, fileSystemQuery.QueryCollection.Count);
        }

    }

}