
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests
{
    [TestClass]
    public class DbTableTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasSchema_ShouldContainSchemaDotTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable { TableName = "Test", Schema = "dbo" };
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("dbo.Test", fullName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasEmptySchema_ShouldContainTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable { TableName = "Test", Schema = "" };
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", fullName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasNullSchema_ShouldContainTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable { TableName = "Test", Schema = null };
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", fullName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasEmptyTableName_ShouldEmptyString()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable { Schema = "Test", TableName = "" };
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("", fullName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasTableNameNull_ShouldEmptyString()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable { Schema = "Test", TableName = null };
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("", fullName);
        }
    }
}
