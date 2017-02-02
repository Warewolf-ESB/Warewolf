/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.ViewModels.QuickVariableInput;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for QuickVariableInputModelTests
    /// </summary>
    [TestClass]
    public class QuickVariableInputModelTests
    {
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

        #region Case Convert Tests

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInput_Using_Case_Convert_Check_Row_Count_Expected_3()
        // ReSharper restore InconsistentNaming
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));

            ModelItem modelItem = TestModelItemFactory.CreateModelItem(activity);


            QuickVariableInputModel model = new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity);
            int colCount = model.GetCollectionCount();

            Assert.AreEqual(3, colCount);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInput_Using_Case_Convert_Add_List_To_Collection_Overwrite_False_Expected_5()
        // ReSharper restore InconsistentNaming
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));
            QuickVariableInputModel model = new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity);
            IList<string> listToAdd = new List<string>();
            listToAdd.Add("[[Add1]]");
            listToAdd.Add("[[Add2]]");

            model.AddListToCollection(listToAdd, false);

            int colCount = model.GetCollectionCount();

            Assert.AreEqual(5, colCount);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void QuickVariableInput_Using_Case_Convert_Add_List_To_Collection_Overwrite_True_Expected_5()
        // ReSharper restore InconsistentNaming
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));
            QuickVariableInputModel model = new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity);
            IList<string> listToAdd = new List<string>();
            listToAdd.Add("[[Add1]]");
            listToAdd.Add("[[Add2]]");

            model.AddListToCollection(listToAdd, true);

            int colCount = model.GetCollectionCount();

            Assert.AreEqual(2, colCount);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("QuickVariableInputViewModel_MakeDataListReady")]
        public void QuickVariableInputViewModel_MakeDataListReady_WhenItemsHaveSpaces_ShouldRemoveSpaces()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            var makeDataListReady = quickVariableInputViewModel.MakeDataListReady(new List<string> { "Test 1", "Test 4", "T e s t" });
            //------------Assert Results-------------------------
            Assert.AreEqual(3, makeDataListReady.Count);
            Assert.AreEqual("[[Test1]]", makeDataListReady[0]);
            Assert.AreEqual("[[Test4]]", makeDataListReady[1]);
            Assert.AreEqual("[[Test]]", makeDataListReady[2]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_Construct")]
        public void QuickVariableInputViewModel_Construct_SetProperties_ShouldSetCorrectProperties()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            
            var list = new List<string> { "Index", "Chars", "New Line", "Space", "Tab" };

            //------------Assert Results-------------------------
            Assert.AreEqual("Chars", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("", quickVariableInputViewModel.SplitToken);
            Assert.IsFalse(quickVariableInputViewModel.CanAdd);
            Assert.AreEqual(list.Count, quickVariableInputViewModel.SplitTypeList.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_CancelCommand")]
        public void QuickVariableInputViewModel_CancelCommand_ClearData_ShouldResetValues()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Space";
            quickVariableInputViewModel.SplitToken = "-";

            quickVariableInputViewModel.CancelCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Chars", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("", quickVariableInputViewModel.SplitToken);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_WhenNoItems_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Space";
            quickVariableInputViewModel.SplitToken = "-";

            quickVariableInputViewModel.AddCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Space", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("-", quickVariableInputViewModel.SplitToken);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_IncorrectIndex_ShouldFail()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Index";
            quickVariableInputViewModel.SplitToken = "T";
            quickVariableInputViewModel.MakeDataListReady(new List<string> { "Test 1", "Test 4", "T e s t" });
            
            quickVariableInputViewModel.AddCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Index", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("T", quickVariableInputViewModel.SplitToken);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_CorrectIndex_ShouldPass()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Index";
            quickVariableInputViewModel.SplitToken = "0";
            quickVariableInputViewModel.MakeDataListReady(new List<string> { "Test 1", "Test 4", "T e s t" });

            quickVariableInputViewModel.AddCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Index", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("0", quickVariableInputViewModel.SplitToken);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_Chars_ShouldPass()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Chars";
            quickVariableInputViewModel.SplitToken = "";
            quickVariableInputViewModel.MakeDataListReady(new List<string> { "Test 1", "Test 4", "T e s t" });

            quickVariableInputViewModel.AddCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Chars", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("", quickVariableInputViewModel.SplitToken);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_RecordSet_ShouldPass()
        {
            //------------Setup for test--------------------------
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity();
            activity.ConvertCollection.Add(new CaseConvertTO("[[result1]]", "UPPER", "[[result1]]", 1));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result2]]", "UPPER", "[[result2]]", 2));
            activity.ConvertCollection.Add(new CaseConvertTO("[[result3]]", "UPPER", "[[result3]]", 3));
            QuickVariableInputModel model = new QuickVariableInputModel(TestModelItemFactory.CreateModelItem(activity), activity);
            var quickVariableInputViewModel = new QuickVariableInputViewModel(model);

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Chars";
            quickVariableInputViewModel.SplitToken = "(";
            quickVariableInputViewModel.MakeDataListReady(new List<string> { "rec().set", "Test 4", "T e s t" });
            quickVariableInputViewModel.VariableListString = "rec().set";
            quickVariableInputViewModel.Prefix = "rec().set";
            quickVariableInputViewModel.AddCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Chars", quickVariableInputViewModel.SplitType);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("QuickVariableInputViewModel_PreviewCommand")]
        public void QuickVariableInputViewModel_PreviewCommand_WhenItemsHaveValues_ShouldResetValues()
        {
            //------------Setup for test--------------------------
            var quickVariableInputViewModel = new QuickVariableInputViewModel(new QuickVariableInputModel(ModelItemUtils.CreateModelItem(), null));

            //------------Execute Test---------------------------
            quickVariableInputViewModel.SplitType = "Space";
            quickVariableInputViewModel.SplitToken = "-";

            quickVariableInputViewModel.PreviewCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Space", quickVariableInputViewModel.SplitType);
            Assert.AreEqual("-", quickVariableInputViewModel.SplitToken);
        }
    }
}
