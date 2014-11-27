
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Data;
using Dev2.Activities.SqlBulkInsert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SqlBulkInserterTests
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInserter_Constructor")]
        public void SqlBulkInserter_Constructor_WhenCreatingNew_ExpectValidObject()
        {
            //------------Setup for test--------------------------
            var bulkInserter = new SqlBulkInserter();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsNotNull(bulkInserter);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInserter_Insert")]
        public void SqlBulkInserter_Insert_WhenInsertingTableData_ExpectInsertSuccess()
        {
            //------------Setup for test--------------------------
            var bulkInserter = new SqlBulkInserter();
            Mock<ISqlBulkCopy> bulkCopy = new Mock<ISqlBulkCopy>();
            bulkCopy.Setup(b => b.WriteToServer(It.IsAny<DataTable>())).Returns(true);
            DataTable dt = new DataTable("myTable");

            //------------Execute Test---------------------------
            var result = bulkInserter.Insert(bulkCopy.Object, dt);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInserter_Insert")]
        public void SqlBulkInserter_Insert_WhenInsertingTableData_ExpectInsertFailure()
        {
            //------------Setup for test--------------------------
            var bulkInserter = new SqlBulkInserter();
            Mock<ISqlBulkCopy> bulkCopy = new Mock<ISqlBulkCopy>();
            bulkCopy.Setup(b => b.WriteToServer(It.IsAny<DataTable>())).Returns(false);
            DataTable dt = new DataTable("myTable");

            //------------Execute Test---------------------------
            var result = bulkInserter.Insert(bulkCopy.Object, dt);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInserter_Insert")]
        public void SqlBulkInserter_Insert_WhenInsertingNullTableData_ExpectInsertFailure()
        {
            //------------Setup for test--------------------------
            var bulkInserter = new SqlBulkInserter();
            Mock<ISqlBulkCopy> bulkCopy = new Mock<ISqlBulkCopy>();
            bulkCopy.Setup(b => b.WriteToServer(It.IsAny<DataTable>())).Returns(false);

            //------------Execute Test---------------------------
            var result = bulkInserter.Insert(bulkCopy.Object, null);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }





        // ReSharper restore InconsistentNaming
    }
}
