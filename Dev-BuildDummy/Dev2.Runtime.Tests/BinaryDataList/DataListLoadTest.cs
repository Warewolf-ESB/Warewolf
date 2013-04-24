using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        [ClassInitialize()]
        public static void BaseActivityUnitTestInitialize(TestContext testContext)
        {
            //var pathToRedis = Path.Combine(testContext.DeploymentDirectory, "redis-server.exe");
            //if (_redisProcess == null) _redisProcess = Process.Start(pathToRedis);
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void BaseActivityUnitTestCleanup()
        {

        }

        //

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
            IBinaryDataListEntry val;
            IBinaryDataListEntry res;
            bool tryGetEntry = dl1.TryGetEntry("recset", out val, out er);
            for (int q = 0; q < runs; q++)
            {
                if (tryGetEntry)
                {
                    res = val.Clone(enTranslationDepth.Data, dl1.UID, out er);
                }

            }


            dl1.Dispose();

            DateTime end1 = DateTime.Now;

            long ticks = (end1.Ticks - start1.Ticks);
            double result1 = (ticks / _ticksPerSec);

            Console.WriteLine(result1 + @" seconds for " + runs + @" to clone ");

            Assert.IsTrue(result1 <= 2.5); // Given .01 buffer ;) WAS : 0.065
   
        }

        [TestMethod]
        public void Clone_50EntryRS_10kTimes_AtDepth()
        {
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dl1;
            string error;
            ErrorResultTO errors = new ErrorResultTO();

            double result1;
            int r = 50;
            int runs = 10000;
            using (dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID))
            {

                IList<Dev2Column> cols = new List<Dev2Column>();
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
                cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));

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
                IBinaryDataListEntry val;
                IBinaryDataListEntry res;
                bool tryGetEntry = dl1.TryGetEntry("recset", out val, out er);
                for (int q = 0; q < runs; q++)
                {

                    if (tryGetEntry)
                    {
                        res = val.Clone(enTranslationDepth.Data, dl1.UID, out er);
                    }

                }

                DateTime end1 = DateTime.Now;

                long ticks = (end1.Ticks - start1.Ticks);
                result1 = (ticks / _ticksPerSec);
            }
            Console.WriteLine(result1 + " seconds for " + runs + " to clone ");

            Assert.IsTrue(result1 <= 10, " It Took " + result1); // Given .1 buffer ;) WAS " 0.65

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

                Assert.IsTrue(result1 <=3.5); // Given .01 buffer WAS : 0.075
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

            double result1;


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
            result1 = (ticks / _ticksPerSec);
            
            
            Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");

            Assert.IsTrue(result1 <= 12, " It Took " + result1); // Given 0.75 WAS : 0.75
            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
           
        }
//
//        [TestMethod]
//        public void LargeBDL_Create_1Mil_5Cols_Recordset_Entries()
//        {
//            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
//            IBinaryDataList dl1;
//            string error;
//            ErrorResultTO errors = new ErrorResultTO();
//
//            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);
//
//            IList<Dev2Column> cols = new List<Dev2Column>();
//            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
//            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
//            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
//            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4"));
//            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5"));
//
//            int runs = 1000000;
//
//            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);
//
//            using(dl1)
//            {
//                DateTime start1 = DateTime.Now;
//                for(int i = 0; i < runs; i++)
//                {
//                    dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f1", "recset", (i + 1), out error);
//                    dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", (i + 1), out error);
//                    dl1.TryCreateRecordsetValue("r1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.valuer1.f3.value", "f3", "recset", (i + 1), out error);
//                    dl1.TryCreateRecordsetValue("r1.f3.value", "f4", "recset", (i + 1), out error);
//                    dl1.TryCreateRecordsetValue("r1.f3.value r1.f3.value v r1.f3.value r1.f3.value", "f5", "recset", (i + 1), out error);
//                }
//
//                DateTime end1 = DateTime.Now;
//
//                long ticks = (end1.Ticks - start1.Ticks);
//                double result1 = (ticks / _ticksPerSec);
//
//                Console.WriteLine(result1 + " seconds for " + runs + " with 5 cols");
//
//                Assert.IsTrue(result1 <= 20); // Given 0.75 WAS : 0.75
//                // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
//            }
//        }
//
//        [TestMethod]
//        public void LargeBDL_Create_1Mil_100Cols_Recordset_Entries()
//        {
//            Dev2RedisClient.StartRedis();
//            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
//            IBinaryDataList dl1;
//            string error;
//            ErrorResultTO errors = new ErrorResultTO();
//
//            dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);
//            using(dl1)
//            {
//                IList<Dev2Column> cols = new List<Dev2Column>();
//                for (int i = 1; i <= 100; i++)
//                {
//                    cols.Add(Dev2BinaryDataListFactory.CreateColumn("f" + i));
//                }
//
//                int runs = 500000;
//
//                dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, out error);
//
//
//                DateTime start1 = DateTime.Now;
//                for (int i = 1; i <= runs; i++)
//                {
//                    for (int j = 1; j <= 70; j++)
//                    {
//                        dl1.TryCreateRecordsetValue("r1.f1.value r1.f1.value r1.f1.valuer1.f1.valuer1.f1.value", "f" + j, "recset", i, out error);
//                    }
//                }
//
//                DateTime end1 = DateTime.Now;
//                long ticks = (end1.Ticks - start1.Ticks);
//                double result1 = (ticks / _ticksPerSec);
//
//                Console.WriteLine(result1 + " seconds for " + runs + " with 100 cols");
//
//                Assert.IsTrue(result1 <= 55); // Given 0.75 WAS : 0.75 
//            }
//            
//            // Since Windblow really sucks at resource allocation, I need to adjust these for when it is forced into a multi-user enviroment!!!!
//           
//        }

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
