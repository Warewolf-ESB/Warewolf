using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract;

namespace Unlimited.UnitTest.Framework.RecordsetSearch {
    /// <summary>
    /// Summary description for RsOpTests
    /// </summary>
    [TestClass]
    public class RsOpTests {

        #region Test Variables

        //IRecordsetScopingObject scopingObj;

        #endregion Test Variables

        public RsOpTests() {
            //
            // TODO: Add constructor logic here
            //
        }

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

        //        #region Additional test attributes
        //        //
        //        // You can use the following additional attributes as you write your tests:
        //        //
        //        // Use ClassInitialize to run code before running the first test in the class
        //        // [ClassInitialize()]
        //        // public static void MyClassInitialize(TestContext testContext) { }
        //        //
        //        // Use ClassCleanup to run code after all tests in a class have run
        //        [ClassCleanup()]
        //        public static void MyClassCleanup() {

        //        }
        //        //
        //        // Use TestInitialize to run code before running each test 
        //        [TestInitialize()]
        //        public void MyTestInitialize() {
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(ParserStrings.FindRecords_DataListShape, ParserStrings.FindRecords_CurrentDL);
        //        }
        //        //
        //        // Use TestCleanup to run code after each test has run
        //        // [TestCleanup()]
        //        // public void MyTestCleanup() { }
        //        //
        //        #endregion

        //        #region General Tests

        //        [TestMethod]
        //        public void Contains_EmptySearchString_Expected_AllRecordsReturned() {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "", "", "[[Result().res]]", false);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(RecordsetNotFoundException))]
        //        public void Contains_EmptyRecordSetFieldSearchString_Expected_RecordsetNotFoundExceptionThrown() {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("", "Contains", "", "", "[[Result().res]]", false);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();
        //        }

        //        [TestMethod]
        //        public void Contains_EmptyResultOutput_Expected_FilteredResultListReturned() {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "", false);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 3);
        //        }

        //        #endregion General Tests

        //        #region Contains Tests

        //        [TestMethod]        
        //        public void Contains_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset()]]", "Contains", "Mrs", "", "[[Result().res]]",false);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 3);
        //        }


        //        [TestMethod]
        //        public void Contains_MatchCase_False_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", false);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void Contains_MatchCase_True_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "test", "", "[[Result().res]]", true);
        //            RsOpContains op = new RsOpContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Contains Tests

        //        #region Not Contains Tests
        //        [TestMethod]
        //        public void NotContains_MatchCase_False_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "mrs", "", "[[Result().res]]", false);
        //            RsOpNotContains op = new RsOpNotContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 6);
        //        }

        //        [TestMethod]
        //        public void NotContains_MatchCase_True_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "Mrs", "", "[[Result().res]]", true);
        //            RsOpNotContains op = new RsOpNotContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 6);
        //        }

        //        [TestMethod]
        //        public void NotContains_MatchCaseLowerSearchUppper_True_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Contains", "mrs", "", "[[Result().res]]", true);
        //            RsOpNotContains op = new RsOpNotContains();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Contains Tests

        //        #region Ends With Tests
        //        [TestMethod]
        //        public void EndWith_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]]", "Ends With", "1", "", "[[Result().res]]", false);
        //            RsOpEndsWith op = new RsOpEndsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 4);
        //        }

        //        [TestMethod]
        //        public void EndsWith_MatchCase_False_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345TestData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "data", "", "[[Result().res]]", false);
        //            RsOpEndsWith op = new RsOpEndsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void EndsWith_MatchCase_True_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345TestData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Ends With", "data", "", "[[Result().res]]", true);
        //            RsOpEndsWith op = new RsOpEndsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Ends With Tests

        //        #region Equals Tests
        //        [TestMethod]
        //        public void Equal_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]]", "Equals", "1", "", "[[Result().res]]", false);
        //            RsOpEqual op = new RsOpEqual();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void Equal_MatchCase_False_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData1</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", false);
        //            RsOpEqual op = new RsOpEqual();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void Equal_MatchCase_True_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData1</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Equal", "testdata1", "", "[[Result().res]]", true);
        //            RsOpEqual op = new RsOpEqual();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Equals Tests

        //        #region Greater Than Tests
        //        [TestMethod]
        //        public void GreaterThan_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]]", ">", "32", "", "[[Result().res]]", false);
        //            RsOpGreaterThan op = new RsOpGreaterThan();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 6);
        //        }

        //        [TestMethod]
        //        public void GreaterThan_Invalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData1</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">", "4", "", "[[Result().res]]", false);
        //            RsOpGreaterThan op = new RsOpGreaterThan();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Greater Than Tests

        //        #region Greater Than Or Equal To Tests
        //        [TestMethod]
        //        public void GreaterThanOrEqualTo_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]]", ">=", "25", "", "[[Result().res]]", false);
        //            RsOpGreaterThanOrEqualTo op = new RsOpGreaterThanOrEqualTo();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 7);
        //        }

        //        [TestMethod]
        //        public void GreaterThanOrEqualTo_Invalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData1</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", ">=", "4", "", "[[Result().res]]", false);
        //            RsOpGreaterThanOrEqualTo op = new RsOpGreaterThanOrEqualTo();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Greater Than Or Equal To Tests

        //        #region Alphanumeric Tests
        //        [TestMethod]
        //        public void IsAlphanumeric_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false);
        //            RsOpIsAlphanumeric op = new RsOpIsAlphanumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsAlphanumericJustNumeric_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>54545451</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false);
        //            RsOpIsAlphanumeric op = new RsOpIsAlphanumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void IsAlphanumericJustAlpha_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>testData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Alphanumeric", "", "", "[[Result().res]]", false);
        //            RsOpIsAlphanumeric op = new RsOpIsAlphanumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void NotAlphanumeric_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field5]]", "<=", "48", "", "[[Result().res]]", false);
        //            RsOpNotAlphanumeric op = new RsOpNotAlphanumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Alphanumeric Tests

        //        #region Date Tests
        //        [TestMethod]
        //        public void IsDate_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field4]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }

        //        [TestMethod]
        //        public void IsDateWithInvalidDate_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>54156sdfs</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_dd_mm_yyyy_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>14/10/1988</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }        

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_Dots_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02.25.2011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_BackSlash_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02\25\2011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_ForwardSlash_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02/25/2011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_Space_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02 25 2011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_NoSpace_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02252011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsDateWithValidDateFormat_With_Dash_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>02-25-2011</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Date", "", "", "[[Result().res]]", false);
        //            RsOpIsDate op = new RsOpIsDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void NotDate_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]]", "Not Date", "", "", "[[Result().res]]", false);
        //            RsOpNotDate op = new RsOpNotDate();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Date Tests

        //        #region Email Tests
        //        [TestMethod]
        //        public void IsEmailWithValidEmail_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>wrongEmail@test.co.za</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
        //            RsOpIsEmail op = new RsOpIsEmail();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void IsEmailWithInvalidEmail_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>wrongEmail@test!.co.za</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
        //            RsOpIsEmail op = new RsOpIsEmail();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void IsEmailWithNoDots_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>wrongEmail@testcoza</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Email", "", "", "[[Result().res]]", false);
        //            RsOpIsEmail op = new RsOpIsEmail();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void NotEmail_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Not Email", "", "", "[[Result().res]]", false);
        //            RsOpNotEmail op = new RsOpNotEmail();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Email Tests

        //        #region Numeric Tests
        //        [TestMethod]
        //        public void IsNumeric_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Is Numeric", "", "", "[[Result().res]]", false);
        //            RsOpIsNumeric op = new RsOpIsNumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }

        //        [TestMethod]
        //        public void IsNumericInvalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
        //            RsOpIsNumeric op = new RsOpIsNumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void IsNumericWithAlphanumericData_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData1234</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Numeric", "", "", "[[Result().res]]", false);
        //            RsOpIsNumeric op = new RsOpIsNumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        [TestMethod]
        //        public void NotNumeric_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Not Numeric", "", "", "[[Result().res]]", false);
        //            RsOpNotNumeric op = new RsOpNotNumeric();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Numeric Tests

        //        #region Text Tests
        //        [TestMethod]
        //        public void IsText_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
        //            RsOpIsText op = new RsOpIsText();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }

        //        [TestMethod]
        //        public void IsTextInvalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>54531</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
        //            RsOpIsText op = new RsOpIsText();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void IsText_WithAlphanumericValue_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData54531</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is Text", "", "", "[[Result().res]]", false);
        //            RsOpIsText op = new RsOpIsText();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        public void NotText_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Not Text", "", "", "[[Result().res]]", false);
        //            RsOpNotText op = new RsOpNotText();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }
        //        #endregion Text Tests

        //        #region XML Tests
        //        [TestMethod]
        //        public void IsXML_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field6]]", "Is XML", "", "", "[[Result().res]]", false);
        //            RsOpIsXML op = new RsOpIsXML();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 5);
        //        }

        //        [TestMethod]
        //        public void IsXMLInvalid_Expected_Exception()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1><x><</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            try
        //            {
        //                scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //                IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Is XML", "", "", "[[Result().res]]", false);
        //                RsOpIsXML op = new RsOpIsXML();
        //                Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //                IList<string> result = func.Invoke();
        //                Assert.Fail("Exception should have been thrown");
        //            }
        //            catch (Exception)
        //            {
        //                Assert.IsTrue(1 == 1);
        //            }            
        //        }

        //        [TestMethod]
        //        public void NotXML_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field6]]", "Not XML", "", "", "[[Result().res]]", false);
        //            RsOpNotXML op = new RsOpNotXML();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 4);
        //        }
        //        #endregion XML Tests

        //        #region Less Than Tests
        //        [TestMethod]
        //        public void LessThan_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]]", "<", "48", "", "[[Result().res]]", false);
        //            RsOpLessThan op = new RsOpLessThan();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 4);
        //        }

        //        [TestMethod]
        //        public void LessThenInvalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<", "4", "", "[[Result().res]]", false);
        //            RsOpLessThan op = new RsOpLessThan();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Less Than Tests

        //        #region Less Than Or Equal To Tests
        //        [TestMethod]
        //        public void LessThanOrEqualTo_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]]", "<=", "48", "", "[[Result().res]]", false);
        //            RsOpLessThanOrEqualTo op = new RsOpLessThanOrEqualTo();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 5);
        //        }

        //        [TestMethod]
        //        public void LessThenOrEqualToInvalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "<=", "4", "", "[[Result().res]]", false);
        //            RsOpLessThanOrEqualTo op = new RsOpLessThanOrEqualTo();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Less Than Or Equal To Tests

        //        #region Regex Tests
        //        [TestMethod]
        //        public void Regex_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Regex", "^[0-9]*$", "", "[[Result().res]]", false);
        //            RsOpRegex op = new RsOpRegex();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 9);
        //        }

        //        [TestMethod]
        //        public void RegexInvalid_Expected_Negative()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Regex", "NotRegexExpression", "", "[[Result().res]]", false);
        //            RsOpRegex op = new RsOpRegex();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Regex Tests

        //        #region Starts With
        //        [TestMethod]
        //        public void StartsWith_Expected_Positive()
        //        {
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field2]],[[Recset().Field1]],[[Recset().Field3]]", "Starts With", "4", "", "[[Result().res]]", false);
        //            RsOpStartsWith op = new RsOpStartsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 2);
        //        }

        //        [TestMethod]
        //        public void StartsWith_MatchCase_False_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", false);
        //            RsOpStartsWith op = new RsOpStartsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 1);
        //        }

        //        [TestMethod]
        //        public void StartsWith_MatchCase_True_Expected_Positive()
        //        {
        //            string dataList = @"<ADL>
        //	<Recset>
        //		<Field1>TestData12345</Field1>
        //	</Recset>
        //</ADL>";
        //            string shape = @"<ADL>
        //	<Recset>
        //		<Field1></Field1>
        //	</Recset>
        //</ADL>";
        //            scopingObj = DataListFactory.CreateRecordsetScopingObject(shape, dataList);
        //            IRecsetSearch props = DataListFactory.CreateSearchTO("[[Recset().Field1]]", "Starts With", "test", "", "[[Result().res]]", true);
        //            RsOpStartsWith op = new RsOpStartsWith();
        //            Func<IList<string>> func = op.BuildSearchExpression(scopingObj, props);
        //            IList<string> result = func.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }
        //        #endregion Starts With  
    }
}
