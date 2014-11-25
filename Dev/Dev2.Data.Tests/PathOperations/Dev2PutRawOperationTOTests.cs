
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
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once InconsistentNaming
namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            //------------Execute Test---------------------------
            var dev2PutRawOperation = new Dev2PutRawOperationTO(WriteType, Contents);
            //------------Assert Results-------------------------
            Assert.AreEqual(WriteType,dev2PutRawOperation.WriteType);
            Assert.AreEqual(Contents,dev2PutRawOperation.FileContents);
        }
    }
}
