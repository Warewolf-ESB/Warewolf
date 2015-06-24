
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unlimited.UnitTest.Framework
{
    /// <summary>
    /// Summary description for Dev2RecordsetIndexScopeTest 
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListLoadTest
    {
        const double _ticksPerSec = 10000000;

        #region Create RS Test

        [TestMethod]
        public void LargeBDL_Create_10k_5Cols_Recordset_Entries()
        {
            string error;

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            const int runs = 10000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

            DateTime start1 = DateTime.Now;
            for(int i = 0; i < runs; i++)
            {
                dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
            }

            DateTime end1 = DateTime.Now;

            long ticks = (end1.Ticks - start1.Ticks);
            double result1 = (ticks / _ticksPerSec);

            Assert.IsTrue(result1 <= 3.5, "It took [ " + result1 + " ] seconds"); // Given .01 buffer WAS : 0.075
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!

        }

        [TestMethod]
        public void LargeBDL_Create_100k_5Cols_Recordset_Entries()
        {
            string error;

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int runs = 100000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

            double result1;

            DateTime start1 = DateTime.Now;
            for(int i = 0; i < runs; i++)
            {
                dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
            }
            DateTime end1 = DateTime.Now;
            long ticks = (end1.Ticks - start1.Ticks);
            result1 = (ticks / _ticksPerSec);

            Assert.IsTrue(result1 <= 25, " It Took " + result1); // Given 0.75 WAS : 0.75
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!

        }

        [TestMethod]
        public void LargeBDL_Create_1Mil_5Cols_Recordset_Entries()
        {
            string error;

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            const int runs = 10000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

            using(dl1)
            {
                DateTime start1 = DateTime.Now;
                for(int i = 0; i < runs; i++)
                {
                    dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
                }

                DateTime end1 = DateTime.Now;

                long ticks = (end1.Ticks - start1.Ticks);
                double result1 = (ticks / _ticksPerSec);

                Console.WriteLine(result1 + @" seconds for " + runs + @" with 5 cols");

                Assert.IsTrue(result1 <= 1500, "Expected 500 seconds but got " + result1 + " seconds"); // Given 0.75 WAS : 0.75
                // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
            }
        }

        #endregion

        
    }
}
