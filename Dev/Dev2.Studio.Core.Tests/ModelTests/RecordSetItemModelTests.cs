using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ModelTests
{
    [TestClass]
    public class RecordSetItemModelTests
    {
        #region Test Fields

        private IRecordSetItemModel _recordSetItemModel;

        #endregion Test Fields

        #region Private Test Methods

        private void TestRecordSetItemModelSet(string name, bool populateAllFields = false)
        {
            if (populateAllFields)
            {
                _recordSetItemModel = new RecordSetItemModel(displayname: name
                    , dev2ColumnArgumentDirection: enDev2ColumnArgumentDirection.None
                    , description: "Test Desc"
                    , parent: new DataListItemModel("Parent")
                     , children: new OptomizedObservableCollection<IRecordSetFieldItemModel>()
                    , hasError: true
                    , errorMessage: string.Empty
                    , isEditable: false
                    , isVisible: false);
            }
            else
            {
                _recordSetItemModel = new RecordSetItemModel(name);
            }

        }

        #endregion Private Test Methods

        #region CTOR Tests

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewRecordSetItemModel_GivenDisplayName_ShouldSetDiplayName()
        {
            //---------------Set up test pack-------------------
            var testitem = "TestItem";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            TestRecordSetItemModelSet(testitem);
            //---------------Test Result -----------------------
            Assert.AreEqual(testitem, _recordSetItemModel.DisplayName);
        }
        #endregion CTOR Tests

        #region Name Validation


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Validatename_GivenValidName_ShouldHaveNoErrorMessage()
        {
            //---------------Set up test pack-------------------
            RecordSetItemModel recordSetItemModel = new RecordSetItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(recordSetItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            recordSetItemModel.DisplayName = "UnitTestDisplayName";
            recordSetItemModel.ValidateName(recordSetItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = string.IsNullOrEmpty(recordSetItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateRecordsetName_GivenInvalidName_ShouldHaveErrorMessage()
        {
            //---------------Set up test pack-------------------
            RecordSetItemModel recordSetItemModel = new RecordSetItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(recordSetItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            recordSetItemModel.DisplayName = "UnitTestWith&amp;&lt;&gt;&quot;&apos;";
            recordSetItemModel.ValidateName(recordSetItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = !string.IsNullOrEmpty(recordSetItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg, "Invalid recordset name does not have error message.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateName_GivenNameHasXmlEscapeCharacters_ShouldHaveErrorMessage()
        {
            //---------------Set up test pack-------------------
            RecordSetItemModel recordSetItemModel = new RecordSetItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(recordSetItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            recordSetItemModel.DisplayName = "UnitTestWith<>";
            recordSetItemModel.ValidateName(recordSetItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = !string.IsNullOrEmpty(recordSetItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg);
        }

        #endregion Name Validation
    }
}