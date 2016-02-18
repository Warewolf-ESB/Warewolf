using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class RequestServiceNameViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_CreateAsync")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequestServiceNameViewModel_CreateAsync_NullParameters_ShouldError()
        {
            //------------Setup for test--------------------------
            

            //------------Execute Test---------------------------
            RequestServiceNameViewModel.CreateAsync(null, "", "");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_CreateAsync")]
        public async Task RequestServiceNameViewModel_CreateAsync_ParametersPassed_ShouldConstructCorrectly()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var requestServiceNameViewModel = await RequestServiceNameViewModel.CreateAsync(new Mock<IEnvironmentViewModel>().Object, "", "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(requestServiceNameViewModel);
            Assert.IsNotNull(requestServiceNameViewModel.OkCommand);
            Assert.IsNotNull(requestServiceNameViewModel.CancelCommand);
            Assert.AreEqual("",requestServiceNameViewModel.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ShowSaveDialog")]
        public async Task RequestServiceNameViewModel_ShowSaveDialog_NoItemSelected_ShouldReturnResourceNameNoPath()
        {
            //------------Setup for test--------------------------
            var mockRequestServiceNameView = new Mock<IRequestServiceNameView>();
            CustomContainer.RegisterInstancePerRequestType<IRequestServiceNameView>(()=> mockRequestServiceNameView.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentViewModel>();
            var requestServiceNameViewModel = await RequestServiceNameViewModel.CreateAsync(mockEnvironmentModel.Object, "", "");
            requestServiceNameViewModel.ShowSaveDialog();
            requestServiceNameViewModel.Name = "TestResource";
            //------------Execute Test---------------------------
            requestServiceNameViewModel.OkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(requestServiceNameViewModel.ResourceName);
            Assert.AreEqual("",requestServiceNameViewModel.ResourceName.Path);
            Assert.AreEqual("TestResource", requestServiceNameViewModel.ResourceName.Name);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ShowSaveDialog")]
        public async Task RequestServiceNameViewModel_ShowSaveDialog_ItemSelected_ShouldReturnResourceNameWithPath()
        {
            //------------Setup for test--------------------------
            var mockRequestServiceNameView = new Mock<IRequestServiceNameView>();
            CustomContainer.RegisterInstancePerRequestType<IRequestServiceNameView>(()=> mockRequestServiceNameView.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentViewModel>();
            var requestServiceNameViewModel = await RequestServiceNameViewModel.CreateAsync(mockEnvironmentModel.Object, "", "");
            requestServiceNameViewModel.ShowSaveDialog();
            requestServiceNameViewModel.Name = "TestResource";
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
            mockExplorerTreeItem.Setup(item => item.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            mockExplorerTreeItem.Setup(item => item.ResourceName).Returns("MyFolder");
            requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerTreeItem.Object;
            //------------Execute Test---------------------------
            requestServiceNameViewModel.OkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(requestServiceNameViewModel.ResourceName);
            Assert.AreEqual("MyFolder\\",requestServiceNameViewModel.ResourceName.Path);
            Assert.AreEqual("TestResource", requestServiceNameViewModel.ResourceName.Name);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ShowSaveDialog")]
        public async Task RequestServiceNameViewModel_ShowSaveDialog_ItemSelectedHasParent_ShouldReturnResourceNameWithPathWithParent()
        {
            //------------Setup for test--------------------------
            var mockRequestServiceNameView = new Mock<IRequestServiceNameView>();
            CustomContainer.RegisterInstancePerRequestType<IRequestServiceNameView>(()=> mockRequestServiceNameView.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentViewModel>();
            var requestServiceNameViewModel = await RequestServiceNameViewModel.CreateAsync(mockEnvironmentModel.Object, "", "");
            requestServiceNameViewModel.ShowSaveDialog();
            requestServiceNameViewModel.Name = "TestResource";
            var mockExplorerParentTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerParentTreeItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
            mockExplorerParentTreeItem.Setup(item => item.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            mockExplorerParentTreeItem.Setup(item => item.ResourceName).Returns("MyParentFolder");

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
            mockExplorerTreeItem.Setup(item => item.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            mockExplorerTreeItem.Setup(item => item.ResourceName).Returns("MyFolder");
            mockExplorerTreeItem.Setup(item => item.Parent).Returns(mockExplorerParentTreeItem.Object);
            
            requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerTreeItem.Object;
            //------------Execute Test---------------------------
            requestServiceNameViewModel.OkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(requestServiceNameViewModel.ResourceName);
            Assert.AreEqual("MyParentFolder\\MyFolder\\",requestServiceNameViewModel.ResourceName.Path);
            Assert.AreEqual("TestResource", requestServiceNameViewModel.ResourceName.Name);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ShowSaveDialog")]
        public async Task RequestServiceNameViewModel_ShowSaveDialog_ItemSelectedHasDuplicateName_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            const string expectedErrorMessage = "An item with name \'TestResource\' already exists in this folder.";
            var mockRequestServiceNameView = new Mock<IRequestServiceNameView>();
            CustomContainer.RegisterInstancePerRequestType<IRequestServiceNameView>(() => mockRequestServiceNameView.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentViewModel>();
            var requestServiceNameViewModel = await RequestServiceNameViewModel.CreateAsync(mockEnvironmentModel.Object, "", "");
            requestServiceNameViewModel.ShowSaveDialog();
            
            
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
            
            mockExplorerTreeItem.Setup(item => item.ResourceName).Returns("MyFolder");

            var childDuplicateExplorerTreeItem = new Mock<IExplorerItemViewModel>();
            childDuplicateExplorerTreeItem.Setup(item => item.ResourceType).Returns(ResourceType.DbService);
            childDuplicateExplorerTreeItem.Setup(item => item.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            childDuplicateExplorerTreeItem.Setup(item => item.ResourceName).Returns("TestResource");
            childDuplicateExplorerTreeItem.Setup(model => model.Parent).Returns(mockExplorerTreeItem.Object);

            var explorerItemViewModels = new ObservableCollection<IExplorerItemViewModel> {childDuplicateExplorerTreeItem.Object };
            mockExplorerTreeItem.Setup(item => item.Children).Returns(explorerItemViewModels);
            requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.SelectedItem = mockExplorerTreeItem.Object;
            requestServiceNameViewModel.Name = "TestResource";
            //------------Execute Test---------------------------
            requestServiceNameViewModel.OkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedErrorMessage, requestServiceNameViewModel.ErrorMessage);
        }
        
    }
}