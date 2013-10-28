using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;
using ResourceType = Dev2.Studio.Core.AppResources.Enums.ResourceType;

namespace Dev2.Core.Tests
{
    [TestClass][ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DragDropHelpersTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute));
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereNullDataObject_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }      
        
        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereNoFormats_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectNoFormats());
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereFormatsReturnsNotResourceTreeViewModelOrWorkflowItemTypeNameFormat_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormat(new []{"SomeText"}));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }


        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereGetDataReturnsNull_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatNoData(new []{"ResourceTreeViewModel","SomeText"}));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereGetDataReturnsNonContextualResourceModel_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            var data = new Object();
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereGetDataReturnsContextualResourceModelWorkflowService_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            var data = new Mock<IContextualResourceModel>();
            data.Setup(model => model.ResourceType).Returns(ResourceType.WorkflowService);
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data.Object));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereGetDataReturnsContextualResourceModelServerResourceTypeWorkflowService_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            var data = new Mock<IContextualResourceModel>();
            data.Setup(model => model.ServerResourceType).Returns(ResourceType.WorkflowService.ToString());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data.Object));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropWhereGetDataReturnsContextualResourceModelNotWorkflowServiceDataContextNotViewModel_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            object dataContext = new object();
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext));
            var data = new Mock<IContextualResourceModel>();
            data.Setup(model => model.ResourceType).Returns(ResourceType.WorkflowService);
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data.Object));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropDataContextViewModelSameEnviornmentID_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var environmentMock = new Mock<IEnvironmentModel>();
            Guid guid = Guid.NewGuid();
            environmentMock.Setup(model => model.ID).Returns(guid);
            var data = new Mock<IContextualResourceModel>();
            data.Setup(model => model.ResourceType).Returns(ResourceType.WorkflowService);
            data.Setup(model => model.Environment).Returns(environmentMock.Object);
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            dataContext.Setup(model => model.EnvironmentModel).Returns(environmentMock.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data.Object));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_UnitTest_PreventDropDataContextViewModelDifferentEnviornmentID_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var environmentMock = new Mock<IEnvironmentModel>();
            Guid guid = Guid.NewGuid();
            environmentMock.Setup(model => model.ID).Returns(guid);
            var data = new Mock<IContextualResourceModel>();
            data.Setup(model => model.ResourceType).Returns(ResourceType.Service);
            data.Setup(model => model.Environment).Returns(environmentMock.Object);
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.NewGuid());
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new []{"ResourceTreeViewModel"},data.Object));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        IDataObject GetMockDataObjectWithFormatData(string[] formats,object data)
        {
            var mock = new Mock<IDataObject>();
            mock.Setup(d => d.GetFormats()).Returns(formats);
            mock.Setup(d => d.GetData(It.IsAny<string>())).Returns(data);
            return mock.Object;
        }

        IDataObject GetMockDataObjectWithFormatNoData(string[] formats)
        {
            return GetMockDataObjectWithFormatData(formats, null);
        }

        IDataObject GetMockDataObjectWithFormat(string[] formats)
        {
            var mock = new Mock<IDataObject>();
            mock.Setup(d => d.GetFormats()).Returns(formats);
            return mock.Object;
        }

        IDataObject GetMockDataObjectNoFormats()
        {
            return GetMockDataObjectWithFormat(new string[0]);
        }

        IWorkflowDesignerView GetMockWorkflowDesignerView()
        {
            return new Mock<IWorkflowDesignerView>().Object;
        }
        
        IWorkflowDesignerView GetMockWorkflowDesignerView(object dataContext)
        {
            var mock = new Mock<IWorkflowDesignerView>();
            mock.Setup(view => view.DataContext).Returns(dataContext);
            return mock.Object;
        }
    }

}