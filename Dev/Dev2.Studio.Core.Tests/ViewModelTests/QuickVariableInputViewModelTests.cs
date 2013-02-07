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

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInputViewModel_Preview_With_All_Fields_Populated_OverWrite_False_Expected_Correct_Preview()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void QuickVariableInputViewModel_Preview_With_All_Fields_Populated_OverWrite_True_Expected_Correct_Preview()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void QuickVariableInputViewModel_Split_With_All_Fields_Populated_Expected_Correct_Results_Returned()
        // ReSharper restore InconsistentNaming
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "FName,LName,TelNo", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "FName", "LName", "TelNo" };


            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInputViewModel_Split_With_Incomplete_VariableList_And_All_Fields_Populated_Expected_Correct_Results_Returned()
        // ReSharper restore InconsistentNaming
        {
            QuickVariableInputViewModel viewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(new DsfCaseConvertActivity()), new DsfCaseConvertActivity())) { Suffix = "", Prefix = "Customer().", VariableListString = "FName,LName,", SplitType = "Chars", SplitToken = ",", Overwrite = false };
            List<string> returnedList = viewModel.Split();
            List<string> expectedCollection = new List<string> { "FName", "LName" };

            CollectionAssert.AreEqual(expectedCollection, returnedList);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInputViewModel_MakeDataListReady_With_All_Fields_Populated_Expected_Correct_Results_Returned()
        // ReSharper restore InconsistentNaming
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
    }
}
