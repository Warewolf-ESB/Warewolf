
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BinaryDataListItemTest
    {
        [TestMethod]
        public void FetchFromList_Expected_NonNullDisplayName()
        {
            string error = string.Empty;

            IBinaryDataList dl1;

            dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("field1"));

            dl1.TryCreateRecordsetTemplate("recset", "", cols, true, out error);

            dl1.TryCreateRecordsetValue("value", "field1", "recset", 1, out error);

            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // DataList Cleanup ;)

            Assert.AreEqual("recset(1).field1", entry.FetchRecordAt(1, out error).First().DisplayValue);
            Assert.AreEqual(error, "");
        }
    }
}
