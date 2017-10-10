using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Data.Interfaces.Enums;
using Dev2.Core.Tests.Workflows;
using Dev2.Core.Tests;

namespace Warewolf.UIBindingTests.WorkflowDesigner
{
    [TestClass]
    public class Workflow_Designer
    {
        private Mock<IShellViewModel> _shellViewModelMock;

        [TestInitialize]
        public void Init()
        {
            var serverRepo = new Mock<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_DebugInputsCommand")]
        public void WorkflowDesignerViewModel_DebugInputsCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            _shellViewModelMock = new Mock<IShellViewModel>();

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DebugInputsCommand.Execute(null);
            Assert.IsTrue(viewModel.DebugInputsCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_DebugStudioCommand")]
        public void WorkflowDesignerViewModel_DebugStudioCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DebugStudioCommand.Execute(null);
            Assert.IsTrue(viewModel.DebugStudioCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_DebugBrowserCommand")]
        public void WorkflowDesignerViewModel_DebugBrowserCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DebugBrowserCommand.Execute(null);
            Assert.IsTrue(viewModel.DebugBrowserCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_ScheduleCommand")]
        public void WorkflowDesignerViewModel_ScheduleCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.ScheduleCommand.Execute(null);
            Assert.IsTrue(viewModel.ScheduleCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_TestEditorCommand")]
        public void WorkflowDesignerViewModel_TestEditorCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.TestEditorCommand.Execute(null);
            Assert.IsTrue(viewModel.TestEditorCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_RunAllTestsCommand")]
        public void WorkflowDesignerViewModel_RunAllTestsCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.RunAllTestsCommand.Execute(null);
            Assert.IsTrue(viewModel.RunAllTestsCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_DuplicateCommand")]
        public void WorkflowDesignerViewModel_DuplicateCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DuplicateCommand.Execute(null);
            Assert.IsTrue(viewModel.DuplicateCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_DeployCommand")]
        public void WorkflowDesignerViewModel_DeployCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DeployCommand.Execute(null);
            Assert.IsTrue(viewModel.DeployCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_ShowDependenciesCommand")]
        public void WorkflowDesignerViewModel_ShowDependenciesCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.ShowDependenciesCommand.Execute(null);
            Assert.IsTrue(viewModel.ShowDependenciesCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_ViewSwaggerCommand")]
        public void WorkflowDesignerViewModel_ViewSwaggerCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion
            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.ViewSwaggerCommand.Execute(null);
            Assert.IsTrue(viewModel.ViewSwaggerCommand.CanExecute(null));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerModel_CopyUrlCommand")]
        public void WorkflowDesignerViewModel_CopyUrlCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.CopyUrlCommand.Execute(null);
            Assert.IsTrue(viewModel.ViewSwaggerCommand.CanExecute(null));
            //------------Assert Results-------------------------
            var workflowLink = viewModel.GetWorkflowLink(false);
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            viewModel.OpenWorkflowLinkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?<DataList></DataList>", workflowLink);
        }

        #region Debug Selection Changed

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_NullDebugState_DoesNothing()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "SelectionChangedTest1", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 7");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            //----------------------- Execute -----------------------//
            EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = null, SelectionType = ActivitySelectionType.Single });

            var result = viewModel.BringIntoViewHitCount;

            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.AreEqual(0, result);
        }



        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionItemNotFound_SelectsFlowchart()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(Flowchart), false);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionItemFound_SelectsModelItem()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_SingleSelectionDecisionOrSwitchItemFound_SelectsDecisionOrSwitch()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Single, typeof(FlowDecision));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_AddSelection_SelectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Add, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_RemoveSelection_SelectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.Remove, typeof(TestActivity));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_DebugSelectionChanged")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_DebugSelectionChanged_ClearSelection_DeselectsItems()
        {
            Verify_DebugSelectionChanged(ActivitySelectionType.None, typeof(TestActivity));
        }

        static void Verify_DebugSelectionChanged(ActivitySelectionType selectionType, Type selectedActivityType, bool selectsModelItem = true)
        {
            //----------------------- Setup -----------------------//
            var ID = Guid.NewGuid();
            var states = new List<IDebugState> { new DebugState { DisplayName = "SelectionChangedTest1", ID = ID, WorkSurfaceMappingId = ID } };
            ID = Guid.NewGuid();
            if (selectionType == ActivitySelectionType.Add || selectionType == ActivitySelectionType.Remove)
            {

                states.Add(new DebugState { DisplayName = "SelectionChangedTest2", ID = ID, WorkSurfaceMappingId = ID });
            }

            #region Setup workflow

            FlowNode prevNode = null;

            var nodes = new List<FlowNode>();
            foreach (var node in states.Select(state => CreateFlowNode(state.ID, state.DisplayName, selectsModelItem, selectedActivityType)))
            {
                if (prevNode != null)
                {
                    if (prevNode is FlowStep flowStep)
                    {
                        flowStep.Next = node;
                    }
                }
                nodes.Add(node);
                prevNode = node;
            }

            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = nodes[0]
                }
            };

            #endregion

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            //----------------------- Execute -----------------------//
            var i = 0;
            foreach (var debugState in states)
            {
                if (selectionType == ActivitySelectionType.None || selectionType == ActivitySelectionType.Remove)
                {
                    // Ensure we have something to clear/remove
                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = ActivitySelectionType.Add });

                    // Only issue change event after all have been added
                    if (++i == states.Count)
                    {
                        var selectionBefore = viewModel.Designer.Context.Items.GetValue<Selection>();
                        Assert.AreEqual(states.Count, selectionBefore.SelectionCount);

                        EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = selectionType });
                    }
                }
                else
                {
                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = debugState, SelectionType = selectionType });
                }
            }

            //----------------------- Assert -----------------------//

            var selection = viewModel.Designer.Context.Items.GetValue<Selection>();

            switch (selectionType)
            {
                case ActivitySelectionType.None:
                    Assert.AreEqual(0, selection.SelectionCount);
                    Assert.AreEqual(1, viewModel.BringIntoViewHitCount); // 1 because we had to add something first!
                    Assert.AreEqual(0, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Single:
                    Assert.AreEqual(1, selection.SelectionCount);
                    Assert.AreEqual(1, viewModel.BringIntoViewHitCount);
                    Assert.AreEqual(1, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Add:
                    Assert.AreEqual(2, selection.SelectionCount);
                    Assert.AreEqual(2, viewModel.BringIntoViewHitCount);
                    Assert.AreEqual(2, viewModel.SelectedDebugModelItems.Count);
                    break;

                case ActivitySelectionType.Remove:
                    Assert.AreEqual(2, selection.SelectionCount);
                    Assert.AreEqual(2, viewModel.BringIntoViewHitCount); // 2 because we had to add something first!
                    Assert.AreEqual(1, viewModel.SelectedDebugModelItems.Count);
                    break;
                default:
                    break;
            }

            foreach (var modelItem in selection.SelectedObjects)
            {
                Assert.AreEqual(selectedActivityType, modelItem.ItemType);
                if (selectsModelItem)
                {
                    var actualID = selectedActivityType == typeof(FlowDecision)
                        ? Guid.Parse(((TestDecisionActivity)modelItem.GetProperty("Condition")).UniqueID)
                        : ModelItemUtils.GetUniqueID(modelItem);

                    var actualState = states.FirstOrDefault(s => s.ID == actualID);
                    Assert.IsNotNull(actualState);
                }
            }

            viewModel.Dispose();
        }

        static FlowNode CreateFlowNode(Guid id, string displayName, bool selectsModelItem, Type activityType)
        {
            if (activityType == typeof(FlowDecision))
            {
                return new FlowDecision(new TestDecisionActivity
                {
                    DisplayName = displayName,
                    UniqueID = selectsModelItem ? id.ToString() : Guid.NewGuid().ToString()
                });
            }

            return new FlowStep
            {
                Action = new TestActivity
                {
                    DisplayName = displayName,
                    UniqueID = selectsModelItem ? id.ToString() : Guid.NewGuid().ToString()
                }
            };
        }

        #endregion

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerViewModel_InitializeViewModel")]
        public void WorkflowDesignerViewModel_InitializeViewModel()
        {
            var eventAggregator = new EventAggregator();

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            mockResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(WorkflowDesignerUnitTest.WorkflowXAMLForTest());

            var dataListViewModel = WorkflowDesignerUnitTest.CreateDataListViewModel(mockResourceModel, eventAggregator);
            var dataListItems = new OptomizedObservableCollection<IScalarItemModel>();
            var dataListItem = new ScalarItemModel("scalar1", enDev2ColumnArgumentDirection.Input);
            var secondDataListItem = new ScalarItemModel("scalar2", enDev2ColumnArgumentDirection.Input);

            dataListItems.Add(dataListItem);
            dataListItems.Add(secondDataListItem);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            dataListItems.ToList().ForEach(dataListViewModel.ScalarCollection.Add);
            dataListViewModel.RecsetCollection.Clear();
            WorkflowDesignerViewModel workflowDesigner = WorkflowDesignerUnitTest.CreateWorkflowDesignerViewModelWithDesignerAttributesInitialized(mockResourceModel.Object, eventAggregator);
            workflowDesigner.PopUp = mockPopUp.Object;

            Assert.IsFalse(workflowDesigner.CanCopyUrl);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.CopyUrlTooltip);

            Assert.IsFalse(workflowDesigner.CanViewSwagger);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.ViewSwaggerTooltip);

            Assert.IsFalse(workflowDesigner.CanShowDependencies);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.ShowDependenciesTooltip);

            Assert.IsFalse(workflowDesigner.CanDeploy);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.DeployTooltip);

            Assert.IsFalse(workflowDesigner.CanDuplicate);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.DuplicateTooltip);

            Assert.IsFalse(workflowDesigner.CanRunAllTests);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.RunAllTestsTooltip);

            Assert.IsFalse(workflowDesigner.CanCreateTest);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.CreateTestTooltip);

            Assert.IsFalse(workflowDesigner.CanCreateSchedule);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.ScheduleTooltip);

            Assert.IsFalse(workflowDesigner.CanDebugBrowser);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.DebugBrowserTooltip);

            Assert.IsFalse(workflowDesigner.CanDebugStudio);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.DebugStudioTooltip);

            Assert.IsFalse(workflowDesigner.CanDebugInputs);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip, workflowDesigner.DebugInputsTooltip);

            Assert.IsTrue(dataListViewModel.ScalarCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ScalarCollection[1].IsUsed);

            workflowDesigner.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            workflowDesigner.Dispose();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerViewModel_HandleMessage")]
        public void WorkflowDesignerViewModel_HandleMessage_EditActivity_NotNull()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 66");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var environmentRepository = WorkflowDesignerUnitTest.SetupEnvironmentRepo(Guid.Empty); // Set the active environment

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowStep));

            #endregion

            var message = new EditActivityMessage(source.Object, Guid.NewGuid());

            //------------Execute Test---------------------------
            viewModel.Handle(message);

            //------------Assert Results-------------------------
            Assert.IsNotNull(message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerViewModel_BuildWorkflowFields")]
        public void WorkflowDesignerViewModel_BuildWorkflowFields_GetWorkflowFieldsFromFlowNodes_WithAction()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IWorkflowHelper> mockWorkflowHelper = new Mock<IWorkflowHelper>();
            Mock<IServer> mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            Guid envId2 = Guid.NewGuid();
            Mock<IServer> mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.EnvironmentID).Returns(envId2);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 66");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var environmentRepository = WorkflowDesignerUnitTest.SetupEnvironmentRepo(Guid.Empty); // Set the active environment

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Action", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Action", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowNode));

            var source1 = new Mock<ModelItem>();
            source1.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source1.Setup(s => s.ItemType).Returns(typeof(FlowNode));

            var source2 = new Mock<ModelItem>();
            source2.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source2.Setup(s => s.ItemType).Returns(typeof(FlowNode));

            #endregion

            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);

            List<ModelItem> flowNodes = new List<ModelItem>();
            flowNodes.Add(source.Object);
            flowNodes.Add(source1.Object);
            flowNodes.Add(source2.Object);

            //------------Execute Test---------------------------
            testClass.SetupGetWorkflowFieldsFromFlowNodes(flowNodes);

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WorkflowDesignerViewModel_BuildWorkflowFields")]
        public void WorkflowDesignerViewModel_BuildWorkflowFields_GetWorkflowFieldsFromFlowNodes_NoAction()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            Mock<IContextualResourceModel> mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IWorkflowHelper> mockWorkflowHelper = new Mock<IWorkflowHelper>();
            Mock<IServer> mockEnv = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            Guid envId2 = Guid.NewGuid();
            Mock<IServer> mockEnv2 = Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, null);
            mockEnv.Setup(c => c.EnvironmentID).Returns(envId2);
            mockResourceModel.Setup(c => c.Environment).Returns(mockEnv.Object);

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 66");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            var environmentRepository = WorkflowDesignerUnitTest.SetupEnvironmentRepo(Guid.Empty); // Set the active environment

            #region setup Mock ModelItem

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = DsfActivityFactory.CreateDsfActivity(resourceModel.Object, new DsfActivity(), true, environmentRepository, true);

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.SetValue(It.IsAny<DsfActivity>())).Verifiable();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("NoAction", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "NoAction", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source.Setup(s => s.ItemType).Returns(typeof(FlowNode));

            var source1 = new Mock<ModelItem>();
            source1.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source1.Setup(s => s.ItemType).Returns(typeof(FlowDecision));

            var source2 = new Mock<ModelItem>();
            source2.Setup(s => s.Properties).Returns(propertyCollection.Object);
            source2.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            #endregion

            var testClass = new WorkflowDesignerViewModelMock(mockResourceModel.Object, mockWorkflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);

            List<ModelItem> flowNodes = new List<ModelItem>();
            flowNodes.Add(source.Object);
            flowNodes.Add(source1.Object);
            flowNodes.Add(source2.Object);

            //------------Execute Test---------------------------
            testClass.SetupGetWorkflowFieldsFromFlowNodes(flowNodes);

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Trevor Williams-Ros")]
        public void WorkflowDesignerViewModel_CanSave_InvokesResourceModelIsAuthorizedForContribute()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 66");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            //----------------------- Execute -----------------------//
            var result = viewModel.CanSave;

            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.AreEqual(ExpectedCanSave, result);

            resourceModel.Verify(m => m.IsAuthorized(AuthorizationContext.Contribute));
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_GetsCommand()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 44");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());

            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            var expandAllCommand = viewModel.ExpandAllCommand;
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsNotNull(expandAllCommand);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_True_RequestExpandAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 332");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestExapandAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedExpandAll);
            viewModel.ExpandAllCommand.Execute(true);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedExpandAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_ExpandAllCommand_False_RequestRestoreAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 34");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestExapandAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedRestoreAll);
            viewModel.ExpandAllCommand.Execute(false);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedRestoreAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_CollapseAllCommand_True_RequestCollapseAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow 22");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestCollapseAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedCollapseAll);
            viewModel.CollapseAllCommand.Execute(true);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedCollapseAll);
        }

        [TestMethod]
        [TestCategory("WorkflowDesignerViewModel_CanSave")]
        [Owner("Hagashen Naidu")]
        public void WorkflowDesignerViewModel_CollapseAllCommand_False_RequestRestoreAll()
        {
            //----------------------- Setup -----------------------//
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.ResourceName).Returns("Some workflow");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            viewModel.SetupRequestCollapseAll();
            viewModel.SetupRequestRestoreAll();
            #endregion

            const bool ExpectedCanSave = true;
            resourceModel.Setup(m => m.IsAuthorized(AuthorizationContext.Contribute)).Returns(ExpectedCanSave).Verifiable();

            //----------------------- Execute -----------------------//
            Assert.IsFalse(viewModel.RequestedRestoreAll);
            viewModel.CollapseAllCommand.Execute(false);
            viewModel.Dispose();

            //----------------------- Assert -----------------------//
            Assert.IsTrue(viewModel.RequestedRestoreAll);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_DataListNull_ShouldReturnUrlEmptyDataListPortion()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns("<DataList></DataList>");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?<DataList></DataList>&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?<DataList></DataList>", displayWorkflowLink);
            Assert.AreEqual(Visibility.Visible, viewModel.WorkflowLinkVisible);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_HasDataListNoInputs_ShouldReturnUrlEmptyDataListPortion()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var mockExtenalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExtenalProcessExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, mockExtenalProcessExecutor.Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            viewModel.OpenWorkflowLinkCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?<DataList></DataList>&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?<DataList></DataList>", displayWorkflowLink);
            mockExtenalProcessExecutor.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()));
            Assert.AreEqual(Visibility.Visible, viewModel.WorkflowLinkVisible);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_HasDataListHasInputs_ShouldReturnUrlWithDataListPortion()
        {
            //------------Setup for test--------------------------
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "");
            var settingPath = Path.Combine(appData, @"Local\Warewolf\DebugData\PersistSettings.dat");

            if (File.Exists(settingPath))
            {
                File.Delete(settingPath);
            }

            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var mockPopController = new Mock<IPopupController>();
            mockPopController.Setup(controller => controller.ShowNoInputsSelectedWhenClickLink()).Verifiable();
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, mockPopController.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            viewModel.OpenWorkflowLinkCommand.Execute("Do not perform action");
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?scalar1=&scalar2=&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?scalar1=&scalar2=", displayWorkflowLink);
            mockPopController.Verify(controller => controller.ShowNoInputsSelectedWhenClickLink(), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_LinkName_SavedDebugData_ShouldReturnUrlWithDataListUsingSavedData()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://myMachineName:3142"));
            var serverEvents = new Mock<IEventPublisher>();
            mockConnection.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(m => m.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.Setup(m => m.Environment.IsConnected).Returns(true);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(mockConnection.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(true);
            resourceModel.Setup(model => model.Category).Returns("myservice");
            resourceModel.Setup(model => model.ResourceName).Returns("myservice");
            resourceModel.Setup(model => model.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var workflowInputDataViewModel = WorkflowInputDataViewModel.Create(resourceModel.Object);
            workflowInputDataViewModel.LoadWorkflowInputs();
            workflowInputDataViewModel.WorkflowInputs[0].Value = "1";
            workflowInputDataViewModel.WorkflowInputs[1].Value = "2";
            workflowInputDataViewModel.DoSaveActions();
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);
            workflowHelper.Setup(helper => helper.SerializeWorkflow(It.IsAny<ModelService>())).Returns(new StringBuilder("my workflow"));
            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);

            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            var workflowLink = viewModel.GetWorkflowLink();
            var displayWorkflowLink = viewModel.DisplayWorkflowLink;
            //------------Assert Results-------------------------
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?scalar1=1&scalar2=2&wid=00000000-0000-0000-0000-000000000000", workflowLink);
            Assert.AreEqual("http://mymachinename:3142/secure/myservice.json?scalar1=1&scalar2=2", displayWorkflowLink);
            workflowInputDataViewModel.WorkflowInputs[0].Value = "";
            workflowInputDataViewModel.WorkflowInputs[1].Value = "";
            workflowInputDataViewModel.DoSaveActions();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowDesignerModel_DoWorkspaceSave")]
        public void WorkflowDesignerViewModel_DoWorkspaceSave_NotNewResourceModel_ShouldCallSave()
        {
            //------------Setup for test--------------------------
            var workflow = new ActivityBuilder
            {
                Implementation = new Flowchart
                {
                    StartNode = CreateFlowNode(Guid.NewGuid(), "CanSaveTest", true, typeof(TestActivity))
                }
            };

            #region Setup viewModel

            var resourceRep = new Mock<IResourceRepository>();
            resourceRep.Setup(r => r.All()).Returns(new List<IResourceModel>());
            resourceRep.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRep.Setup(repository => repository.Save(It.IsAny<IResourceModel>())).Verifiable();
            var resourceModel = new Mock<IContextualResourceModel>();
            var envConn = new Mock<IEnvironmentConnection>();
            var serverEvents = new Mock<IEventPublisher>();
            envConn.Setup(m => m.ServerEvents).Returns(serverEvents.Object);
            resourceModel.Setup(m => m.Environment.Connection).Returns(envConn.Object);
            resourceModel.Setup(m => m.Environment.ResourceRepository).Returns(resourceRep.Object);
            resourceModel.Setup(model => model.IsNewWorkflow).Returns(false);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 9");
            var workflowHelper = new Mock<IWorkflowHelper>();
            workflowHelper.Setup(h => h.CreateWorkflow(It.IsAny<string>())).Returns(workflow);

            var viewModel = new WorkflowDesignerViewModelMock(resourceModel.Object, workflowHelper.Object, new Mock<IExternalProcessExecutor>().Object);
            viewModel.InitializeDesigner(new Dictionary<Type, Type>());
            resourceModel.SetupProperty(model => model.WorkflowXaml);
            #endregion

            //------------Assert Preconditions-------------------
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
            //------------Execute Test---------------------------
            viewModel.DoWorkspaceSave();
            //------------Assert Results-------------------------
            resourceRep.Verify(repository => repository.Save(It.IsAny<IResourceModel>()), Times.Never());
            Assert.IsNull(resourceModel.Object.WorkflowXaml);
        }
    }
}
