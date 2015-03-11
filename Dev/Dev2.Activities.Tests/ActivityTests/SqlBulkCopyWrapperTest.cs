
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Data;
using Dev2.Activities.SqlBulkInsert;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SqlBulkCopyWrapperTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkCopyWrapper_WriteToServer")]
        [ExpectedException(typeof(ArgumentException))]
        // ReSharper disable InconsistentNaming
        public void SqlBulkCopyWrapper_WriteToServer_WhenNullBulkCopyObject_ExpectException()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var sqlBulkCopyWrapper = new SqlBulkCopyWrapper(null);
            DataTable dataTable = new DataTable("myTable");

            //------------Execute Test---------------------------
            sqlBulkCopyWrapper.WriteToServer(dataTable);

        }
    }
}
