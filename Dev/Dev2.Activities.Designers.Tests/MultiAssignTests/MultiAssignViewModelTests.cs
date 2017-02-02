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
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
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
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfMultiAssignActivityViewModel_Construct")]
        public void DsfMultiAssignActivityViewModel_Construct_ShouldHaveCorrectDisplayName()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityDesignerViewModel));
            //------------Execute Test---------------------------
            var displayName = dsfMultiAssignActivityViewModel.ModelItem.GetProperty<string>("DisplayName");
            //------------Assert Results-------------------------
            Assert.AreEqual("Assign (0)", displayName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfMultiAssignActivityViewModel_Construct")]
        public void DsfMultiAssignActivityViewModel_AddNewRow_ShouldUpdateDisplayName()
        {
            //------------Setup for test--------------------------
            var dsfMultiAssignActivityViewModel = CreateDsfMultiAssignActivityViewModel();
            var displayName = dsfMultiAssignActivityViewModel.ModelItem.GetProperty<string>("DisplayName");
            var activityDto = new ActivityDTO("[[a]]", "Value", 2);
            dsfMultiAssignActivityViewModel.ModelItemCollection.Add(ModelItemUtils.CreateModelItem(activityDto));
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dsfMultiAssignActivityViewModel, typeof(ActivityDesignerViewModel));
            Assert.AreEqual("Assign (0)", displayName);
            //------------Execute Test---------------------------
            dsfMultiAssignActivityViewModel.UpdateDisplayName();
            //------------Assert Results-------------------------
            displayName = dsfMultiAssignActivityViewModel.ModelItem.GetProperty<string>("DisplayName");
            Assert.AreEqual("Assign (1)", displayName);

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
        [Owner("Pieter Terblanche")]
        [TestCategory("MultiAssignActivityViewModel_Handle")]
        public void MultiAssignActivityViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateDsfMultiAssignActivityViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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
