
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.DataList
{
    /// <summary>
    /// Summary description for Dev2DataListBuilderTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DataListBuilderTest
    {

        private readonly IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        const string _dlShape = @"<root><recset><f1/><f2/></recset><scalar/></root>";
        const string _adlData = @"<root><recset><f1>f1_value1</f1><f2>f2_value1</f2></recset><recset><f1>f1_value2</f1><f2>f2_value2</f2></recset><scalar>old_scalar</scalar></root>";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


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
            foreach(IDataListPayloadIterationFrame<string> f in frames)
            {

                while(f.HasData())
                {
                    DataListPayloadFrameTO<string> t2 = f.FetchNextFrameItem();

                    expressions.Add(t2.Expression + "." + frameID);
                    values.Add(t2.Value + "." + frameID);
                }

                frameID++;
            }

            IList<string> expectedExpressions = new List<string> { "[[scalar]].1", "[[scalar2]].2" };


            CollectionAssert.AreEqual(expectedExpressions.ToArray(), expressions.ToArray());
            CollectionAssert.AreEqual(expectedExpressions.ToArray(), expressions.ToArray());
        }


        #endregion

        #region Compiler Test
        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_2RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1");
            tmp.Add("[[recset().f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            string error;
            bdl.TryGetEntry("recset", out recset, out error);

            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();

            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(2, res2);
            Assert.IsNull(tmp.DebugOutputs[0].TargetEntry);
            Assert.IsNull(tmp.DebugOutputs[0].RightEntry);
            Assert.IsNull(tmp.DebugOutputs[0].LeftEntry);

        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_2RecordSetEntries_And_Scalar_AsDebug()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            tmp.IsDebug = true;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1");
            tmp.Add("[[recset().f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            string error;
            bdl.TryGetEntry("recset", out recset, out error);

            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();

            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(2, res2);

            Assert.IsNotNull(tmp.DebugOutputs);
            Assert.IsNotNull(tmp.DebugOutputs[0].LeftEntry);
            Assert.IsNotNull(tmp.DebugOutputs[0].RightEntry);
            Assert.IsNotNull(tmp.DebugOutputs[0].TargetEntry);
            Assert.IsNotNull(tmp.DebugOutputs[0].RightEntry.ComplexExpressionAuditor);
            Assert.IsNotNull(tmp.DebugOutputs[0].LeftEntry.ComplexExpressionAuditor);
            Assert.IsNotNull(tmp.DebugOutputs[0].TargetEntry.ComplexExpressionAuditor);
            Assert.AreEqual(1, tmp.DebugOutputs[0].RightEntry.ComplexExpressionAuditor.FetchAuditItems().Count);
            Assert.AreEqual(1, tmp.DebugOutputs[0].LeftEntry.ComplexExpressionAuditor.FetchAuditItems().Count);
            Assert.AreEqual(5, tmp.DebugOutputs[0].TargetEntry.ComplexExpressionAuditor.FetchAuditItems().Count);

        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleStar_Expect_2RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(*).f1]]", "[[scalar]]1");
            tmp.Add("[[recset(*).f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(*).f1]]", "[[scalar]]2");
            tmp.Add("[[recset(*).f2]]", "field2_value2");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            string error;
            bdl.TryGetEntry("recset", out recset, out error);
            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();

            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(2, res2);


        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_4RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            string error;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1");
            tmp.Add("[[recset().f2]]", "field2_value1");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);
            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            bdl.TryGetEntry("recset", out recset, out error);

            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();

            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(4, res2);
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_5RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1a");
            tmp.Add("[[recset().f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value2");
            tmp.Add("[[recset().f2]]", "field2_value2");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value3");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            bdl.TryGetEntry("recset", out recset, out error);

            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();


            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(5, res2);
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_10RecordSetEntries_And_Scalar()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            string error;
            tmp.Add("[[scalar]]", "myScalar");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset().f1]]", "field1_value1a");
            tmp.Add("[[recset().f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[recset(10).f1]]", "field1_value2a");
            tmp.Add("[[recset(10).f2]]", "field2_value2a");

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData.ToStringBuilder(), new StringBuilder(_dlShape), out errors);


            _compiler.Upsert(id, tmp, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            // Else we good to go, normal asserts ;)
            IBinaryDataListEntry recset;
            bdl.TryGetEntry("recset", out recset, out error);
            IBinaryDataListEntry scalar;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res1 = scalar.FetchScalar().TheValue;
            var res2 = recset.FetchLastRecordsetIndex();

            // we have a single scalar
            Assert.AreEqual("myScalar", res1);
            Assert.AreEqual(10, res2);
        }

        [TestMethod]
        public void UpsertBuilder_AssignStyleAppend_Expect_Scalar_WithLastRecord()
        {
            IDev2DataListUpsertPayloadBuilder<string> tmp = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            tmp.Add("[[recset().f1]]", "field1_value1a");
            tmp.Add("[[recset().f2]]", "field2_value1a");
            tmp.FlushIterationFrame();
            tmp.Add("[[scalar]]", "[[recset(*).f1]]");
            tmp.FlushIterationFrame();

            ErrorResultTO errors;
            Guid id = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _adlData.ToStringBuilder(), new StringBuilder(_dlShape), out errors);

            _compiler.Upsert(id, tmp, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);


            IBinaryDataListEntry scalar;
            string error;
            bdl.TryGetEntry("scalar", out scalar, out error);

            var res = scalar.FetchScalar().TheValue;

            // we have a single scalar
            Assert.AreEqual("field1_value1a", res);

        }

        #endregion
    }
}
