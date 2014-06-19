using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            entry.Sort("f1",false,out error);
            var row1 = entry.FetchRecordAt(1, out error);
            Assert.AreEqual(row1[0].TheValue,"a");
            Assert.AreEqual(row1[1].TheValue, "3");
            var row2 = entry.FetchRecordAt(2, out error);
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            Assert.AreEqual(row1[0].TheValue, "");
            Assert.AreEqual(row1[1].TheValue, "2");
            var row2 = entry.FetchRecordAt(2, out error);
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
            var row3 = entry.FetchRecordAt(3, out error);
            Assert.AreEqual(row3[0].TheValue, "12");
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
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            dl0.TryCreateRecordsetValue(new DateTime(2001,01,01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("1", "f2", "recset", 1, out error);
            dl0.TryCreateRecordsetValue("2", "f2", "recset", 3, out error);
            dl0.TryCreateRecordsetValue(new DateTime(1999,01,01).ToString(CultureInfo.InvariantCulture), "f1", "recset", 4, out error);
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
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            Assert.AreEqual(row2[0].TheValue, "");
            Assert.AreEqual(row2[1].TheValue, "");
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
            entry.Sort("f1", true, out error);



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

            entry.MakeRecordsetEvaluateReady(3,"f1", out error);

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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            try
            {
                IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

                string error;
                IBinaryDataListEntry entry;

                // emulate the delete ;)
                if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);
            string error;
            IBinaryDataListEntry entry;

            // emulate the delete at index 1, aka a header delete ;)
            if (bdl.TryGetEntry("rs", out entry, out error))
            {
                entry.TryDeleteRows("1", out error);
            }

            var res = entry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);

            var cloneMinIndex = res.FetchRecordsetIndexes().FetchNextIndex();
            var entryMinIndex = entry.FetchRecordsetIndexes().FetchNextIndex();

            Assert.AreEqual(2, entryMinIndex);
            Assert.AreEqual(cloneMinIndex, entryMinIndex);

        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that once we delete all records with numeric index notation and append new ones we get the correct indexing")]
        public void CanFetchCorrectAppendIndexWhenFullyDeletedViaNumericIndexing()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            // emulate the delete ;)
            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
            {
                var res = entry.ItemCollectionSize();
                var isEmpty = entry.IsEmpty();

                Assert.IsTrue(isEmpty);
                Assert.AreEqual(0,res);
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure we can put an item of data at a valid index")]
        public void CanPutItemAtValidIndex()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                entry.TryAppendRecordItem(itm,out error);
                Assert.IsFalse(entry.IsEmpty());
                Assert.AreEqual(1, entry.ItemCollectionSize());
                Assert.AreEqual(2,entry.FetchAppendRecordsetIndex());
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs><scalarValue/></xml>", out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;
            IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("MyCoolvalue", "scalarValue");

            if (bdl.TryGetEntry("scalarValue", out entry, out error))
            {
                entry.TryPutScalar(itm,out error);

                Assert.AreEqual("MyCoolvalue",entry.FetchScalar().TheValue);
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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


            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), Data, Shape, out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("gRec", out entry, out error))
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


            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), Data, Shape, out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("gRec", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);


            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
            {
                IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem("bob", "rs", "val", 1);
                IBinaryDataListItem itm2 = Dev2BinaryDataListFactory.CreateBinaryItem("jane", "rs", "val", 2);
                entry.TryPutRecordItemAtIndex(itm, 1, out error);
                entry.TryPutRecordItemAtIndex(itm2, 2, out error);
                entry.MakeRecordsetEvaluateReady(2, "val", out error);

                Assert.AreEqual(2,entry.FetchLastRecordsetIndex());
                Assert.AreEqual(1,entry.ItemCollectionSize());
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, "<xml><rs><val/></rs></xml>", out errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;

            if (bdl.TryGetEntry("rs", out entry, out error))
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
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            //------------Execute Test---------------------------

            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;
            IList<IBinaryDataListItem> data = null;

            // fetch record at non-existent field
            if (bdl.TryGetEntry("rs", out entry, out error))
            {
                data = entry.FetchRecordAt(1, "foo", out error);
            }


            //------------Assert Results-------------------------
// ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(0, data.Count, "Found non-existent field?!");
// ReSharper restore PossibleNullReferenceException
        }

    }
}
