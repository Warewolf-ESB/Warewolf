using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using Caliburn.Micro;
using Dev2.Collections;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Composition;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Workspaces;
using Dev2.Webs.Callbacks;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceCallbackHandlerTests
    {
        static ImportServiceContext _importContext;

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _importContext = new ImportServiceContext();
            ImportService.CurrentContext = _importContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            var workspace = new Mock<IWorkspaceItemRepository>();
            ImportService.AddExportedValueToContainer(workspace.Object);

        }

        [TestInitialize]
        public void TestInitialize()
        {
            ImportService.CurrentContext = _importContext;
        }

        #endregion

        #region Save

        [TestMethod]
        public void ServiceCallbackHandlerSaveWithValidArgsExpectedPublishesUpdateResourceMessage()
        {
            Guid resourceID = Guid.NewGuid();

            var showDependencyProvider = new Mock<IShowDependencyProvider>();

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("Some Testing Display Name");
            resourceModel.Setup(r => r.ID).Returns(resourceID);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true))
                .Returns(new List<IResourceModel> { resourceModel.Object });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            aggregator.Setup(e => e.Publish(It.IsAny<UpdateResourceMessage>()))
                .Callback<Object>(m =>
                {
                    var msg = (UpdateResourceMessage)m;
                    Assert.AreEqual(resourceID, msg.ResourceModel.ID);
                })
                .Verifiable();

            var jsonObj = JObject.Parse("{ 'ResourceID': '" + resourceID + "','ResourceType':'Service'}");
            handler.TestSave(envModel.Object, jsonObj);

            aggregator.Verify(e => e.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
        }

        [TestMethod]
        public void ServiceCallbackHandlerSaveWithValidArgsWithMessages()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID);

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "Some Testing Dependant" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var jsonObj = JObject.Parse("{ 'ResourceID': '" + resourceID + "','ResourceType':'Service'}");
            //------------------------------Execute -------------------------------------------------
            handler.TestSave(envModel.Object, jsonObj);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), It.IsAny<IList<string>>()), Times.Once());
        }

        static Mock<IEnvironmentConnection> SetupConnectionWithCompileMessageList(List<CompileMessageTO> compileMessageTos, List<string> deps)
        {
            CompileMessageList compileMessageList = new CompileMessageList { MessageList = compileMessageTos, Dependants = deps };
            string serializeObject = JsonConvert.SerializeObject(compileMessageList);
            var envConnection = new Mock<IEnvironmentConnection>();
            envConnection.Setup(e => e.IsConnected).Returns(true);
            envConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            envConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(serializeObject));
            return envConnection;
        }

        [TestMethod]
        public void ServiceCallbackHandlerSaveWithValidArgsWithNoMessages()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID);

            var envConnection = SetupConnectionWithCompileMessageList(new List<CompileMessageTO>(), new List<string>());

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);
            var jsonObj = JObject.Parse("{ 'ResourceID': '" + resourceID + "','ResourceType':'Service'}");
            //------------------------------Execute -------------------------------------------------
            handler.TestSave(envModel.Object, jsonObj);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }), Times.Never());
        }

        static void SetupObjects(out Mock<IShowDependencyProvider> showDependencyProvider, out Mock<IResourceRepository> resourceRepo, Guid resourceID, ResourceType type = ResourceType.WorkflowService)
        {
            showDependencyProvider = new Mock<IShowDependencyProvider>();

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(r => r.ID).Returns(resourceID);
            resourceModel.Setup(r => r.ResourceName).Returns("Some Test Display Name");
            resourceModel.Setup(r => r.ResourceType).Returns(type);
            resourceModel.Setup(r => r.Errors).Returns(new ObservableReadOnlyList<IErrorInfo>());

            resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true))
                .Returns(new List<IResourceModel> { resourceModel.Object });
            resourceRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false))
                .Returns(resourceModel.Object);
        }

        [TestMethod]
        [TestCategory("ServiceCallbackHandler_CheckForServerMessages")]
        [Description("ServiceCallbackHandler CheckForServerMessages does not notify of change if the only effected resource is currently open")]
        [Owner("Ashley Lewis")]
        public void ServiceCallbackHandler_CheckForServerMessages_OnlyDependantIsInWorkspaceItemRepo_DontShowDependancyViewer()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID);

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "Unsaved 1" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var workspace = new Mock<IWorkspaceItemRepository>();
            var workspaceItem = new WorkspaceItem(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()) { ServiceName = "Unsaved 1" };
            workspace.Setup(c => c.WorkspaceItems).Returns(new List<IWorkspaceItem> { workspaceItem });

            //------------------------------Execute -------------------------------------------------
            handler.TestCheckForServerMessages(envModel.Object, resourceID, workspace.Object);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }), Times.Never());
        }

        [TestMethod]
        [TestCategory("ServiceCallbackHandler_Save")]
        [Description("ServiceCallbackHandler Save does notify of change if resource has more than one dependant")]
        [Owner("Ashley Lewis")]
        public void ServiceCallbackHandler_Save_ManyDependants_ShowDependancyViewer()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID);

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "Unsaved 1", "Another Testing Dependant" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var jsonObj = JObject.Parse("{ 'ResourceID': '" + resourceID + "','ResourceType':'Service'}");
            //------------------------------Execute -------------------------------------------------
            handler.TestSave(envModel.Object, jsonObj);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), It.IsAny<IList<string>>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ServiceCallbackHandler_CheckForServerMessages")]
        [Description("ServiceCallbackHandler CheckForServerMessages does not show dependancy viewer if resource type is db source")]
        [Owner("Ashley Lewis")]
        public void ServiceCallbackHandler_CheckForServerMessages_DbSource_DoNotShowDependancyViewer()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID, ResourceType.Source);
            showDependencyProvider.Setup(
                c => c.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }));

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "CheckForServerMessages 1", "Another Testing Dependant" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var workspace = new Mock<IWorkspaceItemRepository>();
            workspace.Setup(c => c.WorkspaceItems).Returns(new List<IWorkspaceItem>());

            //------------------------------Execute -------------------------------------------------
            handler.TestCheckForServerMessages(envModel.Object, resourceID, workspace.Object);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }), Times.Never());
        }

        [TestMethod]
        [TestCategory("ServiceCallbackHandler_CheckForServerMessages")]
        [Description("ServiceCallbackHandler CheckForServerMessages does not show dependancy viewer if resource type is plugin source")]
        [Owner("Ashley Lewis")]
        public void ServiceCallbackHandler_CheckForServerMessages_PluginSource_DoNotShowDependancyViewer()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID, ResourceType.Source);
            showDependencyProvider.Setup(
                c => c.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }));

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "CheckForServerMessages 1", "Another Testing Dependant" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var workspace = new Mock<IWorkspaceItemRepository>();
            workspace.Setup(c => c.WorkspaceItems).Returns(new List<IWorkspaceItem>());

            //------------------------------Execute -------------------------------------------------
            handler.TestCheckForServerMessages(envModel.Object, resourceID, workspace.Object);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }), Times.Never());
        }

        [TestMethod]
        [TestCategory("ServiceCallbackHandler_CheckForServerMessages")]
        [Description("ServiceCallbackHandler CheckForServerMessages does not show dependancy viewer if resource type is web source")]
        [Owner("Ashley Lewis")]
        public void ServiceCallbackHandler_CheckForServerMessages_WebSource_DoNotShowDependancyViewer()
        {
            //------------------------------Setup-------------------------------------------------
            Mock<IShowDependencyProvider> showDependencyProvider;
            Mock<IResourceRepository> resourceRepo;
            Guid resourceID = Guid.NewGuid();
            SetupObjects(out showDependencyProvider, out resourceRepo, resourceID, ResourceType.Source);
            showDependencyProvider.Setup(
                c => c.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }));

            var compileMessageTos = new List<CompileMessageTO> { new CompileMessageTO() };

            var envConnection = SetupConnectionWithCompileMessageList(compileMessageTos, new List<string> { "CheckForServerMessages 1", "Another Testing Dependant" });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var aggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new ServiceCallbackHandlerMock(aggregator.Object, envRepo.Object, showDependencyProvider.Object);

            var workspace = new Mock<IWorkspaceItemRepository>();
            workspace.Setup(c => c.WorkspaceItems).Returns(new List<IWorkspaceItem>());

            //------------------------------Execute -------------------------------------------------
            handler.TestCheckForServerMessages(envModel.Object, resourceID, workspace.Object);
            //------------------------------Assert Result -------------------------------------------------
            showDependencyProvider.Verify(provider => provider.ShowDependencyViewer(It.IsAny<IContextualResourceModel>(), new List<string> { "" }), Times.Never());
        }

        #endregion
    }
}