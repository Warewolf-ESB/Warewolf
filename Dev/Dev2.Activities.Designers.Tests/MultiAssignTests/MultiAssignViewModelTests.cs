using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.MultiAssign;
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

}
