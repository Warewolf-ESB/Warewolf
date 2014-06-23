using System.IO;
using Dev2.BussinessLogic;

#region Usings

using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.RecordsetSearch
{
    /// <summary>
    ///     Summary description for RsOpTests
    /// </summary>
    [TestClass]    
    public class RsOpTests
    {
        const string DlShape = "<Xml><Recset><Field1/></Recset><Result><res/></Result></Xml>";

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        //#region General Tests

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptySearchString_Expected_AllRecordsReturned()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "", "", "[[Result().res]]", false);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(6, result.Count, "The count is wrong");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptySearchString_RequireAllFieldsToMatch_Expected_AllRecordsReturned()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "", "", "[[Result().res]]", false, true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(6, result.Count, "The count is wrong");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptySearchString_RequireAllFieldsToMatchFalse_Expected_AllRecordsReturned()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "", "", "[[Result().res]]", false);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(6, result.Count, "The count is wrong");
        }

        [TestMethod]
        [ExpectedException(typeof(RecordsetNotFoundException))]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Contains_EmptyRecordSetFieldSearchString_Expected_RecordsetNotFoundExceptionThrown()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("", "Contains", "", "", "[[Result().res]]", false);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            func.Invoke();
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindNonCasedMatch()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mrs Mo</Field1></Recset><Recset><Field1>Mr Bob</Field1></Recset><Recset><Field1>Mrs Smith</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "", false);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindNonCasedMatch_RequiresAllFieldsToMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mrs Mo</Field1></Recset><Recset><Field1>Mr Bob</Field1></Recset><Recset><Field1>Mrs Smith</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "", false, true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindNonCasedMatch_RequiresAllFieldsToMatchTrue_FieldsMatch_Returns()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mrs Mo</Field1></Recset><Recset><Field1>Mrs Bob</Field1></Recset><Recset><Field1>Mrs Smith</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "", false, true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindCasedMatch()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test</Field1></Recset><Recset><Field1>Mr Bob TEST</Field1></Recset><Recset><Field1>Mrs Smith Test</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);
            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindCasedMatch_RequiresAllFieldsToMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test</Field1></Recset><Recset><Field1>Mr Bob TEST</Field1></Recset><Recset><Field1>Mrs Smith Test</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);
            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", true, true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanContainsFindCasedMatch_RequiresAllFieldsToMatch_AllFieldsMatch_ReturnsCorrectIndexes()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test</Field1></Recset><Recset><Field1>Mr Bob test</Field1></Recset><Recset><Field1>Mrs Smith test</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);
            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", true, true);
            var op = new RsOpContains();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindNonCasedMatch()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test 1</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "T", "", "[[Result().res]]", false);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count, "Invalid Count Returned");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindNonCasedMatch_RequiresAllFieldsToMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test 1</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "T", "", "[[Result().res]]", false, true);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count, "Invalid Count Returned");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindNonCasedMatch_RequiresAllFieldsToMatch_AllFieldsMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake test</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "T", "", "[[Result().res]]", false, true);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count, "Invalid Count Returned");
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindCasedMatch()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "t", "", "[[Result().res]]", true);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindCasedMatch_RequiresAllToMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>Mr Bob 1 TEST</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "t", "", "[[Result().res]]", true, true);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void CanEndsWithFindCasedMatch_RequiresAllToMatch_AllFieldsMatch_Returns0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>Mr Bob 1 TESt</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "t", "", "[[Result().res]]", true, true);
            var op = new RsOpEndsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equals", "1", "", "[[Result().res]]", false);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>Mo Cake tes1t</Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equals", "1", "", "[[Result().res]]", false, true);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1>1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equals", "1", "", "[[Result().res]]", false, true);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_MatchCase_False_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>testdata1</Field1></Recset><Recset><Field1>testData1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", true);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_MatchCase_False_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>testdata1</Field1></Recset><Recset><Field1>testData1</Field1></Recset><Recset><Field1>Mrs Smith Tes1t</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", true, true);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void Equal_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>testdata1</Field1></Recset><Recset><Field1>testdata1</Field1></Recset><Recset><Field1>testdata1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", true, true);
            var op = new RsOpEqual();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThan_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>33</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">", "32", "", "[[Result().res]]", false);
            var op = new RsOpGreaterThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThan_RequireAllFields_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>33</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">", "32", "", "[[Result().res]]", false, true);
            var op = new RsOpGreaterThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThan_RequireAllFields_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>35</Field1></Recset><Recset><Field1>33</Field1></Recset><Recset><Field1>34</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">", "32", "", "[[Result().res]]", false, true);
            var op = new RsOpGreaterThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThanOrEqualTo_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">=", "25", "", "[[Result().res]]", false);
            var op = new RsOpGreaterThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThanOrEqualTo_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>32</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">=", "25", "", "[[Result().res]]", false, true);
            var op = new RsOpGreaterThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Find Record Index, Unit Test")]
        [Owner("Travis")]
        public void GreaterThanOrEqualTo_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>26</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>32</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">=", "25", "", "[[Result().res]]", false, true);
            var op = new RsOpGreaterThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsAlphanumeric_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1abc</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false);
            var op = new RsOpIsAlphanumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsAlphanumeric_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1abc</Field1></Recset><Recset><Field1>25</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsAlphanumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsAlphanumeric_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1abc</Field1></Recset><Recset><Field1>ab25</Field1></Recset><Recset><Field1>a1a</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsAlphanumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsLessThanOrEqual()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<=", "48", "", "[[Result().res]]", false);
            var op = new RsOpLessThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsLessThanOrEqual_RequireAllFieldsToMatch_Expect_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<=", "48", "", "[[Result().res]]", false, true);
            var op = new RsOpLessThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsLessThanOrEqual_RequireAllFieldsToMatch_AllFieldsMatch_Expect_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>44</Field1></Recset><Recset><Field1>48</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<=", "48", "", "[[Result().res]]", false, true);
            var op = new RsOpLessThanOrEqualTo();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsDate_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-01-01</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDate_RequireAllFields_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-01-01</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsDate_RequireAllFields_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-01-01</Field1></Recset><Recset><Field1>2013-01-01</Field1></Recset><Recset><Field1>2013-01-01</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsDateWithInvalidDate_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>2013-13-13</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_dd_mm_yyyy_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>13-01-2013</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Dots_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = "<DataList><Recset><Field1>02.25.2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_BackSlash_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02\25\2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_ForwardSlash_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02/25/2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Space_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02 25 2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_NoSpace_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02252011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsDateWithValidDateFormat_With_Dash_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02-25-2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
            var op = new RsOpIsDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void NotDate_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02-25-2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Date", "", "", "[[Result().res]]", false);
            var op = new RsOpNotDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(5, result.Count);
        }

        [TestMethod]
        public void NotDate_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>02-25-2011</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Date", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotDate_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>aad</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Date", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotDate();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsEmailWithValidEmail_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            var op = new RsOpIsEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsEmailWithValidEmail_RequireAllFieldsToMatchTrue_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>55</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsEmailWithValidEmail_RequireAllFieldsToMatchTrue_AllFieldsMatch_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>wrongEmail@test.com</Field1></Recset><Recset><Field1>wrongEmail@test1.co.za</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsEmailWithInvalidEmail_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@test.co.za</Field1></Recset><Recset><Field1>wrongEmail@test!.co.za</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            var op = new RsOpIsEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsEmailWithNoDots_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
            var op = new RsOpIsEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotEmail_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Email", "", "", "[[Result().res]]", false);
            var op = new RsOpNotEmail();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(6, result.Count);
        }

        [TestMethod]
        public void IsNumeric_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            var op = new RsOpIsNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsNumeric_RequiresAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsNumeric_RequiresAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>2</Field1></Recset><Recset><Field1>3</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsNumericInvalid_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail@testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            var op = new RsOpIsNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsNumericWithAlphanumericData_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
            var op = new RsOpIsNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotNumeric_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1></Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Numeric", "", "", "[[Result().res]]", false);
            var op = new RsOpNotNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(6, result.Count);
        }

        [TestMethod]
        public void NotNumeric_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1>1</Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Numeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotNumeric_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmail1testcoza</Field1></Recset><Recset><Field1>aa</Field1></Recset><Recset><Field1>1a</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Numeric", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotNumeric();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsText_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmailtestcoza</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
            var op = new RsOpIsText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void IsText_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmailtestcoza</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>1a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsText_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>wrongEmailtestcoza</Field1></Recset><Recset><Field1>aaa</Field1></Recset><Recset><Field1>a</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsTextInvalid_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>!@#@#</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
            var op = new RsOpIsText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void NotText_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>!@#@#</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Text", "", "", "[[Result().res]]", false);
            var op = new RsOpNotText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(5, result.Count);
        }

        [TestMethod]
        public void NotText_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>!@#@#</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Text", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotText_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>44</Field1></Recset><Recset><Field1>12</Field1></Recset><Recset><Field1>55</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Text", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotText();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void IsXML_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><xml/></Field1></Recset><Recset><Field1><x><a/></x></Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is XML", "", "", "[[Result().res]]", false);
            var op = new RsOpIsXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void IsXML_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><xml/></Field1></Recset><Recset><Field1><x><a/></x></Field1></Recset><Recset><Field1>a</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is XML", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsXML_RequireAllFieldsToMatch_AllFieldsMatch_Expected_2()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><xml/></Field1></Recset><Recset><Field1><x><a/></x></Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is XML", "", "", "[[Result().res]]", false, true);
            var op = new RsOpIsXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void NotXML_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><x/></Field1></Recset><Recset><Field1>s</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not XML", "", "", "[[Result().res]]", false);
            var op = new RsOpNotXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(5, result.Count);
        }

        [TestMethod]
        public void NotXML_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1><x/></Field1></Recset><Recset><Field1>s</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not XML", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NotXML_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>a</Field1></Recset><Recset><Field1>s</Field1></Recset><Recset><Field1>a1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not XML", "", "", "[[Result().res]]", false, true);
            var op = new RsOpNotXML();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void LessThan_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<", "48", "", "[[Result().res]]", false);
            var op = new RsOpLessThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void LessThan_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<", "48", "", "[[Result().res]]", false, true);
            var op = new RsOpLessThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void LessThan_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>23</Field1></Recset><Recset><Field1>21</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<", "48", "", "[[Result().res]]", false, true);
            var op = new RsOpLessThan();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void Regex_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123a</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "^[0-9]*$", "", "[[Result().res]]", false);
            var op = new RsOpRegex();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void Regex_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123a</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "^[0-9]*$", "", "[[Result().res]]", false, true);
            var op = new RsOpRegex();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Regex_RequireAllFieldsToMatch_AllFieldsMatch_Expected_1()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "^[0-9]*$", "", "[[Result().res]]", false, true);
            var op = new RsOpRegex();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void RegexInvalid_Expected_Negative()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>123a</Field1></Recset><Recset><Field1>a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "NotRegexExpression", "", "[[Result().res]]", false);
            var op = new RsOpRegex();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void StartsWith_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>41</Field1></Recset><Recset><Field1>1243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "4", "", "[[Result().res]]", false);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(2, result.Count);
        }


        [TestMethod]
        public void StartsWith_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>41</Field1></Recset><Recset><Field1>1243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "4", "", "[[Result().res]]", false, true);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public void StartsWith_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>41</Field1></Recset><Recset><Field1>4243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "4", "", "[[Result().res]]", false, true);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void StartsWith_MatchCase_False_Expected_Positive()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>test 41</Field1></Recset><Recset><Field1>1test243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", false);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(1, result.Count);
        }


        [TestMethod]
        public void StartsWith_MatchCase_False_RequireAllFieldsToMatch_Expected_0()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>test 41</Field1></Recset><Recset><Field1>1test243a</Field1></Recset><Recset><Field1>4a1</Field1></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset><Recset><Field1/></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", false, true);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void StartsWith_MatchCase_False_RequireAllFieldsToMatch_AllFieldsMatch_Expected_3()
        {
            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>test 41</Field1></Recset><Recset><Field1>test243a</Field1></Recset><Recset><Field1>test4a1</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", false, true);
            var op = new RsOpStartsWith();
            var func = op.BuildSearchExpression(bdl, props);
            var result = func.Invoke();

            Assert.AreEqual(3, result.Count);
        }

        #region New Implementation Tests

        #region Is Between Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenNumbersMatchAllFieldsFalse_ResultsCountOne()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>50</Field1></Recset><Recset><Field1>100</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, false, "25", "75");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenNumbersMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>50</Field1></Recset><Recset><Field1>100</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, true, "25", "75");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenDateTimeMatchAllFieldsFalse_ResultsCountOne()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, false, "10/11/2013", "10/22/2013");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenDateTimeMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, true, "10/11/2013", "10/22/2013");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        [ExpectedException(typeof(InvalidDataException), "IsBetween Numeric and DateTime mis-match")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenDateTimeWrongTypesUsed_ExceptionThrown()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, true, "10/11/2013", "10");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            func.Invoke();

            //------------Assert Results-------------------------           
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBetween_BuildSearchExpression")]
        [ExpectedException(typeof(InvalidDataException), "IsBetween Numeric and DateTime mis-match")]
        public void RsOpIsBetween_BuildSearchExpression_IsBetweenNumberWrongTypesUsed_ExceptionThrown()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>5</Field1></Recset><Recset><Field1>54</Field1></Recset><Recset><Field1>11</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Between", "", "", "[[Result().res]]", false, true, "10", "10/11/2013");
            var rsOpIsBetween = new RsOpIsBetween();
            var func = rsOpIsBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            func.Invoke();

            //------------Assert Results-------------------------           
        }

        #endregion

        #region Not Between Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenNumbersMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>50</Field1></Recset><Recset><Field1>100</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, false, "25", "75");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenNumbersMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>1</Field1></Recset><Recset><Field1>50</Field1></Recset><Recset><Field1>100</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, true, "25", "75");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenDateTimeMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, false, "10/11/2013", "10/22/2013");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenDateTimeMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, true, "10/11/2013", "10/22/2013");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        [ExpectedException(typeof(InvalidDataException), "NotBetween Numeric and DateTime mis-match")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenDateTimeWrongTypesUsed_ExceptionThrown()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10/10/2013</Field1></Recset><Recset><Field1>10/17/2013</Field1></Recset><Recset><Field1>10/24/2013</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, true, "10/11/2013", "10");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            func.Invoke();

            //------------Assert Results-------------------------           
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBetween_BuildSearchExpression")]
        [ExpectedException(typeof(InvalidDataException), "NotBetween Numeric and DateTime mis-match")]
        public void RsOpNotBetween_BuildSearchExpression_NotBetweenNumberWrongTypesUsed_ExceptionThrown()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>5</Field1></Recset><Recset><Field1>54</Field1></Recset><Recset><Field1>11</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Between", "", "", "[[Result().res]]", false, true, "10", "10/11/2013");
            var rsOpNotBetween = new RsOpNotBetween();
            var func = rsOpNotBetween.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            func.Invoke();

            //------------Assert Results-------------------------           
        }

        #endregion

        #region Is Binary Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBinary_BuildSearchExpression")]
        public void RsOpIsBinary_BuildSearchExpression_IsBinaryMatchAllFieldsFalse_ResultsCountOne()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10110101</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Binary", "", "", "[[Result().res]]", false);
            var rsOpIsBinary = new RsOpIsBinary();
            var func = rsOpIsBinary.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsBinary_BuildSearchExpression")]
        public void RsOpIsBinary_BuildSearchExpression_IsBinaryMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10110101</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Binary", "", "", "[[Result().res]]", false, true);
            var rsOpIsBinary = new RsOpIsBinary();
            var func = rsOpIsBinary.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Binary Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBinary_BuildSearchExpression")]
        public void RsOpNotBinary_BuildSearchExpression_NotBinaryMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10110101</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Binary", "", "", "[[Result().res]]", false);
            var RsOpNotBinary = new RsOpNotBinary();
            var func = RsOpNotBinary.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBinary_BuildSearchExpression")]
        public void RsOpNotBinary_BuildSearchExpression_NotBinaryMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>10110101</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Binary", "", "", "[[Result().res]]", false, true);
            var RsOpNotBinary = new RsOpNotBinary();
            var func = RsOpNotBinary.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Is Hex Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsHex_BuildSearchExpression")]
        public void RsOpIsHex_BuildSearchExpression_IsHexMatchAllFieldsFalse_ResultsCountOne()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>77617265776f6c66</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Hex", "", "", "[[Result().res]]", false);
            var RsOpIsHex = new RsOpIsHex();
            var func = RsOpIsHex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpIsHex_BuildSearchExpression")]
        public void RsOpIsHex_BuildSearchExpression_IsHexMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>77617265776f6c66</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Hex", "", "", "[[Result().res]]", false, true);
            var RsOpIsHex = new RsOpIsHex();
            var func = RsOpIsHex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Hex Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotHex_BuildSearchExpression")]
        public void RsOpNotHex_BuildSearchExpression_NotHexMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>77617265776f6c66</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Hex", "", "", "[[Result().res]]", false);
            var RsOpNotHex = new RsOpNotHex();
            var func = RsOpNotHex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotHex_BuildSearchExpression")]
        public void RsOpNotHex_BuildSearchExpression_NotHexMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>77617265776f6c66</Field1></Recset><Recset><Field1>test</Field1></Recset><Recset><Field1>data</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Hex", "", "", "[[Result().res]]", false, true);
            var RsOpNotHex = new RsOpNotHex();
            var func = RsOpNotHex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Base64 Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBase64_BuildSearchExpression")]
        public void RsOpNotBase64_BuildSearchExpression_NotBase64MatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Base64", "", "", "[[Result().res]]", false);
            var RsOpNotBase64 = new RsOpNotBase64();
            var func = RsOpNotBase64.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotBase64_BuildSearchExpression")]
        public void RsOpNotBase64_BuildSearchExpression_NotBase64MatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Base64", "", "", "[[Result().res]]", false, true);
            var RsOpNotBase64 = new RsOpNotBase64();
            var func = RsOpNotBase64.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Ends With Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotEndsWith_BuildSearchExpression")]
        public void RsOpNotEndsWith_BuildSearchExpression_NotEndsWithMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Doesn't End With", "=", "", "[[Result().res]]", false);
            var RsOpNotEndsWith = new RsOpNotEndsWith();
            var func = RsOpNotEndsWith.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotEndsWith_BuildSearchExpression")]
        public void RsOpNotEndsWith_BuildSearchExpression_NotEndsWithMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Doesn't End With", "=", "", "[[Result().res]]", false, true);
            var RsOpNotEndsWith = new RsOpNotEndsWith();
            var func = RsOpNotEndsWith.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Starts With Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotStartsWith_BuildSearchExpression")]
        public void RsOpNotStartsWith_BuildSearchExpression_NotStartsWithMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Doesn't Start With", "d", "", "[[Result().res]]", false);
            var RsOpNotStartsWith = new RsOpNotStartsWith();
            var func = RsOpNotStartsWith.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotStartsWith_BuildSearchExpression")]
        public void RsOpNotStartsWith_BuildSearchExpression_NotStartsWithMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>d2FyZXdvbGY=</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Doesn't Start With", "d", "", "[[Result().res]]", false, true);
            var RsOpNotStartsWith = new RsOpNotStartsWith();
            var func = RsOpNotStartsWith.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region Not Regex Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotRegex_BuildSearchExpression")]
        public void RsOpNotRegex_BuildSearchExpression_NotRegexMatchAllFieldsFalse_ResultsCountTwo()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>999.999.999.999</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Regex", @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "", "[[Result().res]]", false);
            var RsOpNotRegex = new RsOpNotRegex();
            var func = RsOpNotRegex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RsOpNotRegex_BuildSearchExpression")]
        public void RsOpNotRegex_BuildSearchExpression_NotRegexMatchAllFieldsTrue_ResultsCountZero()
        {
            //------------Setup for test--------------------------            

            var dlc = DataListFactory.CreateDataListCompiler();

            const string data = @"<DataList><Recset><Field1>999.999.999.999</Field1></Recset><Recset><Field1>yay</Field1></Recset><Recset><Field1>wow</Field1></Recset></DataList>";

            ErrorResultTO tmpErrors;
            var dlID = dlc.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, DlShape, out tmpErrors);
            var bdl = dlc.FetchBinaryDataList(dlID, out tmpErrors);

            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Not Regex", @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "", "[[Result().res]]", false, true);
            var RsOpNotRegex = new RsOpNotRegex();
            var func = RsOpNotRegex.BuildSearchExpression(bdl, props);

            //------------Execute Test---------------------------
            var result = func.Invoke();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #endregion
    }
}