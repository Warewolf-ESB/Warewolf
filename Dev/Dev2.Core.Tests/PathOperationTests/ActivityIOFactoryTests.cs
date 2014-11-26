
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Data.PathOperations;
using Dev2.PathOperations;
using Dev2.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Unlimited.UnitTest.Framework.PathOperationTests
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ActivityIOFactoryTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Create Path From String Tests

        /// <summary>
        /// Create type of the path from string expected activity IO path file system.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreatePathFromString_Expected_IActivityIOPath_FileSystem_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityIOPath result = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FileSystem_Path, "", "");

            Assert.AreEqual(enActivityIOPathType.FileSystem, result.PathType);
        }

        /// <summary>
        /// Create type of the path from string expected activity IO path FTP.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreatePathFromString_Expected_IActivityIOPath_FTP_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityIOPath result = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Path, "", "");

            Assert.AreEqual(enActivityIOPathType.FTP, result.PathType);
        }

        /// <summary>
        /// Create type of the path from string expected activity IO path FTP.
        /// </summary>

        [TestMethod,ExpectedException(typeof(IOException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_CreateOperationEndPointFromIOPath")]
// ReSharper disable InconsistentNaming
        public void CreatePathFromString_NonRootedPath_ExpectException()
// ReSharper restore InconsistentNaming
        {
            ActivityIOFactory.CreatePathFromString("", "", "");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_CreateOperationEndPointFromIOPath")]
        // ReSharper disable InconsistentNaming
        public void CreatePathFromString_NonRootedPath_ExpectArgumentNullException()
        // ReSharper restore InconsistentNaming
        {
            ActivityIOFactory.CreatePathFromString(null, "", "");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_CreateOperationEndPointFromIOPath")]
        // ReSharper disable InconsistentNaming
        public void CreatePathFromString_AssertAllPropertiesAreSet_ExpectCorrectValues()
        // ReSharper restore InconsistentNaming
        {
            var path = ActivityIOFactory.CreatePathFromString(@"c:\bob","dave","monkey", false);
            Assert.AreEqual(@"c:\bob",path.Path);
            Assert.AreEqual(@"dave", path.Username);
            Assert.AreEqual(@"monkey", path.Password);
            Assert.IsFalse( path.IsNotCertVerifiable);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_CreateUnzipTO")]
        // ReSharper disable InconsistentNaming
        public void CreatePathFromString_CreateUnzipTO_AssertCorrectType()
        // ReSharper restore InconsistentNaming
        {
            var operationTO = ActivityIOFactory.CreateZipTO("20", "ss", "name", true);
            Assert.AreEqual("20", operationTO.CompressionRatio);
            Assert.AreEqual("ss", operationTO.ArchivePassword);
            Assert.AreEqual("name", operationTO.ArchiveName);
            Assert.IsTrue(operationTO.Overwrite);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_CreateUnzipTO")]
        // ReSharper disable InconsistentNaming
        public void CreatePathFromString_CreateUnzipTO_PWD_OverWrite_AssertCorrectType()
        // ReSharper restore InconsistentNaming
        {
            var operationTO = ActivityIOFactory.CreateUnzipTO("20", true);
            Assert.AreEqual("20", operationTO.ArchivePassword);
            Assert.IsTrue(operationTO.Overwrite);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityIOFactory_Dev2PutRawOperationTO")]
        // ReSharper disable InconsistentNaming
        public void CreatePathFromString_CreatePathFromString_AssertCorrectType()
        // ReSharper restore InconsistentNaming
        {
            var operationTO = ActivityIOFactory.CreatePathFromString(@"c:\moon",true);
            Assert.AreEqual(@"c:\moon", operationTO.Path);
            //Assert.AreEqual(, operationTO.);
        }



        /// <summary>
        /// Create type of the path from string expected activity IO path FTPS.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreatePathFromString_Expected_IActivityIOPath_FTPS_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityIOPath result = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTPS_AuthPath, "", "");

            Assert.AreEqual(enActivityIOPathType.FTPS, result.PathType);
        }

        #endregion Create Path From String Tests

        #region Create Operation EndPoint From IO Path Tests

        /// <summary>
        /// Create type of the operation end point from IO path expected activity IO operations end point file sysytem 
        /// provider.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreateOperationEndPointFromIOPath_Expected_IActivityIOOperationsEndPoint_FileSysytemProvider_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FileSystem_Path, "", "");
            IActivityIOOperationsEndPoint result = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            Assert.IsTrue(result.GetType() == typeof(Dev2FileSystemProvider));
        }

        /// <summary>
        /// Create type of the operation end point from IO path expected activity IO operations 
        /// end point FTP provider.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreateOperationEndPointFromIOPath_Expected_IActivityIOOperationsEndPoint_FTPProvider_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Path, "", "");
            IActivityIOOperationsEndPoint result = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            Assert.IsTrue(result.GetType() == typeof(Dev2FTPProvider));
        }

        #endregion Create Operation EndPoint From IO Path Tests

        #region Create Operation Broker Tests

        /// <summary>
        /// Create type of the operations broker expected activity operations broker file sysytem provider.
        /// </summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void CreateOperationsBroker_Expected_IActivityOperationsBroker_FileSysytemProvider_Type()
// ReSharper restore InconsistentNaming
        {

            IActivityOperationsBroker result = ActivityIOFactory.CreateOperationsBroker();

            Assert.IsTrue(result != null);
        }

        #endregion Create Operation Broker Tests


    }
}
