using System.Diagnostics.CodeAnalysis;
using Dev2.Data.PathOperations;
using Dev2.PathOperations;
using Dev2.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unlimited.UnitTest.Framework.PathOperationTests
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
        public void CreatePathFromString_Expected_IActivityIOPath_FileSystem_Type()
        {

            IActivityIOPath result = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FileSystem_Path, "", "");

            Assert.AreEqual(enActivityIOPathType.FileSystem, result.PathType);
        }

        /// <summary>
        /// Create type of the path from string expected activity IO path FTP.
        /// </summary>
        [TestMethod]
        public void CreatePathFromString_Expected_IActivityIOPath_FTP_Type()
        {

            IActivityIOPath result = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Path, "", "");

            Assert.AreEqual(enActivityIOPathType.FTP, result.PathType);
        }

        /// <summary>
        /// Create type of the path from string expected activity IO path FTPS.
        /// </summary>
        [TestMethod]
        public void CreatePathFromString_Expected_IActivityIOPath_FTPS_Type()
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
        public void CreateOperationEndPointFromIOPath_Expected_IActivityIOOperationsEndPoint_FileSysytemProvider_Type()
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
        public void CreateOperationEndPointFromIOPath_Expected_IActivityIOOperationsEndPoint_FTPProvider_Type()
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
        public void CreateOperationsBroker_Expected_IActivityOperationsBroker_FileSysytemProvider_Type()
        {

            IActivityOperationsBroker result = ActivityIOFactory.CreateOperationsBroker();

            Assert.IsTrue(result != null);
        }

        #endregion Create Operation Broker Tests
    }
}
