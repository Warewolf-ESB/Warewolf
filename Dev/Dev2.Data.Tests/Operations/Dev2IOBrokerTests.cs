/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
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
            if (tempFileName != null)
            {
                var tempFile = Path.Combine(tempPath, innerDir, innerDir, tempFileName);
                string directoryName = Path.GetDirectoryName(tempFile);
                if (directoryName != null)
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
                var newFilePath = Path.Combine(dst, tempFileName);
                Assert.IsTrue(File.Exists(tempFile));

                File.Delete(tempFile);
                File.Delete(newFilePath);
            }
        }
        [TestMethod]
        public void PutRaw_Should()
        {
            const string newFileName = "tempTextFile";
            var tempPath = Path.GetTempPath() + newFileName + ".txt";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, null, true, ""));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var raw = activityOperationsBroker.PutRaw(scrEndPoint,
                new Dev2PutRawOperationTO(WriteType.Overwrite, "Some content to write"));
            Assert.AreEqual("Success", raw);
        }

        [TestMethod]
        public void Create_Should()
        {
            var tempPath = Path.GetTempPath() + "SomeName.zip";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, null, true, ""));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var create = activityOperationsBroker.Create(scrEndPoint, new Dev2CRUDOperationTO(false,false), false);
            Assert.AreEqual("Success", create);
        }
        
        [TestMethod]
        public void GivenExistingFile_PutRawAppendTop_ShouldShouldAppendContent()
        {
            const string newFileName = "tempTextFile";
            var path = Path.GetTempPath() + newFileName + ".txt";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(
                ActivityIOFactory.CreatePathFromString(path, string.Empty, null, true));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var raw = activityOperationsBroker.PutRaw(scrEndPoint,
                new Dev2PutRawOperationTO(WriteType.AppendTop, "Some content to write"));
            Assert.AreEqual("Success", raw);
        }        
    }
}
