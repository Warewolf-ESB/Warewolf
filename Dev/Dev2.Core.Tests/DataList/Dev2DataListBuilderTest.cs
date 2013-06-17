using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Common;
using Dev2.DataList.Contract.TO;

namespace Unlimited.UnitTest.Framework.DataList
{
    /// <summary>
    /// Summary description for Dev2DataListBuilderTest
    /// </summary>
    [TestClass]
    public class Dev2DataListBuilderTest
    {

        private IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private string _dlShape = @"<root><recset><f1/><f2/></recset><scalar/></root>";
        private string _adlData = @"<root><recset><f1>f1_value1</f1><f2>f2_value1</f2></recset><recset><f1>f1_value2</f1><f2>f2_value2</f2></recset><scalar>old_scalar</scalar></root>";

        private TestContext testContextInstance;

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        #region Upsert Builder Test
        [TestMethod]
        public void UpsertPayloadBuilder_With_Flush_Expect_Valid_Entries()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            tmp.Add("[[scalar]]", "zzz");
            tmp.Add("[[scalar2]]", "aaa");
            tmp.FlushIterationFrame();
            
            Assert.AreEqual(1, tmp.FetchFrames().Count);
        }

        [TestMethod]
        public void UpsertPayloadBuilder_Without_Flush_Valid_Entries()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            tmp.Add("[[scalar]]", "zzz");
            tmp.Add("[[scalar2]]", "aaa");

            Assert.AreEqual(1, tmp.FetchFrames().Count);
        }

        [TestMethod]
        public void UpsertPayloadBuilder_MultFrames_Expect_Valid_Entries()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            tmp.Add("[[scalar]]", "zzz");
            tmp.FlushIterationFrame();
            tmp.Add("[[scalar2]]", "aaa");

            Assert.AreEqual(2, tmp.FetchFrames().Count);
        }

        [TestMethod]
        public void UpsertPayloadBuilder_FetchFrames_Expect_Ordered_Valid_Entries()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            tmp.Add("[[scalar]]", "zzz");
            tmp.FlushIterationFrame();
            tmp.Add("[[scalar2]]", "aaa");

            IList<string> expressions = new List<string>();
            IList<string> values = new List<string>();

            IList<IDataListPayloadIterationFrame<string>> frames = tmp.FetchFrames();

            int frameID = 1;
            foreach (IDataListPayloadIterationFrame<string> f in frames)
            {

                while(f.HasData()){
                    DataListPayloadFrameTO<string> t2 = f.FetchNextFrameItem();

                    expressions.Add(t2.Expression+"."+frameID);
                    values.Add(t2.Value+"."+frameID);
                }

                frameID++;
            }

            IList<string> expectedExpressions = new List<string>() { "[[scalar]].1", "[[scalar2]].2" };
            IList<string> expectedValues = new List<string>() { "aaa.1", "zzz.2" };


            CollectionAssert.AreEqual(expectedExpressions.ToArray(), expressions.ToArray());
            CollectionAssert.AreEqual(expectedExpressions.ToArray(), expressions.ToArray());
        }

        
        #endregion

        #region Compiler Test
        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_2RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            string error = string.Empty;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1");
            tmp.Add("[[recset().f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }
                
                // Else we good to go, normal asserts ;)
                IBinaryDataListEntry recset;
                bdl.TryGetEntry("recset", out recset, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate recordset");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("myScalar", scalar.FetchScalar().TheValue);
                Assert.AreEqual(2, recset.FetchLastRecordsetIndex());

            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleStar_Expect_2RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error = string.Empty;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(*).f1]]", "[[scalar]]1");
            tmp.Add("[[recset(*).f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(*).f1]]", "[[scalar]]2");
            tmp.Add("[[recset(*).f2]]", "field2_value2");

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }

                // Else we good to go, normal asserts ;)
                IBinaryDataListEntry recset;
                bdl.TryGetEntry("recset", out recset, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate recordset");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("myScalar", scalar.FetchScalar().TheValue);
                Assert.AreEqual(2, recset.FetchLastRecordsetIndex());

            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_4RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            string error = string.Empty;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1");
            tmp.Add("[[recset().f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }

                // Else we good to go, normal asserts ;)
                IBinaryDataListEntry recset;
                bdl.TryGetEntry("recset", out recset, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate recordset");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("myScalar", scalar.FetchScalar().TheValue);
                Assert.AreEqual(4, recset.FetchLastRecordsetIndex());

            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_3RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error = string.Empty;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(1).f1]]", "field1_value1a");
            tmp.Add("[[recset(1).f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }

                // Else we good to go, normal asserts ;)
                IBinaryDataListEntry recset;
                bdl.TryGetEntry("recset", out recset, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate recordset");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("myScalar", scalar.FetchScalar().TheValue);
                Assert.AreEqual(3, recset.FetchLastRecordsetIndex());
            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_10RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error = string.Empty;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1a");
            tmp.Add("[[recset().f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(10).f1]]", "field1_value2a");
            tmp.Add("[[recset(10).f2]]", "field2_value2a");

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }

                // Else we good to go, normal asserts ;)
                IBinaryDataListEntry recset;
                bdl.TryGetEntry("recset", out recset, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate recordset");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("myScalar", scalar.FetchScalar().TheValue);
                Assert.AreEqual(10, recset.FetchLastRecordsetIndex());

            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_Scalar_WithLastRecord()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error = string.Empty;
            tmp.Add("[[recset().f1]]", "field1_value1a");
            tmp.Add("[[recset().f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[scalar]]", "[[recset(*).f1]]");
            tmp.FlushIterationFrame();

            ErrorResultTO errors = new ErrorResultTO();
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData, _dlShape, out errors);

            if (!errors.HasErrors())
            {
                _compiler.Upsert(id, tmp, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors Upserting, Unit Test Fails");
                }

                IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
                if (errors.HasErrors())
                {
                    Assert.Fail("Errors fetching Binary DataList");
                }

                IBinaryDataListEntry scalar;
                bdl.TryGetEntry("scalar", out scalar, out error);
                if (error != string.Empty)
                {
                    Assert.Fail("Cannot locate scalar");
                }

                // we have a single scalar
                Assert.AreEqual("field1_value1a", scalar.FetchScalar().TheValue);

            }
            else
            {
                Assert.Fail("Errors creating datalist, baseline sanity gone!!!!");
            }
        }

        

        #endregion
    }
}
