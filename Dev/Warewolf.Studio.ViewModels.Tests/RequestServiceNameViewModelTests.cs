using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class RequestServiceNameViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullEnvironment_ArgumentNullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new RequestServiceNameViewModel(null,new Mock<IRequestServiceNameView>().Object,Guid.Empty);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullView_ArgumentNullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object,null,Guid.Empty);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_InvalidCharactersName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = "Bad#$Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' contains invalid characters.", viewModel.ErrorMessage);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_ValidCharactersName_ShouldReturnNoErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = "Good Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("", viewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_NullName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = null;
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", viewModel.ErrorMessage);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_EmptyName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = "";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", viewModel.ErrorMessage);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_DuplicateServiceName_ShouldReturnErrorMessage()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerItemViewModelParent1 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent1.Setup(model => model.ResourceName).Returns("Parent 1");
            var mockExplorerItemViewModelParent2 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent2.Setup(model => model.ResourceName).Returns("Parent 2");
            mockExplorerItemViewModelParent1.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent2.Object);
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent1.Object);
            mockExplorerItemViewModel.Setup(model => model.ResourceName).Returns("Service 1");
            mockExplorerItemViewModel.Setup(model => model.ResourceType).Returns(ResourceType.WorkflowService);
            mockExplorerItemViewModelParent1.Setup(model => model.Children)
                .Returns(new List<IExplorerItemViewModel> {mockExplorerItemViewModel.Object});
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModelParent1.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.Name = "Service 1";
            //---------------Test Result -----------------------
            Assert.AreEqual("An item with name 'Service 1' already exists in this folder.", viewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_DuplicateServiceName_FolderMatch_ShouldNotReturnErrorMessage()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerItemViewModelParent1 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent1.Setup(model => model.ResourceName).Returns("Parent 1");
            var mockExplorerItemViewModelParent2 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent2.Setup(model => model.ResourceName).Returns("Parent 2");
            mockExplorerItemViewModelParent1.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent2.Object);
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent1.Object);
            mockExplorerItemViewModel.Setup(model => model.ResourceName).Returns("Service 1");
            mockExplorerItemViewModel.Setup(model => model.ResourceType).Returns(ResourceType.Folder);
            mockExplorerItemViewModelParent1.Setup(model => model.Children)
                .Returns(new List<IExplorerItemViewModel> {mockExplorerItemViewModel.Object});
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModelParent1.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.Name = "Service 1";
            //---------------Test Result -----------------------
            Assert.AreEqual("", viewModel.ErrorMessage);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Name")]
        public void Name_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Name")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.Name = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ErrorMessage")]
        public void ErrorMessage_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ErrorMessage")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.ErrorMessage = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Constructor")]
        public void Constructor_ShouldSetupOkCommand()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.OkCommand);

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_OkCommand")]
        public void OkCommand_CanExecute_HasErrorMessage_False()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = "Bad**Name";
            //------------Assert Results-------------------------
            var canExecute = viewModel.OkCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_OkCommand")]
        public void OkCommand_CanExecute_NoErrorMessage_True()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            //------------Execute Test---------------------------
            viewModel.Name = "Resource Name";
            //------------Assert Results-------------------------
            var canExecute = viewModel.OkCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ErrorMessage")]
        public void ErrorMessage_Set_ShouldRaiseCanExecuteChangeForOkCommand()
        {
            //------------Setup for test--------------------------
            var canExecuteChangedFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, new Mock<IRequestServiceNameView>().Object, Guid.Empty);
            viewModel.OkCommand.CanExecuteChanged += (sender, args) =>
            {
                canExecuteChangedFired = true;
            };
            //------------Assert Preconditions-------------------
            Assert.IsFalse(canExecuteChangedFired);
            //------------Execute Test---------------------------
            viewModel.ErrorMessage = "Some error message";
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecuteChangedFired);

        }

        [TestMethod]
        public void CancelCommand_WhenExecuted_ShouldCallRequestCloseOnView()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.CancelCommand.Execute(null);
            //---------------Test Result -----------------------
            mockView.Verify(view => view.RequestClose(),Times.Once());
        }

        [TestMethod]
        public void OkCommand_WhenExecutedWithNullParent_ShouldSetResourceNameWithEmptyPath()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns((IExplorerItemViewModel) null);
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModel.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.OkCommand.Execute(null);
            //---------------Test Result -----------------------
            Assert.AreEqual("",viewModel.ResourceName.Path);
            Assert.AreEqual(resourceName,viewModel.ResourceName.Name);
        }
        
        [TestMethod]
        public void OkCommand_WhenExecutedWithParents_ShouldSetResourceNameWithPathIncludingParentNames()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerItemViewModelParent1 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent1.Setup(model => model.ResourceName).Returns("Parent 1");
            var mockExplorerItemViewModelParent2 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent2.Setup(model => model.ResourceName).Returns("Parent 2");
            mockExplorerItemViewModelParent1.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent2.Object);
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent1.Object);
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModel.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.OkCommand.Execute(null);
            //---------------Test Result -----------------------
            Assert.AreEqual("Parent 2\\Parent 1\\",viewModel.ResourceName.Path);
            Assert.AreEqual(resourceName,viewModel.ResourceName.Name);
        }
        
        [TestMethod]
        public void OkCommand_WhenExecutedWithSelectedItemIsForlder_ShouldSetResourceNameWithPathSelectedItemResourceName()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerItemViewModelParent1 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent1.Setup(model => model.ResourceName).Returns("Parent 1");
            var mockExplorerItemViewModelParent2 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent2.Setup(model => model.ResourceName).Returns("Parent 2");
            mockExplorerItemViewModelParent1.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent2.Object);
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent1.Object);
            mockExplorerItemViewModel.Setup(model => model.ResourceName).Returns("SelectedItem");
            mockExplorerItemViewModel.Setup(model => model.ResourceType).Returns(ResourceType.Folder);
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModel.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.OkCommand.Execute(null);
            //---------------Test Result -----------------------
            Assert.AreEqual("Parent 2\\Parent 1\\SelectedItem\\",viewModel.ResourceName.Path);
            Assert.AreEqual(resourceName,viewModel.ResourceName.Name);
        }

        [TestMethod]
        public void OkCommand_WhenExecuted_ShouldSetViewResultToOK()
        {
            //---------------Set up test pack-------------------
            var mockView = new Mock<IRequestServiceNameView>();
            mockView.Setup(view => view.RequestClose()).Verifiable();
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, mockView.Object, Guid.Empty);
            const string resourceName = "Test";
            viewModel.Name = resourceName;
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerItemViewModelParent1 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent1.Setup(model => model.ResourceName).Returns("Parent 1");
            var mockExplorerItemViewModelParent2 = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModelParent2.Setup(model => model.ResourceName).Returns("Parent 2");
            mockExplorerItemViewModelParent1.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent2.Object);
            mockExplorerItemViewModel.Setup(model => model.Parent).Returns(mockExplorerItemViewModelParent1.Object);
            mockExplorerItemViewModel.Setup(model => model.ResourceName).Returns("SelectedItem");
            mockExplorerItemViewModel.Setup(model => model.ResourceType).Returns(ResourceType.Folder);
            viewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerItemViewModel.Object;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            viewModel.OkCommand.Execute(null);
            //---------------Test Result -----------------------
            Assert.AreEqual(MessageBoxResult.OK,viewModel.ViewResult);
            mockView.Verify(view => view.RequestClose(),Times.Once());
        }
    }
}
