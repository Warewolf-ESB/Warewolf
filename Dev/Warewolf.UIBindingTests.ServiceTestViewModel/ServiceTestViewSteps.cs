using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.ServiceTestViewModel
{
    [Binding]
    public sealed class ServiceTestViewSteps
    {
        private Mock<IContextualResourceModel> CreateMockResourceModel()
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            var resourceModel = moqModel.Object;
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            dataListViewModel.ScalarCollection.Add(new ScalarItemModel("msg", enDev2ColumnArgumentDirection.Output));
            dataListViewModel.WriteToResourceModel();
            return moqModel;
        }

        private IContextualResourceModel CreateResourceModel(bool isConnected = true)
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(isConnected);
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            return moqModel.Object;
        }

        [Given(@"I Open serviceTestview Then DesignSurface allow drop is false")]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void GivenIOpenServiceTestviewThenDesignSurfaceAllowDropIsFalse()
        {
            Utils.SetupResourceDictionary();
            var serviceTestView = new ServiceTestView();
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mockResourceModel = CreateMockResourceModel();
            mockResourceModel.Setup(model => model.Environment.ResourceRepository.DeleteResourceTest(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            mockResourceModel.Setup(model => model.ID).Returns(Guid.NewGuid);
            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            var testFrameworkViewModel = new Studio.ViewModels.ServiceTestViewModel(CreateResourceModel(), new SynchronousAsyncWorker(), new Mock<Caliburn.Micro.IEventAggregator>().Object, new Mock<IExternalProcessExecutor>().Object, mockWorkflowDesignerViewModel.Object);
            serviceTestView.DataContext = testFrameworkViewModel;
            Utils.ShowTheViewForTesting(serviceTestView);
            var grid = serviceTestView.Content as Grid;
            var grid1 = grid.Children[1] as Grid;
            var border = grid1.Children[1] as Border;
            var grid2 = border.Child as Grid;
            var contentControl = grid2.Children[4] as ContentControl;
            var allowDrop = contentControl.AllowDrop;
            Assert.IsFalse(allowDrop);
        }
       
    }
}
