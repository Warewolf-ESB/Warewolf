/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.WriteFileWithBase64;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;

namespace Dev2.Activities.Designers.Tests.WriteFileWithBase64
{
    [TestClass]
    
    public class WriteFileActivityDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileActivityDesignerViewModel))]
        public void WriteFileActivityDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = WriteFileActivityViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File Name", viewModel.OutputPathLabel);
            Assert.AreEqual("", viewModel.InputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileActivityDesignerViewModel))]
        public void WriteFileActivityDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = WriteFileActivityViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileActivityDesignerViewModel))]
        public void WriteFileActivityDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);
            var viewModel = WriteFileActivityViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileActivityDesignerViewModel))]
        public void WriteFileActivityDesignerViewModel_Contructor_OverwriteIsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = WriteFileActivityViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Overwrite);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileActivityDesignerViewModel))]
        public void WriteFileActivityDesignerViewModel_FileContentsAsBase64_IsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = new TestWriteFileActivityDesignerViewModel(ModelItemUtils.CreateModelItem(new FileWriteActivity
            {
                FileContentsAsBase64 = true
            }));
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.FileContentsAsBase64);
        }

        static TestWriteFileActivityDesignerViewModel WriteFileActivityViewModel()
        {
            var viewModel = new TestWriteFileActivityDesignerViewModel(ModelItemUtils.CreateModelItem(new FileWriteActivity()));
            return viewModel;
        }
    }
}
