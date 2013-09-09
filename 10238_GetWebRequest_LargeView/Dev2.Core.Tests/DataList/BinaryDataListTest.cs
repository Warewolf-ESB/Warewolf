using System.Diagnostics;
using System.IO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Unlimited.UnitTest.Framework.DataList
{
    /// <summary>
    /// Summary description for BinaryDataListTest
    /// </summary>
    [TestClass]
    public class BinaryDataListTest
    {
        private IBinaryDataList dl1;
        private IBinaryDataList dl2;
        private IBinaryDataList dl3;
        private IBinaryDataList dl4;

        private IBinaryDataList dlWithBankScalar;
        private IBinaryDataList dlWithPopulatedScalar;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes


        //Use TestInitialize to run code before running each test 
         [TestInitialize()]
        public void MyTestInitialize()
        {
            string error;

            dl1 = Dev2BinaryDataListFactory.CreateDataList();
            dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl1.TryCreateScalarValue("myValue", "myScalar", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            // skip 3 ;)

            dl1.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 4, out error);
            dl1.TryCreateRecordsetValue("r4.f2.value", "f2", "recset", 4, out error);
            dl1.TryCreateRecordsetValue("r4.f3.value", "f3", "recset", 4, out error);

            // create 2nd obj
            dl2 = Dev2BinaryDataListFactory.CreateDataList();
            dl2.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl2.TryCreateScalarValue("myValue2", "myScalar", out error);

            cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            dl2.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            dl2.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset", 1, out error);

            dl2.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset", 2, out error);

            dl2.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset", 3, out error);
            dl2.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset", 3, out error);
            dl2.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset", 3, out error);

            // create 3rd obj
            dl3 = Dev2BinaryDataListFactory.CreateDataList();
            dl3.TryCreateScalarTemplate(string.Empty, "theScalar", "A scalar", true, out error);
            dl3.TryCreateScalarValue("theValue", "theScalar", out error);

            cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            dl3.TryCreateRecordsetTemplate("recset2", "a recordset", cols, true, out error);

            dl3.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset2", 1, out error);
            dl3.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset2", 1, out error);
            dl3.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset2", 1, out error);

            dl3.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset2", 2, out error);
            dl3.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset2", 2, out error);
            dl3.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset2", 2, out error);

            dl3.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset2", 3, out error);
            dl3.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset2", 3, out error);
            dl3.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset2", 3, out error);

            // create 4th obj
            dl4 = Dev2BinaryDataListFactory.CreateDataList();
            dl4.TryCreateScalarTemplate(string.Empty, "theScalar", "A scalar", true, out error);
            dl4.TryCreateScalarValue("theValue4", "theScalar", out error);
            dl4.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl4.TryCreateScalarValue("myValue4", "myScalar", out error);

            cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            dl4.TryCreateRecordsetTemplate("recset2", "a recordset", cols, true, out error);

            dl4.TryCreateRecordsetValue("r1a.f1.value2", "f1", "recset2", 1, out error);
            dl4.TryCreateRecordsetValue("r1a.f2.value2", "f2", "recset2", 1, out error);
            dl4.TryCreateRecordsetValue("r1a.f3.value2", "f3", "recset2", 1, out error);

            dl4.TryCreateRecordsetValue("r2a.f1.value2", "f1", "recset2", 2, out error);
            dl4.TryCreateRecordsetValue("r2a.f2.value2", "f2", "recset2", 2, out error);
            dl4.TryCreateRecordsetValue("r2a.f3.value2", "f3", "recset2", 2, out error);

            dl4.TryCreateRecordsetValue("r3a.f1.value2", "f1", "recset2", 3, out error);
            dl4.TryCreateRecordsetValue("r3a.f2.value2", "f2", "recset2", 3, out error);
            dl4.TryCreateRecordsetValue("r3a.f3.value2", "f3", "recset2", 3, out error);



            // create 5th obj
            dlWithBankScalar = Dev2BinaryDataListFactory.CreateDataList();
            dlWithBankScalar.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dlWithBankScalar.TryCreateScalarValue("", "myScalar", out error);

            dlWithPopulatedScalar = Dev2BinaryDataListFactory.CreateDataList();
            dlWithPopulatedScalar.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dlWithPopulatedScalar.TryCreateScalarValue("CAKE!", "myScalar", out error);
         }
        
         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup()
         {
         }
        
        #endregion

        #region Positive Test

         [TestMethod] // - ok
         public void UnionDataWithBlankOverwrite_Expect_BlankScalar()
         {

            ErrorResultTO errors;
            Guid mergeID = dlWithPopulatedScalar.UID;
            dlWithPopulatedScalar = dlWithPopulatedScalar.Merge(dlWithBankScalar, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);


            IBinaryDataListEntry scalar;
            string error;
            dlWithPopulatedScalar.TryGetEntry("myScalar", out scalar, out error);

            Assert.AreEqual(string.Empty, scalar.FetchScalar().TheValue);
            Assert.AreEqual(mergeID, dlWithPopulatedScalar.UID);
            Assert.IsFalse(errors.HasErrors());
             
         }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Shape()
        {
            ErrorResultTO errors;

            Guid mergeID = dl1.UID;
            dl1 = dl1.Merge(dl2, enDataListMergeTypes.Union, enTranslationDepth.Shape, false, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            dl1.TryGetEntry("myScalar", out scalar, out error);
            dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual(string.Empty, scalar.FetchScalar().TheValue);
            Assert.AreEqual(string.Empty, (rs.FetchRecordAt(1, out error)[0]).TheValue);
            Assert.AreEqual(mergeID, dl1.UID);
            Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Data()
        {
            ErrorResultTO errors;

            Guid mergeID = dl1.UID;
            dl1 = dl1.Merge(dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            dl1.TryGetEntry("myScalar", out scalar, out error);
            dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r3a.f1.value2", (rs.FetchRecordAt(3, out error)[0]).TheValue);
            Assert.AreEqual(mergeID, dl1.UID);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_NonExistRow_CausesAdd()
        {
            ErrorResultTO errors;

            Guid mergeID = dl1.UID;
            dl1 = dl1.Merge(dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);


            IBinaryDataListEntry scalar;
            IBinaryDataListEntry rs;
            string error;
            dl1.TryGetEntry("myScalar", out scalar, out error);
            dl1.TryGetEntry("recset", out rs, out error);

            Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
            Assert.AreEqual("r2a.f1.value2", (rs.FetchRecordAt(2, out error)[0]).TheValue);
            Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
            Assert.AreEqual(4, rs.ItemCollectionSize());
            Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod] // - ok
        public void UnionCloneList_Expected_Merged_Data_In_New_Object()
        {
            ErrorResultTO errors = new ErrorResultTO();

                Guid mergeID = dl1.UID;
                dl1 = dl1.Merge(dl2, enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);


                IBinaryDataListEntry scalar;
                IBinaryDataListEntry rs;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("recset", out rs, out error);

                Assert.AreEqual("myValue2", scalar.FetchScalar().TheValue);
                Assert.AreEqual("r3a.f1.value2", (rs.FetchRecordAt(3, out error)[0]).TheValue);
                Assert.AreNotEqual(mergeID, dl1.UID);
                Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
                Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod]  // - ok
        public void UnionCloneList_VarList_Expected_Merged_Data_In_New_Object()
        {
            ErrorResultTO errors = new ErrorResultTO();

                Guid mergeID = dl1.UID;
                dl1 = dl1.Merge(dl4, enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);


                IBinaryDataListEntry scalar;
                IBinaryDataListEntry rs;
                IBinaryDataListEntry rs2;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("recset", out rs, out error);
                dl1.TryGetEntry("recset2", out rs2, out error);

                Assert.AreEqual("myValue4", scalar.FetchScalar().TheValue);
                Assert.AreNotEqual(mergeID, dl1.UID);
                Assert.AreEqual(4, rs.FetchLastRecordsetIndex());
                Assert.AreEqual(3, rs2.FetchLastRecordsetIndex());
                Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod] // - ok
        public void IntersectList_Expected_Merged_Shape()
        {
            ErrorResultTO errors = new ErrorResultTO();



                dl1 = dl1.Merge(dl2, enDataListMergeTypes.Intersection, enTranslationDepth.Shape, false, out errors);
                Guid mergeID = dl1.UID;

                IBinaryDataListEntry scalar;
                IBinaryDataListEntry rs;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("recset", out rs, out error);

                Assert.AreEqual(string.Empty, scalar.FetchScalar().TheValue);
                Assert.AreEqual(string.Empty, (rs.FetchRecordAt(1, out error)[0]).TheValue);
                Assert.AreEqual(mergeID, dl1.UID);
                Assert.IsFalse(errors.HasErrors());
            
        }

        [TestMethod] // - ok
        public void IntersectList_VarList_SomeSame_Expected_Merged_Shape_WithErrors()
        {
            ErrorResultTO errors = new ErrorResultTO();

                dl1 = dl1.Merge(dl2, enDataListMergeTypes.Intersection, enTranslationDepth.Shape, false, out errors);
                Guid mergeID = dl1.UID;

                IBinaryDataListEntry scalar;
                IBinaryDataListEntry rs;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("recset", out rs, out error);

                Assert.AreEqual(string.Empty, scalar.FetchScalar().TheValue);
                Assert.AreEqual(string.Empty, (rs.FetchRecordAt(1, out error)[0]).TheValue);
                Assert.AreEqual(mergeID, dl1.UID);
                Assert.IsFalse(errors.HasErrors());
            
        }


        [TestMethod] // - ok
        public void IntersectVarList_Expected_Merged_Data_Missing_recset2()
        {
            ErrorResultTO errors = new ErrorResultTO();

                dl1 = dl1.Merge(dl4, enDataListMergeTypes.Intersection, enTranslationDepth.Data, false, out errors);
                Guid mergeID = dl1.UID;

                IBinaryDataListEntry scalar;
                IBinaryDataListEntry rs;
                IBinaryDataListEntry rs2;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("recset", out rs, out error);
                dl1.TryGetEntry("recset2", out rs2, out error);

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

                var entires = dl2.FetchRecordsetEntries();
                var entry = entires[0];
                int preRecordCount = entry.ItemCollectionSize();
                bool result = entry.TryDeleteRows("");
                int postRecordCount = entry.ItemCollectionSize();

                Assert.IsTrue(postRecordCount == (preRecordCount - 1));
                Assert.IsTrue(result);
            
        }

        [TestMethod]
        public void Delete_All_Records_Expected_Blank_Recordset()
        {

                var entires = dl2.FetchRecordsetEntries();
                var entry = entires[0];
                bool result = entry.TryDeleteRows("*");
                int postRecordCount = entry.ItemCollectionSize();

                Assert.IsTrue(entry.IsEmpty());
                Assert.IsTrue(result);
                Assert.AreEqual(1,entry.FetchAppendRecordsetIndex());
            
        }

        [TestMethod]
        public void Delete_At_Expected_Middle_Row_Deleted()
        {
                var entires = dl2.FetchRecordsetEntries();
                var entry = entires[0];
                int preRecordCount = entry.ItemCollectionSize();
                bool result = entry.TryDeleteRows("2");
                int postRecordCount = entry.ItemCollectionSize();

                Assert.IsTrue(postRecordCount == 2);
                Assert.IsTrue(result);
            
        }

        [TestMethod]
        public void Delete_At_NullIndex_Expected_NoOperationPerformed()
        {
 var entires = dl2.FetchRecordsetEntries();
                var entry = entires[0];
                int preRecordCount = entry.ItemCollectionSize();
                bool result = entry.TryDeleteRows(null);
                int postRecordCount = entry.ItemCollectionSize();

                Assert.IsTrue(postRecordCount == 3);
                Assert.IsFalse(result);
            
        }


        #endregion

        #region Negative Test

        [TestMethod] // - ok
        public void IntersectList_DifferentShape_Expected_Errors()
        {
      
                ErrorResultTO errors = new ErrorResultTO();
                dl1 = dl1.Merge(dl3, enDataListMergeTypes.Intersection, enTranslationDepth.Shape, false, out errors);
                Guid mergeID = dl1.UID;

                IBinaryDataListEntry scalar;
                IBinaryDataListEntry scalar2;
                IBinaryDataListEntry rs;
                IBinaryDataListEntry rs2;
                string error;
                dl1.TryGetEntry("myScalar", out scalar, out error);
                dl1.TryGetEntry("theScalar", out scalar2, out error);
                dl1.TryGetEntry("recset", out rs, out error);
                dl1.TryGetEntry("recset2", out rs2, out error);

                Assert.IsTrue(errors.HasErrors());
                Assert.AreEqual("Missing DataList item [ myScalar ] ", errors.FetchErrors()[0]);
                Assert.AreEqual("Missing DataList item [ recset ] ", errors.FetchErrors()[1]);
            
        }

        #endregion
    }
}
