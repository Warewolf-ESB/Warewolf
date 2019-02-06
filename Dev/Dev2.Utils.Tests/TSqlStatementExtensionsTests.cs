/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQL;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class TSqlStatementExtensionsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TSqlStatementExtensions))]
        public void TSqlStatementExtensions_GetAllTables()
        {
            var statements = TSQLStatementReader.ParseStatements("Select * from Person");
            var tables = statements[0].GetAllTables();
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual("Person", tables[0].TableName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TSqlStatementExtensions))]
        public void TSqlStatementExtensions_GetAllTables_SkipToken()
        {
            var statements = TSQLStatementReader.ParseStatements("Select person.name as firstname from Person p");
            var tables = statements[0].GetAllTables();
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual("Person", tables[0].TableName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TSqlStatementExtensions))]
        public void TSqlStatementExtensions_GetAllTables_Offset()
        {
            var statements = TSQLStatementReader.ParseStatements("SELECT Name, ProductNumber, StandardCost FROM Production.Product ORDER BY StandardCost OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY");
            var tables = statements[0].GetAllTables();
            Assert.AreEqual(2, tables.Count);
            Assert.AreEqual("Production", tables[0].TableName);
            Assert.AreEqual("Product", tables[1].TableName);
        }
       
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TSqlStatementExtensions))]
        public void TSqlStatementExtensions_GetAllTables_TSqlTable_Constructor()
        {
            var tables = new TSqlTable("Person");
            Assert.AreEqual("Person", tables.TableName);
        }
    }
}
