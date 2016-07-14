/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Util;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var file = new Mock<IFile>();
            file.Setup(file1 => file1.Delete(It.IsAny<string>()));

            var innerDir = Guid.NewGuid().ToString();
            var tempPath = Path.GetTempPath();
            var tempFileName = Path.GetFileName(Path.GetTempFileName());
            const string TempData = "some string data";
            var tempFile = Path.Combine(tempPath, innerDir, innerDir, tempFileName);
            string directoryName = Path.GetDirectoryName(tempFile);
            if(directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
            }
            var upperLevelDir = Path.Combine(tempPath, innerDir);
            File.WriteAllText(tempFile, TempData);
            var dst = Path.Combine(tempPath, Guid.NewGuid().ToString());
            var srcPath = ActivityIOFactory.CreatePathFromString(upperLevelDir, string.Empty, null, true);
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(srcPath);
            var dstPath = ActivityIOFactory.CreatePathFromString(dst, string.Empty, null, true);
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dstPath);

            var moveTO = new Dev2CRUDOperationTO(true);
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker(file.Object, new CommonDataUtils());
            activityOperationsBroker.Copy(scrEndPoint, dstEndPoint, moveTO);
            var newFilePath = Path.Combine(dst, innerDir, tempFileName);
            Assert.IsTrue(File.Exists(newFilePath));
            Assert.IsTrue(File.Exists(tempFile));

            File.Delete(tempFileName);
        }
    }
}
