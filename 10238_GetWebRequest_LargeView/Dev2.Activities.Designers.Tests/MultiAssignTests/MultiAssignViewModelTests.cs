using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers.DsfMultiAssign;
using Dev2.Core.Tests.Activities;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.MultiAssignTests
{
    [TestClass]
    public class MultiAssignViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfMultiAssignActivityViewModel_Construct")]
        public void DsfMultiAssignActivityViewModel_Construct_IsInstanceOfActivityViewModelBase_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityViewModelBase));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfMultiAssignActivityViewModel_Construct")]
        public void DsfMultiAssignActivityViewModel_Construct_IsInstanceOfActivityCollectionViewModelBaseOfActivityDTO_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityCollectionViewModelBase<ActivityDTO>));
        }

        [TestMethod]
        [TestCategory("MultiAssignActivityViewModel_Constructor")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void MultiAssignActivityViewModel_Constructor_CollectionNameInitialized()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var mockModel = new Mock<ModelItem>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var vm = new TestDsfMultiAssignActivityViewModel(mockModel.Object);

            //assert
            Assert.AreEqual(ExpectedCollectionName, vm.GetCollectionName(), "Collection Name not initialized on Multi Assign load");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenNoItems_NoErrors()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(0,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(0,errorInfos.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenOneItemValid_NoErrors()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO();
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, activityDto.Errors.Count);
            Assert.AreEqual(1,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(0,errorInfos.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenOneItemOneError_HasOneError()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO("TestField","Value",1);
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            var ruleSet = new RuleSet();
            ruleSet.Add(new StringValueCannotContainTestRule(activityDto.FieldName));
            activityDto.Validate(() => activityDto.FieldName, ruleSet);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(1,activityDto.Errors["FieldName"].Count);
            Assert.AreEqual(1,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(1,errorInfos.Count);
            Assert.AreEqual("The value cannot have 'Test' keyword.", errorInfos[0].Message);
        }     
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenOneItemTwoErrors_HasTwoErrors()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO("TestField1", "Value", 1);
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            var ruleSet = new RuleSet();
            ruleSet.Add(new StringValueCannotContainTestRule(activityDto.FieldName));
            ruleSet.Add(new ValueCannotBeNumberRule(activityDto.FieldName));
            activityDto.Validate(() => activityDto.FieldName, ruleSet);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(2,activityDto.Errors["FieldName"].Count);
            Assert.AreEqual(1,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(2,errorInfos.Count);
            Assert.AreEqual("The value cannot have 'Test' keyword.", errorInfos[0].Message);
            Assert.AreEqual("Value cannot be a number.", errorInfos[1].Message);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenTwoItemsOneErrorEach_HasTwoErrors()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO("TestField","Value",1) { FieldValue = "1" };
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            var ruleSet = new RuleSet();
            ruleSet.Add(new StringValueCannotContainTestRule(activityDto.FieldName));
            var ruleSet2 = new RuleSet();
            ruleSet2.Add(new ValueCannotBeNumberRule(activityDto.FieldValue));
            activityDto.Validate(() => activityDto.FieldName, ruleSet);
            activityDto.Validate(() => activityDto.FieldValue, ruleSet2);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(1,activityDto.Errors["FieldName"].Count);
            Assert.AreEqual(1, activityDto.Errors["FieldValue"].Count);
            Assert.AreEqual(1,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(2,errorInfos.Count);
            Assert.AreEqual("The value cannot have 'Test' keyword.", errorInfos[0].Message);
            Assert.AreEqual("Value cannot be a number.", errorInfos[1].Message);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenTwoItemsOneWithOneErrorOneWithTwoErrors_HasThreeErrors()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO("TestField1","Value",1) { FieldValue = "1" };
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            var ruleSet = new RuleSet();
            ruleSet.Add(new StringValueCannotContainTestRule(activityDto.FieldName));
            ruleSet.Add(new ValueCannotBeNumberRule(activityDto.FieldName));
            var ruleSet2 = new RuleSet();
            ruleSet2.Add(new ValueCannotBeNumberRule(activityDto.FieldValue));
            activityDto.Validate(() => activityDto.FieldName, ruleSet);
            activityDto.Validate(() => activityDto.FieldValue, ruleSet2);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(2,activityDto.Errors["FieldName"].Count);
            Assert.AreEqual(1, activityDto.Errors["FieldValue"].Count);
            Assert.AreEqual(1,dsfMultiAssignActivityViewModel.Items.Count);
            Assert.AreEqual(3,errorInfos.Count);
            Assert.AreEqual("The value cannot have 'Test' keyword.", errorInfos[0].Message);
            Assert.AreEqual("Value cannot be a number.", errorInfos[1].Message);
            Assert.AreEqual("Value cannot be a number.", errorInfos[2].Message);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ErrorsProperty")]
        public void MultiAssignActivityViewModel_ErrorsProperty_Constructor_IsNotNull()
        {
            //------------Setup for test--------------------------
            var multiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Execute Test---------------------------
            var errorInfos = multiAssignActivityViewModel.Errors;
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfos);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ErrorsProperty")]
        public void MultiAssignActivityViewModel_ErrorsProperty_SetValue_FiresPropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var multiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Execute Test---------------------------
            var propertyChangedTester = TestUtils.PropertyChangedTester(multiAssignActivityViewModel,()=>multiAssignActivityViewModel.Errors, ()=>multiAssignActivityViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() });
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangedTester);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ValidateErrors")]
        public void MultiAssignActivityViewModel_ValidateErrors_WhenHasErrors_SetsErrorsCollection()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var activityDto = new ActivityDTO("TestField1", "Value", 1) { FieldValue = "1" };
            dsfMultiAssignActivityViewModel.Items.Add(activityDto);
            var ruleSet = new RuleSet();
            ruleSet.Add(new StringValueCannotContainTestRule(activityDto.FieldName));
            ruleSet.Add(new ValueCannotBeNumberRule(activityDto.FieldValue));
            activityDto.Validate(() => activityDto.FieldName);
            activityDto.Validate(() => activityDto.FieldValue, ruleSet);
            //------------Execute Test---------------------------
            var errorInfos = dsfMultiAssignActivityViewModel.ValidationErrors().ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, errorInfos.Count);
            Assert.AreEqual(2,dsfMultiAssignActivityViewModel.Errors.Count);
            Assert.AreEqual("The value cannot have 'Test' keyword.", dsfMultiAssignActivityViewModel.Errors[0].Message);
            Assert.AreEqual("Value cannot be a number.", dsfMultiAssignActivityViewModel.Errors[1].Message);
        }

        static DsfMultiAssignActivityViewModel CreateDsfMultiAssignActivityViewModel()
        {
            var dsfMultiAssignActivityViewModel = new DsfMultiAssignActivityViewModel(ModelItemUtils.CreateModelItem(new object()));
            return dsfMultiAssignActivityViewModel;
        }
    }

    public class ValueCannotBeNumberRule : Rule<String>
    {
        public ValueCannotBeNumberRule(string valueToCheck)
            : base(valueToCheck)
        {
        }

        #region Overrides of RuleBase

        public override IErrorInfo Check()
        {
            if(String.IsNullOrEmpty(ValueToCheck))
                return null;
            if(ValueToCheck.Contains("1"))
            {
                return new ErrorInfo
                {
                    Message = "Value cannot be a number.",
                    FixData = "Variable values cannot be numbers"
                };
            }
            return null;
        }

        #endregion
    }

    public class StringValueCannotContainTestRule : Rule<String>
    {
        #region Overrides of Rule

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StringValueCannotContainTestRule(string valueToCheck)
            : base(valueToCheck)
        {
        }

        public override IErrorInfo Check()
        {
            if(String.IsNullOrEmpty(ValueToCheck)) return null;
            if(ValueToCheck.Contains("Test"))
            {
                return new ErrorInfo
                {
                    Message = "The value cannot have 'Test' keyword.",
                    FixData = "Please rename this field so that it does not contain the word 'Test'."
                };
            }
            return null;
        }

        #endregion
    }
}
