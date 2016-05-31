using System.Collections.Generic;
using System.Text;
using Caliburn.Micro;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests
{
    partial class DataListViewModelTests
    {

        #region Initialization



        void SetupForComplexObjects()
        {

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);



            _mockResourceModel = mockResourceModel;

            _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.ComplexObjectCollection.Clear();

            DataListSingleton.SetDataList(_dataListViewModel);
        }

        #endregion Initialize
      
    }
}
