/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.PathOperations;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2PutRawOperationTOTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2PutRawOperationTO_Constructor")]
        public void Dev2PutRawOperation_Constructor_TakesContentsAndWriteTypeEnum()
        {
            //------------Setup for test--------------------------
            const WriteType WriteType = WriteType.AppendBottom;
            const string Contents = "Some test";
            //------------Execute Test---------------------------
            var dev2PutRawOperation = new Dev2PutRawOperationTO(WriteType,Contents);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dev2PutRawOperation);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2PutRawOperationTO_Constructor")]
        public void Dev2PutRawOperationTO_Constructor_GivenParameter_ShouldSetProperties()
        {
            //------------Setup for test--------------------------
            const WriteType WriteType = WriteType.AppendBottom;
            const string Contents = "Some test";
            var ContentsAsBase64 = false;
            //------------Execute Test---------------------------
            var dev2PutRawOperation = new Dev2PutRawOperationTO(WriteType, Contents);
            //------------Assert Results-------------------------
            Assert.AreEqual(WriteType,dev2PutRawOperation.WriteType);
            Assert.AreEqual(Contents,dev2PutRawOperation.FileContents);
            Assert.AreEqual(ContentsAsBase64,dev2PutRawOperation.FileContentsAsBase64);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2PutRawOperationTO))]
        public void Dev2PutRawOperationTO_Constructor_GivenParameter_FileContentsAsBase64_ShouldSetProperties()
        {
            //------------Setup for test--------------------------
            const WriteType WriteType = WriteType.AppendBottom;
            const string Contents = "Some test";
            var ContentsAsBase64 = true;
            //------------Execute Test---------------------------
            var dev2PutRawOperation = new Dev2PutRawOperationTO(WriteType, Contents, ContentsAsBase64);
            //------------Assert Results-------------------------
            Assert.AreEqual(WriteType, dev2PutRawOperation.WriteType);
            Assert.AreEqual(Contents, dev2PutRawOperation.FileContents);
            Assert.AreEqual(ContentsAsBase64, dev2PutRawOperation.FileContentsAsBase64);
        }
    }
}
