using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ModelTests
{
    [TestClass]

    public class ScalarItemModelTests
    {
        #region Test Fields

        private IScalarItemModel _scalarItemModel;

        #endregion Test Fields

        #region Private Test Methods

        private void TestScalarItemModelSet(string name, bool populateAllFields = false)
        {
            if (populateAllFields)
            {
                _scalarItemModel = new ScalarItemModel(name, enDev2ColumnArgumentDirection.None
                    , "Test Description"
                    , false
                    , ""
                    , true
                    , true
                    , false
                    , false);
            }
            else
            {
                _scalarItemModel = new ScalarItemModel(name);
            }

        }

        #endregion Private Test Methods

        #region CTOR Tests

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewScalarItemModel_GivenDisplayName_ShouldSetDiplayName()
        {
            //---------------Set up test pack-------------------
            var testitem = "TestItem";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            TestScalarItemModelSet(testitem);
            //---------------Test Result -----------------------
            Assert.AreEqual(testitem, _scalarItemModel.DisplayName);
        }
        #endregion CTOR Tests

        #region Name Validation

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Validatename_GivenValidName_ShouldHaveNoErrorMessage()
        {
            //---------------Set up test pack-------------------
            IScalarItemModel scalarItemModel = new ScalarItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(scalarItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            scalarItemModel.DisplayName = "UnitTestDisplayName";
            scalarItemModel.ValidateName(scalarItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = string.IsNullOrEmpty(scalarItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateScalarName_GivenInvalidName_ShouldHaveErrorMessage()
        {
            //---------------Set up test pack-------------------
            IScalarItemModel scalarItemModel = new ScalarItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(scalarItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            scalarItemModel.DisplayName = "UnitTestWith&amp;&lt;&gt;&quot;&apos;";
            scalarItemModel.ValidateName(scalarItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = !string.IsNullOrEmpty(scalarItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg, "Invalid scalar name does not have error message.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateName_GivenNameHasXmlEscapeCharacters_ShouldHaveErrorMessage()
        {
            //---------------Set up test pack-------------------
            IScalarItemModel scalarItemModel = new ScalarItemModel("DisplayName");
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(scalarItemModel.ErrorMessage));
            //---------------Execute Test ----------------------
            scalarItemModel.DisplayName = "UnitTestWith<>";
            scalarItemModel.ValidateName(scalarItemModel.DisplayName);//Convention
            //---------------Test Result -----------------------
            var hasErrorMsg = !string.IsNullOrEmpty(scalarItemModel.ErrorMessage);
            Assert.IsTrue(hasErrorMsg);
        }

        #endregion Name Validation
    }
}