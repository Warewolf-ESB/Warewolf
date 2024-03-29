/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
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
        
        public void ActivityIOFactory_CreateOperationEndPointFromIOPath_WithSftp_ShouldSetTypeTpSFtp()

        {
            //------------Setup for test--------------------------
            const string Path = "sftp://sftp.theunlimited.co.za/text.txt";
            //------------Execute Test---------------------------
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(Path, string.Empty, null, true));
            //------------Assert Results-------------------------
            Assert.AreEqual(enActivityIOPathType.SFTP, scrEndPoint.IOPath.PathType);
        }

        [TestMethod]
        public void PutRaw_Should()
        {
            const string newFileName = "tempTextFile";
            var tempPath = Path.GetTempPath() + newFileName + ".txt";
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, null, true, ""));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var raw = activityOperationsBroker.PutRaw(scrEndPoint,
                new Dev2PutRawOperationTO(WriteType.Overwrite, "Some content to write"));
            Assert.AreEqual("Success", raw);
        }

        [TestMethod]
        public void Create_Should()
        {
            var tempPath = Path.GetTempPath() + "SomeName.zip";
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, null, true, ""));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var create = activityOperationsBroker.Create(scrEndPoint, new Dev2CRUDOperationTO(false,false), false);
            Assert.AreEqual("Success", create);
        }
        
        [TestMethod]
        public void GivenExistingFile_PutRawAppendTop_ShouldShouldAppendContent()
        {
            const string newFileName = "tempTextFile";
            var path = Path.GetTempPath() + newFileName + ".txt";
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(
                ActivityIOFactory.CreatePathFromString(path, string.Empty, null, true));
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var raw = activityOperationsBroker.PutRaw(scrEndPoint,
                new Dev2PutRawOperationTO(WriteType.AppendTop, "Some content to write"));
            Assert.AreEqual("Success", raw);
        }
    }
}
