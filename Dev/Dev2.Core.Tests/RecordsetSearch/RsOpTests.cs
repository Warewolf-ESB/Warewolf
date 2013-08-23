using System;
using System.Collections.Generic;
using Dev2.DataList;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract;
using Dev2.Common;

namespace Dev2.Tests.RecordsetSearch {
    /// <summary>
    /// Summary description for RsOpTests
    /// </summary>
    [TestClass]
    public class RsOpTests {

        public RsOpTests() {
            //
            // TODO: Add constructor logic here
            //
        }

        private const string dlShape = "<Xml><Recset><Field1/></Recset><Result><res/></Result></Xml>";

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        //#region General Tests

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptySearchString_Expected_AllRecordsReturned()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "", "", "[[Result().res]]", false);
            RsOpContains op = new RsOpContains();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(6, result.Count, "The count is wrong");
        }

        [TestMethod]
        [ExpectedException(typeof(RecordsetNotFoundException))]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptyRecordSetFieldSearchString_Expected_RecordsetNotFoundExceptionThrown()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("", "Contains", "", "", "[[Result().res]]", false);
            RsOpContains op = new RsOpContains();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            func.Invoke();
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindNonCasedMatch()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mrs Mo</Field1></Recset><Recset><Field1>Mr Bob</Field1></Recset><Recset><Field1>Mrs Smith</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "", false);
            RsOpContains op = new RsOpContains();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2,result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindCasedMatch()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>Mo Cake test</Field1></Recset><Recset><Field1>Mr Bob TEST</Field1></Recset><Recset><Field1>Mrs Smith Test</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);
            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", true);
            RsOpContains op = new RsOpContains();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindNonCasedMatch()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>Mo Cake test 1</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "T", "", "[[Result().res]]", false);
            RsOpEndsWith op = new RsOpEndsWith();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2, result.Count, "Invalid Count Returned");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindCasedMatch()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "t", "", "[[Result().res]]", true);
            RsOpEndsWith op = new RsOpEndsWith();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }


        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equals", "1", "", "[[Result().res]]", false);
            RsOpEqual op = new RsOpEqual();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_MatchCase_False_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>testdata1</Field1></Recset><Recset><Field1>testData1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", true);
            RsOpEqual op = new RsOpEqual();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }


        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThan_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            var data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>33</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">", "32", "", "[[Result().res]]", false);
            RsOpGreaterThan op = new RsOpGreaterThan();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThanOrEqualTo_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">=", "25", "", "[[Result().res]]", false);
            RsOpGreaterThanOrEqualTo op = new RsOpGreaterThanOrEqualTo();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2,result.Count);
        }

        [TestMethod]
        public void IsAlphanumeric_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1abc</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false);
            RsOpIsAlphanumeric op = new RsOpIsAlphanumeric();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(3,result.Count);
        }


        [TestMethod]
        public void IsLessThanOrEqual()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<=", "48", "", "[[Result().res]]", false);
            var op = new RsOpLessThanOrEqualTo();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }


        [TestMethod]
        public void IsDate_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-01-01</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithInvalidDate_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-13-13</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(0,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_dd_mm_yyyy_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>13-01-2013</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Dots_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>02.25.2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_BackSlash_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02\25\2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_ForwardSlash_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02/25/2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Space_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02 25 2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_NoSpace_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02252011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Dash_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02-25-2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            RsOpIsDate op = new RsOpIsDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void NotDate_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02-25-2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Date", "", "", "[[Result().res]]", false);
            RsOpNotDate op = new RsOpNotDate();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(5,result.Count);
        }

        [TestMethod]
        public void IsEmailWithValidEmail_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            RsOpIsEmail op = new RsOpIsEmail();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsEmailWithInvalidEmail_Expected_Negative()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>wrongEmail@test!.co.za</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            RsOpIsEmail op = new RsOpIsEmail();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsEmailWithNoDots_Expected_Negative()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            RsOpIsEmail op = new RsOpIsEmail();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(0,result.Count);
        }

        [TestMethod]
        public void NotEmail_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Email", "", "", "[[Result().res]]", false);
            RsOpNotEmail op = new RsOpNotEmail();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(6,result.Count);
        }

        [TestMethod]
        public void IsNumeric_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            RsOpIsNumeric op = new RsOpIsNumeric();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsNumericInvalid_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            RsOpIsNumeric op = new RsOpIsNumeric();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(0,result.Count);
        }

        [TestMethod]
        public void IsNumericWithAlphanumericData_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            RsOpIsNumeric op = new RsOpIsNumeric();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(0,result.Count);
        }

        [TestMethod]
        public void NotNumeric_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Numeric", "", "", "[[Result().res]]", false);
            RsOpNotNumeric op = new RsOpNotNumeric();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(6,result.Count);
        }


        [TestMethod]
        public void IsText_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmailtestcoza</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
            RsOpIsText op = new RsOpIsText();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void IsTextInvalid_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>!@#@#</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
            RsOpIsText op = new RsOpIsText();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void NotText_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>!@#@#</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Text", "", "", "[[Result().res]]", false);
            RsOpNotText op = new RsOpNotText();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(5,result.Count);
        }

        [TestMethod]
        public void IsXML_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><xml/></Field1></Recset><Recset><Field1><x><a/></x></Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is XML", "", "", "[[Result().res]]", false);
            RsOpIsXML op = new RsOpIsXML();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2,result.Count);
        }

        [TestMethod]
        public void NotXML_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><x/></Field1></Recset><Recset><Field1>s</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not XML", "", "", "[[Result().res]]", false);
            RsOpNotXML op = new RsOpNotXML();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(5,result.Count);
        }

        [TestMethod]
        public void LessThan_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<", "48", "", "[[Result().res]]", false);
            RsOpLessThan op = new RsOpLessThan();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void Regex_Expected_Positive()
        {

            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123a</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "^[0-9]*$", "", "[[Result().res]]", false);
            RsOpRegex op = new RsOpRegex();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }

        [TestMethod]
        public void RegexInvalid_Expected_Negative()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123a</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "NotRegexExpression", "", "[[Result().res]]", false);
            RsOpRegex op = new RsOpRegex();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(0,result.Count);
        }

        [TestMethod]
        public void StartsWith_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>41</Field1></Recset><Recset><Field1>1243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "4", "", "[[Result().res]]", false);
            RsOpStartsWith op = new RsOpStartsWith();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(2,result.Count);
        }

        [TestMethod]
        public void StartsWith_MatchCase_False_Expected_Positive()
        {
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>test 41</Field1></Recset><Recset><Field1>1test243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><DataList>";

            ErrorResultTO tmpErrors;
            Guid dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, dlShape, out tmpErrors);
            IBinaryDataList bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", false);
            RsOpStartsWith op = new RsOpStartsWith();
            Func<IList<string>> func = op.BuildSearchExpression(bdl, props);
            IList<string> result = func.Invoke();

            Assert.AreEqual(1,result.Count);
        }
    }
}
