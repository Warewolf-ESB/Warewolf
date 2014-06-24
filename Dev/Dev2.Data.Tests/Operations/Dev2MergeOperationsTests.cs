using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2MergeOperationsTests
    {
        private IDev2MergeOperations _mergeOperations;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _mergeOperations = new Dev2MergeOperations();
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Index Tests

        [TestMethod]
        public void Merge_Index_Merge_Left_Padding_Pad_Five_Chars_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Index", "15", "0", "Left");
            Assert.AreEqual("TestData!!00000", _mergeOperations.MergeData.ToString());
        }

        [TestMethod]
        public void Merge_Index_Merge_Right_Padding_Pad_Five_Chars_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Index", "15", "0", "Right");
            Assert.AreEqual("00000TestData!!", _mergeOperations.MergeData.ToString());
        }

        [TestMethod]
        public void Merge_Index_Merge_Right_Padding_Remove_Five_Chars_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Index", "5", "0", "Right");
            Assert.AreEqual("TestD", _mergeOperations.MergeData.ToString());
        }

        [TestMethod]
        public void Merge_Index_Merge_Left_Padding_Remove_Five_Chars_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Index", "5", "0", "Left");
            Assert.AreEqual("TestD", _mergeOperations.MergeData.ToString());
        }

        #endregion

        #region Tab Tests

        [TestMethod]
        public void Merge_Tab_Merge_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Tab", "", "", "Left");
            Assert.AreEqual("TestData!!	", _mergeOperations.MergeData.ToString());
        }

        #endregion

        #region Chars Tests

        [TestMethod]
        public void Merge_Chars_Merge_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "Chars", " wow amazing test data:)", "", "Left");
            Assert.AreEqual("TestData!! wow amazing test data:)", _mergeOperations.MergeData.ToString());
        }

        #endregion

        #region New Line Tests

        [TestMethod]
        public void Merge_New_Line_Merge_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(@"TestData!!", "New Line", "", "", "Left");
            var expected = "TestData!!";
            Assert.AreEqual(expected, _mergeOperations.MergeData.ToString().Trim());
        }

        #endregion

        #region Negative Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The value can not be null.")]
        public void Merge_Chars_Merge_With_Null_Value_Expected_Successful_Merge()
        {
            _mergeOperations.Clear();
            _mergeOperations.Merge(null, "Chars", "", "", "Left");
        }

        #endregion
    }
}
