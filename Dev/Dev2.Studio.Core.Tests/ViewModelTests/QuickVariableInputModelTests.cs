
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [ExcludeFromCodeCoverage]
    public class QuickVariableInputModelTests
    {
        public QuickVariableInputModelTests()
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
    }
}
