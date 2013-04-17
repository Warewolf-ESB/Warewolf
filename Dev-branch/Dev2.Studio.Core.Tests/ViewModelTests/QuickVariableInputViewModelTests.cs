using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Studio.ViewModels.QuickVariableInput;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for QuickVariableInputViewModelTests
    /// </summary>
    [TestClass]
    public class QuickVariableInputViewModelTests
    {
        public QuickVariableInputViewModelTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

        #region Preview Tests

        [TestMethod]
        public void QuickVariableInputViewModelPreviewWithAllFieldsPopulatedOverWriteFalseExpectedCorrectPreview()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            viewModel.Preview();

            Assert.AreEqual(@"4 [[Customer().Fname]]
5 [[Customer().LName]]
6 [[Customer().TelNo]]
", viewModel.PreviewText);
        }

        [TestMethod]
        public void QuickVariableInputViewModelPreviewWithAllFieldsPopulatedOverWriteTrueExpectedCorrectPreview()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = true };
            viewModel.Preview();

            Assert.AreEqual(@"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]
", viewModel.PreviewText);
        }

        [TestMethod]
        public void QuickVariableInputViewModelPreviewWithMoreThenThreeItemsBeingAddedExpectedCorrectPreview()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo,Email", SplitType = "Chars", SplitToken = ",", Overwrite = true };
            viewModel.Preview();

            Assert.AreEqual(@"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]
...", viewModel.PreviewText);
        }

        #endregion

        #region Split Tests

        [TestMethod]
        public void QuickVariableInputViewModelSplitWithIncompleteVariableListAndAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "FName,LName,", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "FName", "LName" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        public void QuickVariableInputViewModelSplitWithAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "FName,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "FName", "LName", "TelNo" };


            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        public void QuickVariableInputViewModelSplitWithIndexAndAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "FNameLNameTelNo", SplitType = "Index", SplitToken = "4", Overwrite = false };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "FNam", "eLNa", "meTe", "lNo" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        public void QuickVariableInputViewModelNewLineWithAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity()))
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"Fname
Lname
TelNo
Email",
                SplitType = "New Line",
                SplitToken = "",
                Overwrite = false
            };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "Fname", "Lname", "TelNo", "Email" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        public void QuickVariableInputViewModelSpaceWithAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity()))
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"Fname Lname TelNo Email",
                SplitType = "Space",
                SplitToken = "",
                Overwrite = false
            };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "Fname", "Lname", "TelNo", "Email" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        public void QuickVariableInputViewModelTabWithAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity()))
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"Fname	Lname	TelNo	Email",
                SplitType = "Tab",
                SplitToken = "",
                Overwrite = false
            };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "Fname", "Lname", "TelNo", "Email" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        #endregion

        #region MakeDataListReady Tests

        [TestMethod]
        public void QuickVariableInputViewModelMakeDataListReadyWithAllFieldsPopulatedExpectedCorrectResultsReturned()
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            IList<string> listToMakeReady = new List<string>();
            listToMakeReady.Add("FName");
            listToMakeReady.Add("LName");
            listToMakeReady.Add("TelNo");

            IList<string> returnedList = viewModel.MakeDataListReady(listToMakeReady);

            Assert.AreEqual(3, returnedList.Count);
            Assert.AreEqual("[[Customer().FName]]", returnedList[0]);
            Assert.AreEqual("[[Customer().LName]]", returnedList[1]);
            Assert.AreEqual("[[Customer().TelNo]]", returnedList[2]);
        }

        #endregion

        #region AddToActivity Tests

        [TestMethod]
        public void QuickVariableInputViewModelAddToActivityWithAllFieldsPopulatedExpectedSixRowsReturned()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.AddToActivity();

            int colCount = viewModel.Model.GetCollectionCount();
            Assert.AreEqual(6, colCount);
        }

        [TestMethod]
        public void QuickVariableInputViewModelAddToActivityWithErrorInPrefixExpectedErrorTextToBeDisplayed()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Custo@mer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.AddToActivity();
            string expected = "Prefix contains invalid characters";
            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Validation Tests

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithInvalidCharsInPrefixWithTwoDotsExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer()..", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.Preview();
            string expected = "Prefix contains invalid characters";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithInvalidCharsInPrefixExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Cust<>omer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.Preview();
            string expected = "Prefix contains invalid characters";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithInvalidCharsInSuffixExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "@", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.Preview();
            string expected = "Suffix contains invalid characters";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithNegativeNumberForIndexSplitExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Index", SplitToken = "-1", Overwrite = false };

            viewModel.Preview();
            string expected = "Please supply a whole positive number for an Index split";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithDecimalNumberForIndexSplitExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Index", SplitToken = "100.3000", Overwrite = false };

            viewModel.Preview();
            string expected = "Please supply a whole positive number for an Index split";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithTextForIndexSplitExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Index", SplitToken = "text", Overwrite = false };

            viewModel.Preview();
            string expected = "Please supply a whole positive number for an Index split";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithBlankValueForCharSplitExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = "", Overwrite = false };

            viewModel.Preview();
            string expected = "Please supply a value for a Character split";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithBlankValueInVariableListExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().", VariableListString = "", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.Preview();
            string expected = "Variable List String can not be blank/empty";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void QuickVariableInputViewModelValidationWithFunnyRecordsetNotationInPreffixExpectedPreviewTextContainsErrorText()
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity)) { Suffix = "", Prefix = "Customer().Other<>text", VariableListString = "Fname,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };

            viewModel.Preview();
            string expected = "Prefix contains invalid characters";

            string actual = viewModel.PreviewText;
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
