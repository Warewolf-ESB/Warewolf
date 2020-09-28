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
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.WriteFileWithBase64
{
    [TestClass]
    
    public class WriteFileWithBase64DesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WriteFileWithBase64DesignerViewModel))]
        public void WriteFileWithBase64DesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = WriteFileWithBasse64ViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File Name", viewModel.OutputPathLabel);
            Assert.AreEqual("", viewModel.InputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WriteFileWithBase64DesignerViewModel))]
        public void WriteFileWithBase64DesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = WriteFileWithBasse64ViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WriteFileWithBase64DesignerViewModel))]
        public void WriteFileWithBase64DesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);
            var viewModel = WriteFileWithBasse64ViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory(nameof(WriteFileWithBase64DesignerViewModel))]
        public void WriteFileWithBase64DesignerViewModel_Contructor_OverwriteIsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = WriteFileWithBasse64ViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Overwrite);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WriteFileWithBase64DesignerViewModel))]
        public void WriteFileWithBase64DesignerViewModel_FileContentsAsBase64_IsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = new TestWriteFileWithBase64DesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileWriteWithBase64
            {
                FileContentsAsBase64 = true
            }));
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.FileContentsAsBase64);
        }

        static TestWriteFileWithBase64DesignerViewModel WriteFileWithBasse64ViewModel()
        {
            var viewModel = new TestWriteFileWithBase64DesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileWriteWithBase64()));
            return viewModel;
        }
    }
}
