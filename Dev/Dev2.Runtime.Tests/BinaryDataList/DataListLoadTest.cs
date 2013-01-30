using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Common;

namespace Unlimited.UnitTest.Framework
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DataListLoadTest
    {
        public DataListLoadTest()
        {
        }

        private TestContext testContextInstance;
        private double _ticksPerSec = 10000000;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        #region Clone RS Test
        [TestMethod]
        public void Clone_50EntryRS_1kTimes_AtDepth()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            ErrorResultTO errors = new ErrorResultTO();

            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int r = 50;
            int runs = 1000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

            
            for (int i = 0; i < r; i++)
            {
                dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
            }

            DateTime start1 = DateTime.Now;
            string er = string.Empty;
            for (int q = 0; q < runs; q++)
            {
                IBinaryDataListEntry val;
                IBinaryDataListEntry res;
                if (dl1.TryGetEntry("recset", out val, out er))
                {
                    res = val.Clone(enTranslationDepth.Data, out er);
                }

            }

            DateTime end1 = DateTime.Now;

            long ticks = (end1.Ticks - start1.Ticks);
            double result1 = (ticks / _ticksPerSec);

            Console.WriteLine(result1 + " seconds for " + runs + " to clone ");

            Assert.IsTrue(result1 <= 0.40); // Given .01 buffer ;) WAS : 0.065
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
   
        }

        [TestMethod]
        public void Clone_50EntryRS_10kTimes_AtDepth()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            ErrorResultTO errors = new ErrorResultTO();

            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int r = 50;
            int runs = 10000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);


            for (int i = 0; i < r; i++)
            {
                dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
            }

            DateTime start1 = DateTime.Now;
            string er = string.Empty;
            for (int q = 0; q < runs; q++)
            {
                IBinaryDataListEntry val;
                IBinaryDataListEntry res;
                if (dl1.TryGetEntry("recset", out val, out er))
                {
                    res = val.Clone(enTranslationDepth.Data, out er);
                }

            }

            DateTime end1 = DateTime.Now;

            long ticks = (end1.Ticks - start1.Ticks);
            double result1 = (ticks / _ticksPerSec);

            Console.WriteLine(result1 + " seconds for " + runs + " to clone ");

            Assert.IsTrue(result1 <= 2.5); // Given .1 buffer ;) WAS " 0.65
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
        }
        #endregion

        #region Create RS Test
        [TestMethod]
        public void LargeBDL_Create_10k_5Cols_Recordset_Entries()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            ErrorResultTO errors = new ErrorResultTO();

            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int runs = 10000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

            DateTime start1 = DateTime.Now;
            for (int i = 0; i < runs; i++)
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

            Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

            Assert.IsTrue(result1 <= 0.5); // Given .01 buffer WAS : 0.075
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!

        }

        [TestMethod]
        public void LargeBDL_Create_100k_5Cols_Recordset_Entries()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            ErrorResultTO errors = new ErrorResultTO();

            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int runs = 100000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);


            DateTime start1 = DateTime.Now;
            for (int i = 0; i < runs; i++)
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

            Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

            Assert.IsTrue(result1 <= 3.0); // Given 0.75 WAS : 0.75
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!

        }

        //[TestMethod]
        //public void LargeBDL_Persist_1M_5Cols_Recordset_Entries()
        //{
        //    IDataListCompiler c = DataListFactory.CreateDataListCompiler();
        //    IBinaryDataList dl1;
        //    string error;
        //    ErrorResultTO errors = new ErrorResultTO();

        //    dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

        //    IList<Dev2Column> cols = new List<Dev2Column>();
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

        //    int runs = 999999;//1000000;

        //    dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);


        //    DateTime start1 = DateTime.Now;
        //    for (int i = 0; i < runs; i++)
        //    {
        //        dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
        //    }

        //    DateTime end1 = DateTime.Now;

        //    long ticks = (end1.Ticks - start1.Ticks);
        //    double result1 = (ticks / _ticksPerSec);

        //    Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

        //    Assert.IsTrue(result1 <= 6.0); // create speed

        //}

        //[TestMethod]
        //public void LargeBDL_Persist_10M_5Cols_Recordset_Entries()
        //{
        //    IDataListCompiler c = DataListFactory.CreateDataListCompiler();
        //    IBinaryDataList dl1;
        //    string error;
        //    ErrorResultTO errors = new ErrorResultTO();

        //    dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

        //    IList<Dev2Column> cols = new List<Dev2Column>();
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
        //    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

        //    int runs = 10000000;

        //    dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);


        //    DateTime start1 = DateTime.Now;
        //    for (int i = 0; i < runs; i++)
        //    {
        //        dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
        //        dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
        //    }

        //    DateTime end1 = DateTime.Now;

        //    long ticks = (end1.Ticks - start1.Ticks);
        //    double result1 = (ticks / _ticksPerSec);

        //    Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

        //    Assert.IsTrue(result1 <= 60); // create speed

        //}

        #endregion
    }
}
