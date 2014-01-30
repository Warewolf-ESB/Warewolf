using System;
using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.MultiAssignTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityDesignerViewModel));
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
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityCollectionDesignerViewModel<ActivityDTO>));
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
            var vm = CreateDsfMultiAssignActivityViewModel();

            //assert
            Assert.AreEqual(ExpectedCollectionName, vm.CollectionName, "Collection Name not initialized on Multi Assign load");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MultiAssignActivityViewModel_ErrorsProperty")]
        public void MultiAssignActivityViewModel_ErrorsProperty_Constructor_IsNull()
        {
            //------------Setup for test--------------------------
            var multiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Execute Test---------------------------
            var errorInfos = multiAssignActivityViewModel.Errors;
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfos);
        }


        public static MultiAssignDesignerViewModel CreateDsfMultiAssignActivityViewModel()
        {
            var dsfMultiAssignActivityViewModel = new MultiAssignDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity()));
            return dsfMultiAssignActivityViewModel;
        }
    }

    public class ValueCannotBeNumberRule : Rule<String>
    {
        public ValueCannotBeNumberRule(string valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        #region Overrides of RuleBase

        public override IActionableErrorInfo Check()
        {
            if(String.IsNullOrEmpty(ValueToCheck))
                return null;
            if(ValueToCheck.Contains("1"))
            {
                return new ActionableErrorInfo(OnInvalid)
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
        public StringValueCannotContainTestRule(string valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            if(String.IsNullOrEmpty(ValueToCheck)) return null;
            if(ValueToCheck.Contains("Test"))
            {
                return new ActionableErrorInfo(OnInvalid)
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
