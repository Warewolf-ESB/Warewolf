using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2IOBrokerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ActivityIOFactory_CreateOperationEndPointFromIOPath")]
        // ReSharper disable InconsistentNaming
        public void ActivityIOFactory_CreateOperationEndPointFromIOPath_WithSftp_ShouldSetTypeTpSFtp()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string Path = "sftp://sftp.theunlimited.co.za/text.txt";
            //------------Execute Test---------------------------
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(Path, string.Empty, null, true));
            //------------Assert Results-------------------------
            Assert.AreEqual(enActivityIOPathType.SFTP, scrEndPoint.IOPath.PathType);
        }

        [TestMethod]
        public void CopyFileWithPathsExpectedRecursiveCopy()
        {
            var innerDir = Guid.NewGuid().ToString();
            var tempPath = Path.GetTempPath();
            var tempFileName = Path.GetFileName(Path.GetTempFileName());
            const string TempData = "some string data";
            if(tempFileName != null)
            {
                var tempFile = Path.Combine(tempPath, innerDir, innerDir, tempFileName);
                string directoryName = Path.GetDirectoryName(tempFile);
                if(directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                }
                var upperLevelDir = Path.Combine(tempPath, innerDir);
                File.WriteAllText(tempFile, TempData);
                var dst = Path.Combine(tempPath, Guid.NewGuid().ToString());
                var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(upperLevelDir, string.Empty, null, true));
                var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(dst, string.Empty, null, true));

                var moveTO = new Dev2CRUDOperationTO(true);
                ActivityIOFactory.CreateOperationsBroker().Copy(scrEndPoint, dstEndPoint, moveTO);
                var newFilePath = Path.Combine(dst, innerDir, tempFileName);
                Assert.IsTrue(File.Exists(newFilePath));
                Assert.IsTrue(File.Exists(tempFile));
            }
        }

    }
}