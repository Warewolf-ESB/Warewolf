
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.BinaryDataList
{
    /// <summary>
    /// Test for the BinaryDataListEntry ;)
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BinaryDataListEntryTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_MakeRecordsetEvaluateReady")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchRecordsetIndexes_WhenAliases_ExpectAliasKeys()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // dl1 - Child
            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl0.UID, "f1", "recset", "f1", out errors);

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            var idxes = entry.FetchRecordsetIndexes();
            var minIdx = idxes.MinIndex();
            var maxIdx = idxes.MaxIndex();
            var gapsCount = idxes.FetchGaps().Count;

            Assert.AreEqual(3, idxes.Count);
            Assert.AreEqual(0, gapsCount);
            Assert.AreEqual(1, minIdx);
            Assert.AreEqual(3, maxIdx);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_WhenGaps_ExpectSortedResults()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("c", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("b", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("a", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", false, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "a");
            Assert.AreEqual(row1[1].TheValue, "3");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "b");
            Assert.AreEqual(row3[1].TheValue, "2");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "c");
            Assert.AreEqual(row4[1].TheValue, "1");
            // adjust the alias mapping data ;)


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Foward_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("c", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("a", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", false, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            DoNullVariableAssertion(row1[0]);
            Assert.AreEqual(row1[1].TheValue, "2");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "a");
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "c");
            Assert.AreEqual(row4[1].TheValue, "1");
            // adjust the alias mapping data ;)


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Backward_Int_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("30", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("12", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", true, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "30");
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "12");
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            DoNullVariableAssertion(row4[0]);
            Assert.AreEqual(row4[1].TheValue, "2");
            // adjust the alias mapping data ;)


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Backward_float_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("30.1", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("12.4", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", true, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "30.1");
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "12.4");
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            DoNullVariableAssertion(row4[0]);
            Assert.AreEqual(row4[1].TheValue, "2");
            // adjust the alias mapping data ;)


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Backward_floatWithEmptyVals_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("30.1", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("12.4", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", true, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "30.1");
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "12.4");
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "");
            Assert.AreEqual(row4[1].TheValue, "2");
            // adjust the alias mapping data ;)


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Backward_Date_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue(new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue(new DateTime(1999, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", true, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, new DateTime(1999, 01, 01).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            DoNullVariableAssertion(row4[0]);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_Backward_DateWithEmptyVals_WhenGapsAndMissingEntries_ExpectSortedResultsWithBlanksAtStart()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue(new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue(new DateTime(1999, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.Sort("f1", true, out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, new DateTime(1999, 01, 01).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(row3[1].TheValue, "3");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "");
            Assert.AreEqual(row4[1].TheValue, "2");
            // adjust the alias mapping data ;)


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_BlankRecordSetData")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_BlankRecordSetData_Data_ExpectBlank()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue(new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("sdsd", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue(new DateTime(1999, 01, 01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.BlankRecordSetData("f1");
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "");
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            var binaryDataListItem = row2[0];
            DoNullVariableAssertion(binaryDataListItem);
            binaryDataListItem = row2[1];
            DoNullVariableAssertion(binaryDataListItem);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "");
            Assert.AreEqual(row3[1].TheValue, "2");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "");
            Assert.AreEqual(row4[1].TheValue, "3");
        }

        static void DoNullVariableAssertion(IBinaryDataListItem binaryDataListItem)
        {
            try
            {
                var val = binaryDataListItem.TheValue;
                Assert.IsNull(val);
            }
            catch(Exception e)
            {
                StringAssert.Contains(e.Message, string.Format("No Value assigned for: [[{0}]]", binaryDataListItem.DisplayValue));
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_BlankRecordSetData")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_BlankRecordSetData_InvalidColumn_Data_ExpectBlank()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("a", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("sdsd", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("b", "f1", "recset", 4, out error);
            dl0.TryCreateRecordsetValue("3", "f2", "recset", 4, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.BlankRecordSetData("fx");
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue, "a");
            Assert.AreEqual(row1[1].TheValue, "1");
            var row2 = entry.FetchRecordAt(2, out error);
            DoNullVariableAssertion(row2[0]);
            DoNullVariableAssertion(row2[1]);
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "sdsd");
            Assert.AreEqual(row3[1].TheValue, "2");
            var row4 = entry.FetchRecordAt(4, out error);
            Assert.AreEqual(row4[0].TheValue, "b");
            Assert.AreEqual(row4[1].TheValue, "3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_BlankRecordSetData")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_BlankRecordSetData_Empty_Data_ExpectBlank()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.BlankRecordSetData("fx");
            var row1 = entry.FetchRecordAt(1, out error);
            DoNullVariableAssertion(row1[0]);
            DoNullVariableAssertion(row1[1]);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_DeleteAllRows")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_DeleteAllRows_Empty_Data_ExpectError()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            var row1 = entry.FetchRecordAt(1, out error);
            DoNullVariableAssertion(row1[0]);
            DoNullVariableAssertion(row1[1]);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_DeleteAllRows")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_DeleteAllRows_HasValues_ExpectEmptyDS()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            var row = entry.FetchRecordAt(1, out error);
            Assert.AreEqual("r1.f1.value", row[0].TheValue);
            Assert.AreEqual("r1.f2.value", row[1].TheValue);
            entry.TryDeleteRows("*", out error);

            row = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row[0].TheValue, "");
            Assert.AreEqual(row[1].TheValue, "");
            row = entry.FetchRecordAt(2, out error);
            Assert.AreEqual(row[0].TheValue, "");
            Assert.AreEqual(row[1].TheValue, "");
            row = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row[0].TheValue, "");
            Assert.AreEqual(row[1].TheValue, "");
            row = entry.FetchRecordAt(4, out error);
            DoNullVariableAssertion(row[0]);
            DoNullVariableAssertion(row[1]);
            Assert.AreEqual(String.Empty, error);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasField_HasField_ExpectTrue()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            Assert.IsTrue(entry.HasField("f1"));
            Assert.IsTrue(entry.HasField("f2"));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasField_TryFetchRecordSet_ExpectTrue()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);


            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            entry.TryFetchRecordsetColumnAtIndex("bob", 3, out error);
            Assert.AreEqual("Index [ 3 ] is out of bounds", error);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasField_NoField_ExpectFalse()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            Assert.IsFalse(entry.HasField("f3"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasField_IsScalar_ExpectFalse()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("moo.bob", out entry, out error);
            Assert.IsFalse(entry.HasField("f3"));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasColumns_IsScalar_ExpectFalse()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();



            // dl0 - Parent
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("moo.bob", out entry, out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            Assert.IsFalse(entry.HasColumns(cols));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasColumns_NullPassedIn_ExpectFalse()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();



            // dl0 - Parent
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("moo.bob", out entry, out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            Assert.IsFalse(entry.HasColumns(null));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasColumns_IsRecordSet_AllColumnsExist_ExpectTrue()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();


            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);


            Assert.IsTrue(entry.HasColumns(cols));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_AdjustAliasOperationForExternalServicePopulate")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_AdjustAliasOperationForExternalServicePopulate_IsRecordSet_ExpectInternalNotSetToEmpty()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();


            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            var internalobj = new PrivateObject(entry).GetField("_internalObj") is SBinaryDataListEntry ? (SBinaryDataListEntry)new PrivateObject(entry).GetField("_internalObj") : new SBinaryDataListEntry();
            Assert.IsFalse(internalobj.IsEmtpy);
            entry.AdjustAliasOperationForExternalServicePopulate();
            Assert.IsFalse(internalobj.IsEmtpy);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasColumns_IsRecordSet_AllColumnsDoNotExist_ExpectFalse()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();


            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            IList<Dev2Column> cols2 = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            Assert.IsTrue(entry.HasColumns(cols2));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_FetchLastRecordsetIndex")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchLastRecordsetIndex_WhenAliases_ExpectMaxIndexOfAlias()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error); // fetch this from alias

            // dl1 - Child
            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl0.UID, "f1", "recset", "f1", out errors);

            var ent = entry.FetchLastRecordsetIndex();

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual(ent, 3);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_FetchLastRecordsetIndex")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchLastRecordsetIndex_AssertInternalPassThrough()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error); // fetch this from alias



            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            var internalSbc = new PrivateObject(entry, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") is SBinaryDataListEntry ? (SBinaryDataListEntry)new PrivateObject(entry, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") : new SBinaryDataListEntry();

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual(entry.ColumnIODirection, internalSbc.ColumnIODirection);
            Assert.AreEqual(entry.IsEditable, internalSbc.IsEditable);
            Assert.AreEqual(entry.IsEvaluationScalar, internalSbc.IsEvaluationScalar);

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_FetchAppendRecordsetIndex")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchAppendRecordsetIndex_WhenAliases_InternalNotMinus1_ExpectAppendValue()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error); // fetch this from alias

            // dl1 - Child
            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl0.UID, "f1", "recset", "f1", out errors);

            var ent = entry.FetchAppendRecordsetIndex();

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual(ent, 4);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_FetchAppendRecordsetIndex")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchAppendRecordsetIndex_WhenAliases_InternalMinus1_ExpectAppendValue()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);


            // dl1 - Child
            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl0.UID, "f1", "recset", "f1", out errors);

            var ent = entry.FetchAppendRecordsetIndex();

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual(1, ent);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_FetchScalar")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchScalar_IsRecordSet_ExpectLastIndex()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl1.UID, "f1", "recset", "f1", out errors);

            var ent = entry.FetchScalar();

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual("r2.f1.value", ent.TheValue);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_TryFetchIndexedRecordsetUpsertPayload")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_TryFetchIndexedRecordsetUpsertPayload_IsRecordSet_ExpectLastIndex()
        // ReSharper restore InconsistentNaming
        {

            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl1.UID, "f1", "recset", "f1", out errors);

            var ent = entry.TryFetchIndexedRecordsetUpsertPayload(2, out error);

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            Assert.AreEqual("r2.f1.value", ent.TheValue);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_HasField")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_HasColumns_IsRecordSet_EmptyInput_Expecttrue()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();


            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            IList<Dev2Column> cols2 = new List<Dev2Column>();
            Assert.IsTrue(entry.HasColumns(cols2));

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListEntry_Distinct")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Distinct_ExpectDistinctIndexes()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();


            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);

            var distinctRows = entry.GetDistinctRows(new List<string> { "f1" });
            Assert.IsNotNull(distinctRows);
            Assert.AreEqual(2, distinctRows.Count);
            Assert.AreEqual(1, distinctRows[0]);
            Assert.AreEqual(2, distinctRows[1]);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Merge")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Merge_NonMatchingColumn_ExpectEmptyDS()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            dl0.TryCreateRecordsetValue("r1.f1.value", "f3", "recset2", 1, out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            IBinaryDataListEntry entry2;
            dl0.TryGetEntry("recset", out entry, out error);
            dl0.TryGetEntry("recset2", out entry2, out error);
            entry.Merge(entry2, out error);
            Assert.AreEqual("Mapping error: Column not found f3", error);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Merge")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Merge_MatchingColumns_ExpectAppendedToend()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));

            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetTemplate("recset2", "a recordset", colsDiff, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);


            dl0.TryCreateRecordsetValue("r2.f1.value", "f1", "recset2", 1, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            IBinaryDataListEntry entry2;
            dl0.TryGetEntry("recset", out entry, out error);
            dl0.TryGetEntry("recset2", out entry2, out error);
            entry.Merge(entry2, out error);


            var row = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row[0].TheValue, "r1.f1.value");
            row = entry.FetchRecordAt(2, out error);
            Assert.AreEqual(row[0].TheValue, "r2.f1.value");


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Merge")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Merge_Scalar_ExpectEmptyDS()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            IList<Dev2Column> colsDiff = new List<Dev2Column>();
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            colsDiff.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            IBinaryDataListEntry entry2;
            dl0.TryGetEntry("recset", out entry, out error);
            dl0.TryGetEntry("moo.bob", out entry2, out error);
            entry.Merge(entry2, out error);
            Assert.AreEqual("Type mis-match, one side is Recordset while the other is a scalar", error);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Sort")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Sort_ExpectNoErrorIfEmpty()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);


            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("recset", out entry, out error);
            Assert.IsFalse(entry.TryDeleteRows("*", out error));
            Assert.AreEqual("Recordset was empty.", error);


        }
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_MakeRecordsetEvaluateReady")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchRecordsetIndexes_WhenAliasesPresentAndMadeEvaluateReady_ExpectChildEntryKeys()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            // dl0 - Parent
            dl0.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 2, out error);
            dl0.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 3, out error);
            dl0.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 3, out error);

            // dl1 - Child
            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            compiler.PushBinaryDataList(dl1.UID, dl1, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            // adjust the alias mapping data ;)

            entry.AdjustForIOMapping(dl0.UID, "f1", "recset", "f1", out errors);

            entry.MakeRecordsetEvaluateReady(2, "f1", out error);

            //------------Assert Results-------------------------

            // ensure that it has fetched the alias indexes ;)

            var idxes = entry.FetchRecordsetIndexes();
            var minIdx = idxes.MinIndex();
            var maxIdx = idxes.MaxIndex();
            var gapsCount = idxes.FetchGaps().Count;

            Assert.AreEqual(1, idxes.Count);
            Assert.AreEqual(1, gapsCount);
            Assert.AreEqual(2, minIdx);
            Assert.AreEqual(2, maxIdx);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_MakeRecordsetEvaluateReady")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_MakeRecordsetEvaluateReady_NormalUsage_ExpectEvaluateReadyRecordset()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();
            dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl1.TryCreateScalarValue("myValue", "myScalar", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);

            // skip row 3 ;)

            dl1.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 4, out error);
            dl1.TryCreateRecordsetValue("r4.f2.value", "f2", "recset", 4, out error);


            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);

            //add gap where we want to evaluate for return, as this is the bug ;)
            entry.FetchRecordsetIndexes().AddGap(3);

            entry.MakeRecordsetEvaluateReady(3, "f1", out error);

            //------------Assert Results-------------------------

            var idxes = entry.FetchRecordsetIndexes();
            var minIdx = idxes.MinIndex();
            var maxIdx = idxes.MaxIndex();

            Assert.AreEqual(1, idxes.Count);
            Assert.AreEqual(3, minIdx);
            Assert.AreEqual(3, maxIdx);

        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that once we delete all records with () notation and append new ones we get the correct indexing")]
        public void CanFetchCorrectAppendIndexWhenFullyDeletedViaBlankIndexing()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>".ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            try
            {
                IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

                string error;
                IBinaryDataListEntry entry;

                // emulate the delete ;)
                if(bdl.TryGetEntry("rs", out entry, out error))
                {
                    entry.TryDeleteRows("", out error);
                    entry.TryDeleteRows("", out error);
                    entry.TryDeleteRows("", out error);
                }

                var res = entry.FetchAppendRecordsetIndex();

                Assert.AreEqual(1, res);
            }
            finally
            {
                // clean up ;)
                compiler.ForceDeleteDataListByID(dlID);

            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_Clone")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Clone_IndexDataMoves_ExpectClonedIndexData()
        // ReSharper restore InconsistentNaming
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>".ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);
            string error;
            IBinaryDataListEntry entry;

            // emulate the delete at index 1, aka a header delete ;)
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                entry.TryDeleteRows("1", out error);
            }

            var res = entry.Clone(DataList.Contract.enTranslationDepth.Data, Guid.NewGuid(), out error);

            var cloneMinIndex = res.FetchRecordsetIndexes().FetchNextIndex();
            var entryMinIndex = entry.FetchRecordsetIndexes().FetchNextIndex();

            Assert.AreEqual(2, entryMinIndex);
            Assert.AreEqual(cloneMinIndex, entryMinIndex);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Clone")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Clone_Scalar_ExpectClonedObj()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();



            // dl0 - Parent
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("moo.bob", out entry, out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            var cloned = entry.Clone(DataList.Contract.enTranslationDepth.Data, Guid.NewGuid(), out error);
            Assert.AreNotEqual(cloned.GetHashCode(), entry.GetHashCode());
            Assert.AreEqual(cloned.FetchScalar().TheValue, entry.FetchScalar().TheValue);
            Assert.AreEqual(cloned.FetchScalar().FieldName, entry.FetchScalar().FieldName);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_Clone")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Clone_ShapOnly_ExpectClonedShape()
        // ReSharper restore InconsistentNaming
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>".ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);
            string error;
            IBinaryDataListEntry entry;

            // emulate the delete at index 1, aka a header delete ;)
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                entry.TryDeleteRows("1", out error);
            }

            var res = entry.Clone(DataList.Contract.enTranslationDepth.Shape, Guid.NewGuid(), out error);

            var cloneMinIndex = res.FetchRecordsetIndexes().FetchNextIndex();
            var entryMinIndex = entry.FetchRecordsetIndexes().FetchNextIndex();

            Assert.AreEqual(2, entryMinIndex);
            Assert.AreEqual(cloneMinIndex, entryMinIndex);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry_Clone")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Clone_ScalarIdenticalGuid_ExpectClonedObj()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string error;
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl0 = Dev2BinaryDataListFactory.CreateDataList();



            // dl0 - Parent
            dl0.TryCreateScalarTemplate("moo", "bob", "a scalar", true, true, enDev2ColumnArgumentDirection.Both, out error);

            dl0.TryCreateScalarValue("moo.bob", "moo.bob", out error);
            // push datalist
            compiler.PushBinaryDataList(dl0.UID, dl0, out errors);
            IBinaryDataListEntry entry;
            dl0.TryGetEntry("moo.bob", out entry, out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            var cloned = entry.Clone(DataList.Contract.enTranslationDepth.Data, dl0.UID, out error);

            var internalSbc = new PrivateObject(cloned, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") is SBinaryDataListEntry ? (SBinaryDataListEntry)new PrivateObject(cloned, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") : new SBinaryDataListEntry();
            var internalSbe = new PrivateObject(entry, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") is SBinaryDataListEntry ? (SBinaryDataListEntry)new PrivateObject(entry, new PrivateType(typeof(BinaryDataListEntry))).GetField("_internalObj") : new SBinaryDataListEntry();

            Assert.AreEqual(internalSbc.Columns, internalSbe.Columns);
            Assert.AreEqual(internalSbc.DataListKey, internalSbe.DataListKey);
            Assert.AreEqual(internalSbc.Description, internalSbc.Description);
            Assert.AreEqual(internalSbc.IsEditable, internalSbc.IsEditable);
            Assert.AreEqual(internalSbc.Namespace, internalSbc.Namespace);
            Assert.AreEqual(internalSbc.IsEditable, internalSbe.IsEditable);
            Assert.AreEqual(internalSbc.IsManagmentServicePayload, internalSbe.IsManagmentServicePayload);

        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that once we delete all records with numeric index notation and append new ones we get the correct indexing")]
        public void CanFetchCorrectAppendIndexWhenFullyDeletedViaNumericIndexing()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>".ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            // emulate the delete ;)
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                entry.TryDeleteRows("3", out error);
                entry.TryDeleteRows("2", out error);
                entry.TryDeleteRows("1", out error);
            }

            var res = entry.FetchAppendRecordsetIndex();

            Assert.AreEqual(1, res);
        }


        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that when initalized we have the correct metadata")]
        public void WhenInitedReturnsEmptyAndSizeZero()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                var res = entry.ItemCollectionSize();
                var isEmpty = entry.IsEmpty();

                Assert.IsTrue(isEmpty);
                Assert.AreEqual(0, res);
            }

        }


        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can put a row of data at a valid index")]
        public void CanPutRowAtValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                // ReSharper disable RedundantEmptyObjectCreationArgumentList
                IList<IBinaryDataListItem> row = new List<IBinaryDataListItem>()
// ReSharper restore RedundantEmptyObjectCreationArgumentList
                {
                    itm
                };
                entry.TryPutRecordRowAt(row, 1, out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(1, entry.ItemCollectionSize());
            }
        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we cannot put a row of data at an invalid index")]
        public void CannotPutRowAtInValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", -1);
                IList<IBinaryDataListItem> row = new List<IBinaryDataListItem>
                    {
                    itm
                };
                entry.TryPutRecordRowAt(row, -1, out error);
                Assert.IsTrue(entry.IsEmpty());
                Assert.AreEqual(0, entry.ItemCollectionSize());
            }

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we cannot put a row of data at an invalid index")]
        public void BinaryDataListEntry_TryPutRecordRowAt_IndexLessThan_Last()
        {
            string error;
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList();
            dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl1.TryCreateScalarValue("myValue", "myScalar", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);

            //------------Execute Test---------------------------
            IBinaryDataListEntry entry;
            dl1.TryGetEntry("recset", out entry, out error);
            IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "f1", -1);
            IList<IBinaryDataListItem> row = new List<IBinaryDataListItem>
                    {
                    itm
                };
            entry.TryPutRecordRowAt(row, 1, out error);
            Assert.AreEqual("bob", entry.FetchRecordAt(1, out error)[0].TheValue);


        }
        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can put an item of data at a valid index")]
        public void CanPutItemAtValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                entry.TryPutRecordItemAtIndex(itm, 1, out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(1, entry.ItemCollectionSize());
            }


        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we cannot put an item of data at a invalid index")]
        public void CannotPutItemAtInValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", -1);
                entry.TryPutRecordItemAtIndex(itm, -1, out error);
                Assert.IsTrue(entry.IsEmpty());
                Assert.AreEqual(0, entry.ItemCollectionSize());
            }


        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can append an item of data at a valid index")]
        public void CanAppendRecordItemAtValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                entry.TryAppendRecordItem(itm, out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(1, entry.ItemCollectionSize());
                Assert.AreEqual(2, entry.FetchAppendRecordsetIndex());
            }


        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can fetch a scalar value")]
        public void CanFetchScalarValue()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs><scalarValue/></xml>".ToStringBuilder(), out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;
            IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("MyCoolvalue", "scalarValue");

            if(bdl.TryGetEntry("scalarValue", out entry, out error))
            {
                entry.TryPutScalar(itm, out error);

                Assert.AreEqual("MyCoolvalue", entry.FetchScalar().TheValue);
            }


        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can insert at an index other then 1 and return the correct append index")]
        public void CanInsertAtIndexOtherThan1AndReturnCorrectItemCount()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 3);
                entry.TryPutRecordItemAtIndex(itm, 3, out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(1, entry.ItemCollectionSize());
                Assert.AreEqual(4, entry.FetchAppendRecordsetIndex());
            }

        }


        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can insert at an index other then 1 and return the correct append index")]
        public void CanInsertAtIndex1Then3AndReturnCorrectItemCount()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                IBinaryDataListItem itm2 = Dev2BinaryDataListFactory.CreateBinaryItem("jane", "rs", "val", 3);
                entry.TryPutRecordItemAtIndex(itm, 1, out error);
                entry.TryPutRecordItemAtIndex(itm2, 3, out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(2, entry.ItemCollectionSize());
                Assert.AreEqual(4, entry.FetchAppendRecordsetIndex());
            }

        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we init a datalist and have its entry correctly respect empty rows")]
        public void CanInitDataListAndCorrectlyReturnEmptyData()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Shape = @"<ADL>
                        <gRec>
                        <opt></opt>
                        <display></display>
                        </gRec>
                        </ADL>";

            const string Data = @"<ADL>
                        <gRec>
                        <opt>Value1</opt>
                        <display>display1</display>
                        </gRec>
                        <gRec>
                        <opt>Value2</opt>
                        <display>display2</display>
                        </gRec>
                        <gRec><opt/><display/></gRec>
                        </ADL>";


            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), Data.ToStringBuilder(), Shape.ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("gRec", out entry, out error))
            {

                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(3, entry.ItemCollectionSize());
                Assert.AreEqual(4, entry.FetchAppendRecordsetIndex());
            }
        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can fetch items beyond the populated index and have it return empty")]
        public void CanReturnEmptyForIndexOutOfRange()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Shape = @"<ADL>
                        <gRec>
                        <opt></opt>
                        <display></display>
                        </gRec>
                        </ADL>";

            const string Data = @"<ADL>
                        <gRec>
                        <opt>Value1</opt>
                        <display>display1</display>
                        </gRec>
                        <gRec>
                        <opt>Value2</opt>
                        <display>display2</display>
                        </gRec>
                        <gRec><opt/><display/></gRec>
                        </ADL>";


            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), Data.ToStringBuilder(), Shape.ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("gRec", out entry, out error))
            {
                IBinaryDataListItem itm = entry.TryFetchRecordsetColumnAtIndex("opt", 3, out error);
                Assert.AreEqual(string.Empty, error);
                Assert.AreEqual(string.Empty, itm.TheValue);
            }

        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can make evaluate ready when using a fixed index")]
        public void CanMakeEvaluateReadyAtSpecificIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                IBinaryDataListItem itm2 = Dev2BinaryDataListFactory.CreateBinaryItem("jane", "rs", "val", 2);
                entry.TryPutRecordItemAtIndex(itm, 1, out error);
                entry.TryPutRecordItemAtIndex(itm2, 2, out error);
                entry.MakeRecordsetEvaluateReady(2, "val", out error);

                Assert.AreEqual(2, entry.FetchLastRecordsetIndex());
                Assert.AreEqual(1, entry.ItemCollectionSize());
            }
        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can make evaluate ready when using all indexes")]
        public void CanMakeEvaluateReadyForAllIndexes()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                IBinaryDataListItem itm2 = Dev2BinaryDataListFactory.CreateBinaryItem("jane", "rs", "val", 2);
                entry.TryPutRecordItemAtIndex(itm, 1, out error);
                entry.TryPutRecordItemAtIndex(itm2, 2, out error);
                entry.MakeRecordsetEvaluateReady(GlobalConstants.AllIndexes, "val", out error);

                Assert.AreEqual(2, entry.FetchLastRecordsetIndex());
                Assert.AreEqual(2, entry.ItemCollectionSize());
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_FetchRecordAt")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_FetchRecordAt_ColumnDoesNotExist_BlankRowNotInserted()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>".ToStringBuilder(), "<xml><rs><val/></rs></xml>".ToStringBuilder(), out errors);

            //------------Execute Test---------------------------

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;
            IList<IBinaryDataListItem> data = null;

            // fetch record at non-existent field
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                data = entry.FetchRecordAt(1, "foo", out error);
            }


            //------------Assert Results-------------------------
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(0, data.Count, "Found non-existent field?!");
            // ReSharper restore PossibleNullReferenceException
        }

        [Ignore] //This is for when the ordering of DataList varaibles does not matter
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_Indexer")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Indexer_WhenAliasedAndColumnsAreSwappedInXMLShape_ExpectCorrectData()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            const string InputDefs = @"<Inputs><Input Name=""val"" Source=""[[rs(*).val]]"" Recordset=""rs"" /><Input Name=""val2"" Source=""[[rs(*).val2]]"" Recordset=""rs"" /></Inputs>";
            const string OutputDefs = @"<Outputs><Output Name=""val"" MapsTo=""[[rs(*).val]]"" Value=""[[rs().val]]"" Recordset=""rs"" /><Output Name=""val2"" MapsTo=""[[rs(*).val2]]"" Value=""[[rs().val2]]"" Recordset=""rs"" /></Outputs>";
            var oldID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>val_value</val><val2>val2_value</val2></rs></xml>", "<xml><rs><val/><val2/></rs></xml>".ToStringBuilder(), out errors);
            var shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val2/><val/></rs></xml>".ToStringBuilder(), out errors);

            //------------Execute Test---------------------------
            compiler.ShapeForSubExecution(oldID, shapeID, InputDefs, OutputDefs, out errors); // this triggers the aliasing ;)

            //------------Assert Results-------------------------
            var bdl = compiler.FetchBinaryDataList(shapeID, out errors);
            string error;
            IBinaryDataListEntry entry;

            // how check the ordering of the data coming out after the alias operation
            // because the columns are swapped we need to ensure the right index is fetched from storage ;)
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                var record = entry.FetchRecordAt(1, out error);
                // val2 first because it is first col in shapeID datalist
                Assert.AreEqual("val2_value", record[0].TheValue);
                Assert.AreEqual("val_value", record[1].TheValue);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_Indexer")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Indexer_WhenNonAliasedAndColumnsAreSwappedInXMLShape_ExpectCorrectData()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            const string InputDefs = @"<Inputs><Input Name=""val"" Source=""[[rs(*).val]]"" Recordset=""rs"" /></Inputs>";
            const string OutputDefs = @"<Outputs><Output Name=""result"" MapsTo=""[[rs(*).result]]"" Value=""[[rs().result]]"" Recordset=""rs"" /></Outputs>";
            var oldID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>val_value</val><result></result></rs></xml>".ToStringBuilder(), "<xml><rs><val/><result/></rs></xml>".ToStringBuilder(), out errors);
            var shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><result/><val/></rs></xml>".ToStringBuilder(), out errors);

            //------------Execute Test---------------------------
            compiler.ShapeForSubExecution(oldID, shapeID, InputDefs, OutputDefs, out errors); // this triggers the aliasing ;)

            //------------Assert Results-------------------------
            var bdl = compiler.FetchBinaryDataList(shapeID, out errors);
            string error;
            IBinaryDataListEntry entry;

            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                var record = entry.FetchRecordAt(1, out error);
                Assert.AreEqual("val_value", record[0].TheValue);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("BinaryDataListEntry_Indexer")]
        // ReSharper disable InconsistentNaming
        public void BinaryDataListEntry_Indexer_WhenAliasedAndColumnsAreNotSwappedInXMLShape_ExpectCorrectData()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            const string InputDefs = @"<Inputs><Input Name=""val"" Source=""[[rs(*).val]]"" Recordset=""rs"" /><Input Name=""val2"" Source=""[[rs(*).val2]]"" Recordset=""rs"" /></Inputs>";
            const string OutputDefs = @"<Outputs><Output Name=""val"" MapsTo=""[[rs(*).val]]"" Value=""[[rs().val]]"" Recordset=""rs"" /><Output Name=""val2"" MapsTo=""[[rs(*).val2]]"" Value=""[[rs().val2]]"" Recordset=""rs"" /></Outputs>";
            var oldID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>val_value</val><val2>val2_value</val2></rs></xml>".ToStringBuilder(), "<xml><rs><val/><val2/></rs></xml>".ToStringBuilder(), out errors);
            var shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), "<xml><rs><val/><val2/></rs></xml>".ToStringBuilder(), out errors);

            //------------Execute Test---------------------------
            compiler.ShapeForSubExecution(oldID, shapeID, InputDefs, OutputDefs, out errors); // this triggers the aliasing ;)

            //------------Assert Results-------------------------
            var bdl = compiler.FetchBinaryDataList(shapeID, out errors);
            string error;
            IBinaryDataListEntry entry;

            // how check the ordering of the data coming out after the alias operation
            // because the columns are swapped we need to ensure the right index is fetched from storage ;)
            if(bdl.TryGetEntry("rs", out entry, out error))
            {
                var record = entry.FetchRecordAt(1, out error);
                // right way around because xml shape matches 
                Assert.AreEqual("val_value", record[0].TheValue);
                Assert.AreEqual("val2_value", record[1].TheValue);
            }
            else
            {
                Assert.Fail();
            }

        }

    }
}
