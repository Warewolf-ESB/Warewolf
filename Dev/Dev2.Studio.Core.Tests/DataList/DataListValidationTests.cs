using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;

namespace Dev2.Core.Tests.DataList
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DataListValidationTests
    {

        IDataListValidator Validator = new DataListValidator();

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
        //public void MyTestInitialize() 
        //{                     
        //}
        
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region RecordSet Tests
        [TestMethod]
        public void RecordSet_With_No_Items_HasError_True()
        {
            var child = DataListItemModelFactory.CreateDataListModel("");
            var parent = DataListItemModelFactory.CreateDataListModel("RecordSet");
            parent.Children.Add(child);
            Validator.Add(parent);
            Assert.IsTrue(parent.HasError);
        }

        [TestMethod]
        public void RecordSet_With_Items_HasError_False()
        {
            var child = DataListItemModelFactory.CreateDataListModel("Child");
            var parent = DataListItemModelFactory.CreateDataListModel("RecordSet");
            parent.Children.Add(child);
            Validator.Add(parent);
            Assert.IsFalse(parent.HasError);
        }
        #endregion

        #region Add Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithInvalidName_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar!");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithInvalidNameWithDot_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar.");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithInvalidNameContainsDot_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar.ad");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithInvalidNameWithBrackets_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar()");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithUnderScoreName_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar_1");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddScalar_WithNumber_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);       
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddRecordSet_WithValidNameWithBrackets_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar()", children: new OptomizedObservableCollection<IDataListItemModel> { DataListItemModelFactory.CreateDataListModel("Child")});
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddRecordSet_WithValidNameWithInvalidCharacter_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar().", children: new OptomizedObservableCollection<IDataListItemModel> { DataListItemModelFactory.CreateDataListModel("Child")});
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_Add")]
        public void DataListValidator_AddRecordSetField_WithValidNameWithInvalidCharacter_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var dataListItemModel = DataListItemModelFactory.CreateDataListModel("TestScalar()", children: new OptomizedObservableCollection<IDataListItemModel> { DataListItemModelFactory.CreateDataListModel("Child@")});
            //------------Execute Test---------------------------
            Validator.Add(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.Children[0].HasError);
        }

        [TestMethod]
        public void AddTest_With_Duplicate_Name_Expected_Item_HasError_True()
        {
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar1"));            
            IDataListItemModel newItem = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            Validator.Add(newItem);

            Assert.IsTrue(newItem.HasError);
        }

        [TestMethod]
        public void AddTest_With_Unique_Name_Expected_Item_HasError_False()
        {
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar1"));
            IDataListItemModel newItem = DataListItemModelFactory.CreateDataListModel("TestScalar2");
            Validator.Add(newItem);

            Assert.IsFalse(newItem.HasError);
        }

        //2013.04.10: Ashley Lewis - Bug 9168
        [TestMethod]
        public void AddRecordsetWithDuplicateScalarExpectedRecordsetHasDuplicateVariableErrorMessage()
        {   
            //Initialization
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar"));

            //Execute
            IDataListItemModel newItem = DataListItemModelFactory.CreateDataListModel("TestScalar");
            IDataListItemModel newItemsChild = DataListItemModelFactory.CreateDataListModel("Field");
            newItem.Children.Add(newItemsChild);
            Validator.Add(newItem);

            //Assert
            Assert.AreEqual(StringResources.ErrorMessageDuplicateVariable, newItem.ErrorMessage,"Recordset shows incorrect error when there is a duplicate scalar");
        }

        [TestMethod]
        public void AddScalarWithDuplicateRecordsetExpectedScalarHasDuplicateRecordsetErrorMessage()
        {
            //Initialization
            IDataListItemModel existingRecordset = DataListItemModelFactory.CreateDataListModel("TestRecordset");
            IDataListItemModel existingRecordsetChild = DataListItemModelFactory.CreateDataListModel("Field");
            existingRecordset.Children.Add(existingRecordsetChild);
            Validator.Add(existingRecordset);

            //Execute
            IDataListItemModel newItem = DataListItemModelFactory.CreateDataListModel("TestRecordset");
            Validator.Add(newItem);

            //Assert
            Assert.AreEqual(StringResources.ErrorMessageDuplicateRecordset, newItem.ErrorMessage, "Scalar shows incorrect error when there is a duplicate recordset");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListValidator_ValidateForDuplicates")]
        public void DataListValidator_ValidateForDuplicates_WhenScalarAndRecordSetFieldHasSameName_NoDuplicateItemMessage()
        {
            //------------Setup for test--------------------------
            IDataListItemModel existingRecordset = DataListItemModelFactory.CreateDataListModel("TestRecordset");
            IDataListItemModel existingRecordsetChild = DataListItemModelFactory.CreateDataListModel("Field");
            existingRecordset.Children.Add(existingRecordsetChild);
            Validator.Add(existingRecordset);
            //------------Execute Test---------------------------
            IDataListItemModel newItem = DataListItemModelFactory.CreateDataListModel("Field");
            Validator.Add(newItem);
            //------------Assert Results-------------------------
            Assert.IsNull(newItem.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
            Assert.IsNull(existingRecordsetChild.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
            Assert.IsNull(existingRecordset.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
        }
        #endregion Add Tests

        #region Remove Tests

        [TestMethod]
        public void RemoveTest_Remove_Duplicate_Item_Expected_Item_HasError_False()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar2");
            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar2");
            Validator.Add(newItem1);
            Validator.Add(newItem2);
            Validator.Remove(newItem2);

            Assert.IsFalse(newItem1.HasError);
        }

        [TestMethod]
        public void RemoveTest_RemoveDuplicateItem_WithInvalidChars_Expected_ItemHasErrorTrue()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar@");
            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar@");
            Validator.Add(newItem1);
            Validator.Add(newItem2);
            Validator.Remove(newItem2);
            Validator.Remove(newItem1);

            Assert.IsTrue(newItem1.HasError);
        }

        #endregion Remove Tests

        #region Move Tests

        [TestMethod]
        public void MoveTest_Move_Duplicate_Item_Expected_Items_HasError_False()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            Validator.Add(newItem1);
            Validator.Add(newItem2);
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar2"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar3"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar4"));
            newItem1.DisplayName = "TestScalar5";
            Validator.Move(newItem1);

            Assert.IsFalse(newItem1.HasError && newItem2.HasError);
        }

        [TestMethod]
        public void MoveTest_Move_Item_That_Isnt_In_List_Expected_Item_HasError_False()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar5");
            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar1");            
            Validator.Add(newItem2);
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar2"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar3"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar4"));         
            Validator.Move(newItem1);

            Assert.IsFalse(newItem1.HasError && newItem2.HasError);
        }

        // Sashen.Naidoo
        // BUG 8556 : Tests that when changing the name of a duplicate item, the datalist updates the error state
        //            of the newly changed item and it's previous matching duplicate
        [TestMethod]
        public void MoveTest_Move_ItemThatHasBeenChangedFromPreviousDuplicateState_Expected_ItemHasErrorFalse()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar5");
            newItem1.LastIndexedName = "TestScalar1";

            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            Validator.Add(newItem2);
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar2"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar3"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar4"));
            Validator.Move(newItem1);

            Assert.IsFalse(newItem1.HasError && newItem2.HasError);
        }

        // Sashen.Naidoo
        // BUG 8556 : Tests the integrity of the previous duplicate checking functionality.
        [TestMethod]
        public void MoveTest_Move_DuplicateItem_Expected_ItemHasErrorTrue()
        {
            IDataListItemModel newItem1 = DataListItemModelFactory.CreateDataListModel("TestScalar1");

            IDataListItemModel newItem2 = DataListItemModelFactory.CreateDataListModel("TestScalar1");
            Validator.Add(newItem2);
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar2"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar3"));
            Validator.Add(DataListItemModelFactory.CreateDataListModel("TestScalar4"));
            Validator.Move(newItem1);

            Assert.IsTrue(newItem1.HasError && newItem2.HasError);
        }

        #endregion Move Tests
    }
}
