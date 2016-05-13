
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Windows;
using Castle.DynamicProxy.Generators;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Views.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DragDropHelpersTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            AttributesToAvoidReplicating.Add(typeof(UIPermissionAttribute));
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_NullDataObject_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_NoFormats_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectNoFormats());
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_FormatsReturnsNotResourceTreeViewModelOrWorkflowItemTypeNameFormat_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormat(new[] { "SomeText" }));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }


        [TestMethod]
        public void DragDropHelpers_PreventDrop_GetDataReturnsNull_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatNoData(new[] { "ExplorerItemModel", "SomeText" }));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_GetDataReturnsNonExplorerItemModel_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var data = new Object();
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_GetDataReturnsExplorerItemModelWorkflowService_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "WorkflowService" };
            data.IsService = true;
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_LocalResourceOnRemoteDesignSurface_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "WorkflowService",EnvironmentId = Guid.NewGuid()};
            var dataContext = new Mock<IWorkflowDesignerViewModel>();

            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            differentEnvironment.Setup(a => a.IsLocalHost).Returns(false);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            CustomContainer.Register(new Mock<IPopupController>().Object);
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }
        [TestMethod]
        [Owner("Massimo.Guerrera")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_GetDataReturnsExplorerItemModelSource_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView());

            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbSource" };
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_GetDataReturnsExplorerItemModelServerResourceTypeWorkflowService_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var environmentMock = new Mock<IEnvironmentModel>();
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "WorkflowService", EnvironmentId = environmentMock.Object.ID };
            data.IsService = true;
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            dataContext.Setup(model => model.EnvironmentModel).Returns(environmentMock.Object);

            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));

            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_GetDataReturnsExplorerItemModelNotWorkflowServiceDataContextNotViewModel_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            object dataContext = new object();
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext));

            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbService" };
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_FormatOfWorkflowItemTypeNameFormat_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            object dataContext = new object();
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext));

            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbService" };
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "WorkflowItemTypeNameFormat" }, data));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_DataContextViewModelSameEnvironmentID_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var environmentMock = new Mock<IEnvironmentModel>();
            Guid guid = Guid.NewGuid();
            environmentMock.Setup(model => model.ID).Returns(guid);
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbService", EnvironmentId = environmentMock.Object.ID };
            data.IsService = true;
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            dataContext.Setup(model => model.EnvironmentModel).Returns(environmentMock.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        public void DragDropHelpers_PreventDrop_DataContextViewModelDifferentEnvironmentID_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var environmentMock = new Mock<IEnvironmentModel>();
            Guid guid = Guid.NewGuid();
            environmentMock.Setup(model => model.ID).Returns(guid);
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbService", EnvironmentId = environmentMock.Object.ID };
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.NewGuid());
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));
            //------------Execute Test---------------------------
            bool canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));
            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_WorkflowDesignerView_DataContextIsNotWorkflowDesignerViewModel_True()
        {
            //------------Setup for test--------------------------
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "DbService" };

            var dataContext = new object();
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext));

            //------------Execute Test---------------------------
            var canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));

            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_UserIsAuthorized_False()
        {
            //------------Setup for test--------------------------
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.Execute, ResourceType = "WorkflowService" };
            data.IsService = true;
            var dataContext = new Mock<IWorkflowDesignerViewModel>();
            var differentEnvironment = new Mock<IEnvironmentModel>();
            differentEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            dataContext.Setup(model => model.EnvironmentModel).Returns(differentEnvironment.Object);
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext.Object));

            //------------Execute Test---------------------------
            var canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));

            //------------Assert Results-------------------------
            Assert.IsFalse(canDoDrop);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DragDropHelpers_PreventDrop")]
        public void DragDropHelpers_PreventDrop_UserIsNotAuthorized_True()
        {
            //------------Setup for test--------------------------
            var data = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Permissions = Permissions.View, ResourceType = "WorkflowService" };

            var dataContext = new object();
            var dragDropHelpers = new DragDropHelpers(GetMockWorkflowDesignerView(dataContext));

            //------------Execute Test---------------------------
            var canDoDrop = dragDropHelpers.PreventDrop(GetMockDataObjectWithFormatData(new[] { "ExplorerItemModel" }, data));

            //------------Assert Results-------------------------
            Assert.IsTrue(canDoDrop);
        }

        IDataObject GetMockDataObjectWithFormatData(string[] formats, object data)
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
