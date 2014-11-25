
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
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DataList
{
    /// <summary>
    /// Summary description for BinaryDataListTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BinaryDataListTest
    {
        // ReSharper disable InconsistentNaming

        private IBinaryDataList _dl1;
        private IBinaryDataList _dl2;
        private IBinaryDataList _dl3;
        private IBinaryDataList _dl4;

        private IBinaryDataList _dlWithBankScalar;
        private IBinaryDataList _dlWithPopulatedScalar;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes


        //Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            string error;

            _dl1 = Dev2BinaryDataListFactory.CreateDataList();
            _dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dl1.TryCreateScalarValue("myValue", "myScalar", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            _dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            _dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            _dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            _dl1.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            _dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            _dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            _dl1.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            // skip 3 ;)

            _dl1.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 4, out error);
            _dl1.TryCreateRecordsetValue("r4.f2.value", "f2", "recset", 4, out error);
            _dl1.TryCreateRecordsetValue("r4.f3.value", "f3", "recset", 4, out error);

            // create 2nd obj
            _dl2 = Dev2BinaryDataListFactory.CreateDataList();
            _dl2.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dl2.TryCreateScalarValue("myValue2", "myScalar", out error);

            cols = new List<Dev2Column> { Dev2BinaryDataListFactory.CreateColumn("f1"), Dev2BinaryDataListFactory.CreateColumn("f2"), Dev2BinaryDataListFactory.CreateColumn("f3") };

            _dl2.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            _dl2.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset", 1, out error);
            _dl2.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset", 1, out error);
            _dl2.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset", 1, out error);

            _dl2.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset", 2, out error);
            _dl2.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset", 2, out error);
            _dl2.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset", 2, out error);

            _dl2.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset", 3, out error);
            _dl2.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset", 3, out error);
            _dl2.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset", 3, out error);

            // create 3rd obj
            _dl3 = Dev2BinaryDataListFactory.CreateDataList();
            _dl3.TryCreateScalarTemplate(string.Empty, "theScalar", "A scalar", true, out error);
            _dl3.TryCreateScalarValue("theValue", "theScalar", out error);

            cols = new List<Dev2Column> { Dev2BinaryDataListFactory.CreateColumn("f1"), Dev2BinaryDataListFactory.CreateColumn("f2"), Dev2BinaryDataListFactory.CreateColumn("f3") };

            _dl3.TryCreateRecordsetTemplate("recset2", "a recordset", cols, true, out error);

            _dl3.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset2", 1, out error);
            _dl3.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset2", 1, out error);
            _dl3.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset2", 1, out error);

            _dl3.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset2", 2, out error);
            _dl3.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset2", 2, out error);
            _dl3.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset2", 2, out error);

            _dl3.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset2", 3, out error);
            _dl3.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset2", 3, out error);
            _dl3.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset2", 3, out error);

            // create 4th obj
            _dl4 = Dev2BinaryDataListFactory.CreateDataList();
            _dl4.TryCreateScalarTemplate(string.Empty, "theScalar", "A scalar", true, out error);
            _dl4.TryCreateScalarValue("theValue4", "theScalar", out error);
            _dl4.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dl4.TryCreateScalarValue("myValue4", "myScalar", out error);

            cols = new List<Dev2Column> { Dev2BinaryDataListFactory.CreateColumn("f1"), Dev2BinaryDataListFactory.CreateColumn("f2"), Dev2BinaryDataListFactory.CreateColumn("f3") };

            _dl4.TryCreateRecordsetTemplate("recset2", "a recordset", cols, true, out error);

            _dl4.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset2", 1, out error);
            _dl4.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset2", 1, out error);
            _dl4.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset2", 1, out error);

            _dl4.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset2", 2, out error);
            _dl4.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset2", 2, out error);
            _dl4.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset2", 2, out error);

            _dl4.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset2", 3, out error);
            _dl4.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset2", 3, out error);
            _dl4.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset2", 3, out error);



            // create 5th obj
            _dlWithBankScalar = Dev2BinaryDataListFactory.CreateDataList();
            _dlWithBankScalar.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dlWithBankScalar.TryCreateScalarValue("", "myScalar", out error);

            _dlWithPopulatedScalar = Dev2BinaryDataListFactory.CreateDataList();
            _dlWithPopulatedScalar.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dlWithPopulatedScalar.TryCreateScalarValue("CAKE!", "myScalar", out error);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        #endregion

        #region Positive Test

        [TestMethod] // - ok
        public void UnionDataWithBlankOverwrite_Expect_BlankScalar()
        {

            ErrorResultTO errors;
            Guid mergeID = _dlWithPopulatedScalar.UID;
            _dlWithPopulatedScalar = _dlWithPopulatedScalar.Merge(_dlWithBankScalar, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);


            IBinaryDataListEntry scalar;
            string error;
            _dlWithPopulatedScalar.TryGetEntry("myScalar", out scalar, out error);

            Assert.AreEqual(string.Empty, scalar.FetchScalar().TheValue);
            Assert.AreEqual(mergeID, _dlWithPopulatedScalar.UID);
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Shape()
        {
            ErrorResultTO errors;

            Guid mergeID = _dl1.UID;
            _dl1 = _dl1.Merge(_dl2, enDataListMergeTypes.Union, enTranslationDepth.Shape, false, out errors);

            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r1.f1.value", (rs.FetchRecordAt(1, out error)[0]).TheValue);
            Assert.AreEqual(mergeID, _dl1.UID);
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Data()
        {
            ErrorResultTO errors;

            Guid mergeID = _dl1.UID;
            _dl1 = _dl1.Merge(_dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r3a.f1.value2", (rs.FetchRecordAt(3, out error)[0]).TheValue);
            Assert.AreEqual(mergeID, _dl1.UID);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_NonExistRow_CausesAdd()
        {
            ErrorResultTO errors;

            _dl1 = _dl1.Merge(_dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r2a.f1.value2", (rs.FetchRecordAt(2, out error)[0]).TheValue);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.AreEqual(4, rs.ItemCollectionSize());
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Data_In_New_Object()
        {
            ErrorResultTO errors;

            Guid mergeID = _dl1.UID;
            _dl1 = _dl1.Merge(_dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r3a.f1.value2", (rs.FetchRecordAt(3, out error)[0]).TheValue);
            Assert.AreNotEqual(mergeID, _dl1.UID);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod]  // - ok
        public void UnionCloneList_VarList_Expected_Merged_Data_In_New_Object()
        {
            ErrorResultTO errors;

            Guid mergeID = _dl1.UID;
            _dl1 = _dl1.Merge(_dl4, enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            IBinaryDataListEntry rs2;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);
            _dl1.TryGetEntry("recset2", out rs2, out error);

            Assert.AreEqual("myValue4", scalar.FetchScalar().TheValue);
            Assert.AreNotEqual(mergeID, _dl1.UID);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.AreEqual(3, rs2.FetchLastRecordsetIndex());
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod]
        public void IntersectList_Expected_Merged_Shape()
        {
            ErrorResultTO errors;

            _dl1 = _dl1.Merge(_dl2, enDataListMergeTypes.Intersection, enTranslationDepth.Shape, false, out errors);
            Guid mergeID = _dl1.UID;

            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r1.f1.value", (rs.FetchRecordAt(1, out error)[0]).TheValue);
            Assert.AreEqual(mergeID, _dl1.UID);
            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod] // - ok
        public void IntersectVarList_Expected_Merged_Data_Missing_recset2()
        {
            ErrorResultTO errors;

            _dl1 = _dl1.Merge(_dl4, enDataListMergeTypes.Intersection, enTranslationDepth.Data, false, out errors);

            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            IBinaryDataListEntry rs2;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("recset", out rs, out error);
            _dl1.TryGetEntry("recset2", out rs2, out error);

            Assert.AreEqual("myValue4", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r1.f1.value", (rs.FetchRecordAt(1, out error)[0]).TheValue);
            Assert.IsTrue(rs2 == null);
            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Missing DataList item [ recset ] ", errors.FetchErrors()[0]);

        }

        #endregion

        #region Delete Row Tests

        [TestMethod]
        public void Delete_Last_Record_Expected_Last_Row_Deleted()
        {

            var entires = _dl2.FetchRecordsetEntries();
            var entry = entires[0];
            int preRecordCount = entry.ItemCollectionSize();
            string errors;
            bool result = entry.TryDeleteRows("", out errors);
            int postRecordCount = entry.ItemCollectionSize();

            Assert.IsTrue(postRecordCount == (preRecordCount - 1));
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void Delete_All_Records_Expected_Blank_Recordset()
        {

            var entires = _dl2.FetchRecordsetEntries();
            var entry = entires[0];
            string errors;
            bool result = entry.TryDeleteRows("*", out errors);

            Assert.IsTrue(entry.IsEmpty());
            Assert.IsTrue(result);
            Assert.AreEqual(1, entry.FetchAppendRecordsetIndex());

        }

        [TestMethod]
        public void Delete_At_Expected_Middle_Row_Deleted()
        {
            var entires = _dl2.FetchRecordsetEntries();
            var entry = entires[0];
            string errors;
            bool result = entry.TryDeleteRows("2", out errors);
            int postRecordCount = entry.ItemCollectionSize();

            Assert.IsTrue(postRecordCount == 2);
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void Delete_At_NullIndex_Expected_NoOperationPerformed()
        {
            var entires = _dl2.FetchRecordsetEntries();
            var entry = entires[0];
            string errors;
            bool result = entry.TryDeleteRows(null, out errors);
            int postRecordCount = entry.ItemCollectionSize();

            Assert.IsTrue(postRecordCount == 3);
            Assert.IsFalse(result);

        }


        #endregion

        #region Negative Test

        [TestMethod] // - ok
        public void IntersectList_DifferentShape_Expected_Errors()
        {

            ErrorResultTO errors;
            _dl1 = _dl1.Merge(_dl3, enDataListMergeTypes.Intersection, enTranslationDepth.Shape, false, out errors);

            IBinaryDataListEntry scalar;
            IBinaryDataListEntry scalar2;
            IBinaryDataListEntry rs;
            IBinaryDataListEntry rs2;
            string error;
            _dl1.TryGetEntry("myScalar", out scalar, out error);
            _dl1.TryGetEntry("theScalar", out scalar2, out error);
            _dl1.TryGetEntry("recset", out rs, out error);
            _dl1.TryGetEntry("recset2", out rs2, out error);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Missing DataList item [ myScalar ] ", errors.FetchErrors()[0]);
            Assert.AreEqual("Missing DataList item [ recset ] ", errors.FetchErrors()[1]);

        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
