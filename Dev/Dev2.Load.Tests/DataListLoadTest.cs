using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2.Common;
using Dev2.Data.Audit;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Integration.Tests;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Unlimited.UnitTest.Framework
{
    /// <summary>
    /// Summary description for Dev2RecordsetIndexScopeTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListLoadTest
    {
        private double _ticksPerSec = 10000000;

        #region Clone RS Test

        [TestMethod]
        public void Clone_50EntryRS_1kTimes_AtDepth()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            string error;
            new ErrorResultTO();

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

            int r = 50;
            int runs = 1000;

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);


            for(int i = 0; i < r; i++)
            {
                dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
            }

            DateTime start1 = DateTime.Now;
            string er = string.Empty;
            IBinaryDataListEntry val;
            IBinaryDataListEntry res;
            bool tryGetEntry = dl1.TryGetEntry("recset", out val, out er);
            for(int q = 0; q < runs; q++)
            {
                if(tryGetEntry)
                {
                    res = val.Clone(enTranslationDepth.Data, dl1.UID, out er);
                }

            }


            DateTime end1 = DateTime.Now;

            long ticks = (end1.Ticks - start1.Ticks);
            double result1 = (ticks / _ticksPerSec);

            Console.WriteLine(result1 + @" seconds for " + runs + @" to clone ");

            Assert.IsTrue(result1 <= 10.5); // Given .01 buffer ;) WAS : 0.065

        }

        [TestMethod]
        public void CloneWhereHasComplexExpressionAuditorExpectIsOnClonedObject()
        {
            //------------Setup for test--------------------------
            IBinaryDataList dl1;
            string error;
            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);
            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);
            dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", 1, out error);
            string er = string.Empty;
            IBinaryDataListEntry val;
            IBinaryDataListEntry res;
            bool tryGetEntry = dl1.TryGetEntry("recset", out val, out er);
            val.ComplexExpressionAuditor = new ComplexExpressionAuditor();
            //------------Execute Test---------------------------
            res = val.Clone(enTranslationDepth.Data, dl1.UID, out er);

            //------------Assert Results-------------------------
            Assert.IsNotNull(res.ComplexExpressionAuditor);
        }

        [TestMethod]
        public void Clone_50EntryRS_10kTimes_AtDepth()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            new ErrorResultTO();

            double result1;
            int r = 50;
            int runs = 10000;
            using(dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID))
            {

                IList<Dev2Column> cols = new List<Dev2Column>();
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

                string error;
                dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);

                for(int i = 0; i < r; i++)
                {
                    dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
                    dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
                }

                DateTime start1 = DateTime.Now;
                string er = string.Empty;
                IBinaryDataListEntry val;
                IBinaryDataListEntry res;
                bool tryGetEntry = dl1.TryGetEntry("recset", out val, out er);
                for(int q = 0; q < runs; q++)
                {

                    if(tryGetEntry)
                    {
                        res = val.Clone(enTranslationDepth.Data, dl1.UID, out er);
                    }

                }

                DateTime end1 = DateTime.Now;

                long ticks = (end1.Ticks - start1.Ticks);
                result1 = (ticks / _ticksPerSec);
            }
            Console.WriteLine(result1 + " seconds for " + runs + " to clone ");

            if(result1 <= 5)
            {
                Assert.IsTrue(result1 <= 5, " It Took " + result1); // Given .1 buffer ;) WAS " 0.65
            }
            else if(result1 <= 30)
            {
                Assert.Inconclusive(" It Took " + result1); // Given .1 buffer ;) WAS " 0.65
            }
            else
            {
                Assert.Fail("Time for new hardward buddy!");
            }

        }
        #endregion

        #region Create RS Test

        [TestMethod]
        public void LargeBDL_Create_10k_5Cols_Recordset_Entries()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            new ErrorResultTO();

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
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            new ErrorResultTO();

            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

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

            int runs = 1000000;

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

                Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

                Assert.IsTrue(result1 <= 20, "Expected 20 seconds but got " + result1 + " seconds"); // Given 0.75 WAS : 0.75
                // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
            }
        }

        [TestMethod]
        public void LargeBDL_Persist_1M_5Cols_Recordset_Entries()
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

            int runs = 999999;//1000000;

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

            Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

            Assert.IsTrue(result1 <= 6.0, "Expected 6.0 seconds but got " + result1 + " seconds"); // create speed

        }

        [TestMethod]
        [Ignore]//Ashley - Causes server datalist temp file to spike in size until it runs out of disk.
        public void LargeBDL_Persist_10M_5Cols_Recordset_Entries()
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

            int runs = 10000000;

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

            Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

            Assert.IsTrue(result1 <= 60); // create speed

        }

        #endregion

        #region Delete RS Test

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DeleteRecordsActivity_Delete")]
        public void DeleteRecordsActivity_Delete_LargePayload_TakesLessThenTwoAndAHalfSecond()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "DeleteTestFlow");

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            int startIndex = ResponseData.IndexOf(@"<yeardiff>", StringComparison.Ordinal) + 10;
            int endIndex = ResponseData.IndexOf(@"</yeardiff>", StringComparison.Ordinal);
            string substring = ResponseData.Substring(startIndex, endIndex - startIndex);
            int val;
            if(int.TryParse(substring, out val))
            {
                Assert.IsTrue(val < 2500, "Deleting tool to long it took " + val.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                Assert.Fail("Could get the time");
            }
        }

        #endregion
    }
}
