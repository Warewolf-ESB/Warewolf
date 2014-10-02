
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract.Network;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Webs;
using Dev2.Utilities;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

#endregion

//using System.Windows.Media.Imaging;

namespace Dev2.Core.Tests
{
    /// <summary>
    ///     This is a result class for mvTest and is intended
    ///     to contain all mvTest Unit Tests
    /// </summary>
    [TestClass]
    public class mvTest
    {
        #region Variables

        private static ImportServiceContext _importServiceContext;
        private static readonly object syncroot = new object();
        private static readonly object monitorLock = new object();
        private readonly Guid _firstResourceID = Guid.NewGuid();
        private readonly Guid _secondResourceID = Guid.NewGuid();
        private readonly Guid _serverID = Guid.NewGuid();
        private readonly Guid _workspaceID = Guid.NewGuid();
        private string _displayName = "test2";
        private Mock<IEnvironmentConnection> _environmentConnection;
        private Mock<IEnvironmentModel> _environmentModel;
        private IEnvironmentRepository _environmentRepo;
        private Mock<IEventAggregator> _eventAggregator;
        private Mock<IFeedbackInvoker> _feedbackInvoker;
        private Mock<IContextualResourceModel> _firstResource;
        private MainViewModel _mainViewModel;
        private Mock<IWorkspaceItemRepository> _mockWorkspaceRepo;
        public Mock<IPopupController> _popupController;
        private Mock<IResourceDependencyService> _resourceDependencyService;
        private string _resourceName = "TestResource";
        private Mock<IResourceRepository> _resourceRepo;
        private Mock<IContextualResourceModel> _secondResource;
        private string _serviceDefinition = "<x/>";
        private Mock<IWebController> _webController;
        private Mock<IWindowManager> _windowManager;

        #endregion Variables

        #region init

        //Use TestInitialize to run code before running each result
        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(monitorLock);
        }

        //Use TestInitialize to run code before running each result
        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(monitorLock);
        }

        #endregion init

        [TestMethod]
        public void DeployCommandCanExecuteIrrespectiveOfEnvironments()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                Assert.IsTrue(_mainViewModel.DeployCommand.CanExecute(null));
            }
        }

        [TestMethod]
        public void SettingsCommandCanExecuteIrrespectiveOfEnvironments()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                Assert.IsTrue(_mainViewModel.SettingsCommand.CanExecute(null));
            }
        }

        [TestMethod]
        public void SettingsCommandCreatesSettingsWorkSurfaceContext()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _mainViewModel.SettingsCommand.Execute(null);
                var ctx = _mainViewModel.ActiveItem;
                Assert.IsTrue(ctx.WorkSurfaceKey.WorkSurfaceContext == WorkSurfaceContext.Settings);
            }
        }

        [TestMethod]
        public void IsActiveEnvironmentConnectExpectFalseWithNullEnvironment()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var actual = _mainViewModel.IsActiveEnvironmentConnected();
                Assert.IsTrue(actual == false);
            }
        }

        [TestMethod]
        public void ShowDependenciesMessageExpectsDependencyVisualizerWithResource()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var msg = new ShowDependenciesMessage(_firstResource.Object);
                _mainViewModel.Handle(msg);
                var ctx = _mainViewModel.ActiveItem;
                var vm = ctx.WorkSurfaceViewModel as DependencyVisualiserViewModel;
                Assert.IsTrue(vm.ResourceModel.Equals(_firstResource.Object));
            }
        }

        [TestMethod]
        public void ShowDependenciesMessageExpectsNothingWithNullResource()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var msg = new ShowDependenciesMessage(null);
                _mainViewModel.Handle(msg);
                Assert.IsTrue(
                    _mainViewModel.Items.All(
                        i => i.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.DependencyVisualiser));
            }
        }

        [TestMethod]
        public void ShowHelpTabMessageExpectHelpTabWithUriActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var msg = new ShowHelpTabMessage("testuri");
                _mainViewModel.Handle(msg);
                var helpctx = _mainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
                Assert.IsTrue(helpctx.Uri == "testuri");
            }
        }

        [TestMethod]
        public void DeactivateWithCloseExpectBuildWithEmptyDebugWriterWriteMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _eventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                               .Verifiable();

                _mainViewModel.Dispose();

                _eventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(1));
            }
        }

        [TestMethod]
        public void DeactivateWithCloseAndTwoTabsExpectBuildTwiceWithEmptyDebugWriterWriteMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _eventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                               .Verifiable();
                AddAdditionalContext();

                _mainViewModel.Dispose();

                _eventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(2));
            }
        }

        [TestMethod]
        public void
            CloseContextWithCloseTrueAndResourceSavedExpectsRemoveWorkspaceItemRemoveCalledAndTabClosedMessageAndContextRemoved
            ()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                Assert.IsTrue(_mainViewModel.Items.Count == 2);

                _firstResource.Setup(r => r.IsWorkflowSaved).Returns(true);
                var activetx =
                    _mainViewModel.Items.ToList()
                                  .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

                _eventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                                .Callback<object>((o =>
                                {
                                    var msg = (TabClosedMessage)o;
                                    Assert.IsTrue(msg.Context.Equals(activetx));
                                }));

                _mainViewModel.DeactivateItem(activetx, true);
                _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
                Assert.IsTrue(_mainViewModel.Items.Count == 1);
                _eventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
            }
        }

        [TestMethod]
        public void
            CloseContextWithCloseTrueAndResourceNotSavedPopupOkExpectsRemoveWorkspaceItemCalledAndContextRemovedAndSaveResourceEventAggregatorMessage
            ()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                Assert.IsTrue(_mainViewModel.Items.Count == 2);
                _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
                _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);

                var activetx =
                    _mainViewModel.Items.ToList()
                                  .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

                _eventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                                .Callback<object>((o =>
                                {
                                    var msg = (TabClosedMessage)o;
                                    Assert.IsTrue(msg.Context.Equals(activetx));
                                }));

                _eventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>()))
                                .Callback<object>((o =>
                                {
                                    var msg = (SaveResourceMessage)o;
                                    Assert.IsTrue(msg.Resource.Equals(_firstResource.Object));
                                }));

                _mainViewModel.DeactivateItem(activetx, true);
                _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
                Assert.IsTrue(_mainViewModel.Items.Count == 1);
                _eventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
                _eventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Once());
            }
        }

        [TestMethod]
        public void CloseContextWithCloseTrueAndResourceNotSavedPopupNotOkExpectsWorkspaceItemNotRemoved()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                Assert.IsTrue(_mainViewModel.Items.Count == 2);
                _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
                _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
                var activetx =
                    _mainViewModel.Items.ToList()
                                  .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
                _mainViewModel.DeactivateItem(activetx, false);
                _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
            }
        }

        [TestMethod]
        public void AdditionalWorksurfaceAddedExpectsLAstAddedTOBeActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                AddAdditionalContext();
                Assert.IsTrue(_mainViewModel.Items.Count == 3);
                var activeItem = _mainViewModel.ActiveItem;
                var secondKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, _secondResource.Object.ID,
                                                          _secondResource.Object.ServerID);
                Assert.IsTrue(activeItem.WorkSurfaceKey.ResourceID.Equals(secondKey.ResourceID) && activeItem.WorkSurfaceKey.ServerID.Equals(secondKey.ServerID));
            }
        }


        [TestMethod]
        public void CloseContextWithCloseFalseExpectsPreviousItemActivatedAndAllItemsPResent()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                AddAdditionalContext();
                Assert.IsTrue(_mainViewModel.Items.Count == 3);

                var firstCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
                var secondCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);

                _mainViewModel.ActivateItem(firstCtx);
                _mainViewModel.DeactivateItem(secondCtx, false);

                Assert.IsTrue(_mainViewModel.Items.Count == 3);
                Assert.IsTrue(_mainViewModel.ActiveItem.Equals(firstCtx));
            }
        }

        [TestMethod]
        public void CloseContextWithCloseTrueExpectsPreviousItemActivatedAndOneLessItem()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                AddAdditionalContext();
                Assert.IsTrue(_mainViewModel.Items.Count == 3);

                var firstCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
                var secondCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);

                _mainViewModel.ActivateItem(firstCtx);
                _mainViewModel.DeactivateItem(firstCtx, true);

                Assert.IsTrue(_mainViewModel.Items.Count == 2);
                Assert.IsTrue(_mainViewModel.ActiveItem.Equals(secondCtx));
            }
        }

        [TestMethod]
        public void CloseContextWithCloseFalseExpectsContextNotRemoved()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var activetx =
                    _mainViewModel.Items.ToList()
                                  .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
                _mainViewModel.DeactivateItem(activetx, false);
                _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
            }
        }

        #region Workspaces and init

        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void OnImportsSatisfiedWithNonStudioContextExpectsOnlyStartPage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _environmentModel.SetupGet(m => m.DsfChannel).Returns(new Mock<IStudioClientContext>().Object);
                _mainViewModel = new MainViewModel();

                Assert.IsTrue(_mainViewModel.Items.Count == 1);
            }
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsTwoItems()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                //One saved workspaceitem, one startpage
                Assert.IsTrue(_mainViewModel.Items.Count == 2);
            }
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsContextsAddedForSavedWorkspaces()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var activetx =
                    _mainViewModel.Items.ToList()
                                  .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
                var expectedKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, _firstResourceID,
                                                                  _serverID);
                Assert.IsTrue(expectedKey.ResourceID.Equals(activetx.WorkSurfaceKey.ResourceID) && expectedKey.ServerID.Equals(activetx.WorkSurfaceKey.ServerID));
            }
        }

        [TestMethod]
        [Ignore] // Mis match between active and first tab visible
        public void OnImportsSatisfiedExpectsStartpageActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var activetx = _mainViewModel.ActiveItem;
                Assert.IsTrue(activetx.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Help);
                var helpvm = activetx.WorkSurfaceViewModel as HelpViewModel;
                Assert.IsTrue(helpvm.Uri == FileHelper.GetFullPath(StringResources.Uri_Studio_Homepage));
            }
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsDisplayNameSet()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                const string expected = "Warewolf";
                Assert.AreEqual(expected, _mainViewModel.DisplayName);
            }
        }

        #endregion workspaces

        #region Methods used by tests

        private void CreateFullExportsAndVmWithEmptyRepo()
        {
            CreateResourceRepo();
            var securityContext = GetMockSecurityContext();
            var mockEnv = new Mock<IEnvironmentRepository>();
            mockEnv.Setup(g => g.All()).Returns(new List<IEnvironmentModel>());
            var environmentRepo = mockEnv.Object;
            var workspaceRepo = GetworkspaceItemRespository();
            _eventAggregator = new Mock<IEventAggregator>();
            _popupController = new Mock<IPopupController>();
            _resourceDependencyService = new Mock<IResourceDependencyService>();
            _importServiceContext =
                CompositionInitializer.InitializeMockedMainViewModel(securityContext: securityContext,
                                                                     environmentRepo: environmentRepo,
                                                                     workspaceItemRepository: workspaceRepo,
                                                                     aggregator: _eventAggregator,
                                                                     popupController: _popupController,
                                                                     resourceDepService: _resourceDependencyService);

            ImportService.CurrentContext = _importServiceContext;
            _mainViewModel = new MainViewModel(environmentRepo, false);
        }

        private void CreateFullExportsAndVm()
        {
            CreateResourceRepo();
            var securityContext = GetMockSecurityContext();
            var environmentRepo = GetEnvironmentRepository();
            var workspaceRepo = GetworkspaceItemRespository();
            _eventAggregator = new Mock<IEventAggregator>();
            _popupController = new Mock<IPopupController>();
            _feedbackInvoker = new Mock<IFeedbackInvoker>();
            _resourceDependencyService = new Mock<IResourceDependencyService>();
            _webController = new Mock<IWebController>();
            _windowManager = new Mock<IWindowManager>();
            _importServiceContext =
                CompositionInitializer.InitializeMockedMainViewModel(securityContext: securityContext,
                                                                     environmentRepo: environmentRepo,
                                                                     workspaceItemRepository: workspaceRepo,
                                                                     aggregator: _eventAggregator,
                                                                     popupController: _popupController,
                                                                     resourceDepService: _resourceDependencyService,
                                                                     feedbackInvoker: _feedbackInvoker,
                                                                     webController: _webController,
                                                                     windowManager: _windowManager);

            ImportService.CurrentContext = _importServiceContext;
            _mainViewModel = new MainViewModel(environmentRepo, false);
        }


        public Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.ServiceDefinition).Returns(_serviceDefinition);
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_firstResourceID);

            return result;
        }

        public Mock<IFrameworkSecurityContext> GetMockSecurityContext()
        {
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.Name).Returns("Test User");
            var mockContext = new Mock<IFrameworkSecurityContext>();
            mockContext.Setup(m => m.UserIdentity).Returns(mockIdentity.Object);
            return mockContext;
        }

        public IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        private void CreateResourceRepo()
        {
            _environmentModel = CreateMockEnvironment();

            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);

            var channel = new Mock<IStudioClientContext>();
            channel.SetupGet(c => c.WorkspaceID).Returns(_workspaceID);
            channel.SetupGet(c => c.ServerID).Returns(_serverID);

            _environmentModel.SetupGet(s => s.DsfChannel).Returns(channel.Object);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        public static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);

            var eventAggregator = new Mock<IEventAggregator>();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.Setup(c => c.AppServerUri)
                      .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri)
                      .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            connection.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));

            return connection;
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);
            var wizardEngine = new Mock<IWizardEngine>();

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.WizardEngine).Returns(wizardEngine.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));

            return env;
        }

        public Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            _mockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(_workspaceID);
            item.SetupGet(i => i.ServerID).Returns(_serverID);
            item.SetupGet(i => i.ServiceName).Returns(_resourceName);
            list.Add(item.Object);
            _mockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            _mockWorkspaceRepo.Setup(c => c.Remove(_firstResource.Object)).Verifiable();
            return _mockWorkspaceRepo;
        }

        private void AddAdditionalContext()
        {
            _secondResource = new Mock<IContextualResourceModel>();

            _secondResource.Setup(c => c.ResourceName).Returns("WhoCares");
            _secondResource.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            _secondResource.Setup(c => c.ServiceDefinition).Returns("");
            _secondResource.Setup(c => c.Category).Returns("Testing2");
            _secondResource.Setup(c => c.Environment).Returns(_environmentModel.Object);
            _secondResource.Setup(c => c.ServerID).Returns(_serverID);
            _secondResource.Setup(c => c.ID).Returns(_secondResourceID);

            var msg = new AddWorkSurfaceMessage(_secondResource.Object);
            _mainViewModel.Handle(msg);
        }

        private void SetupForDelete()
        {
            _popupController.Setup(c => c.Show()).Verifiable();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            _resourceDependencyService.Setup(s => s.HasDependencies(_firstResource.Object)).Returns(false).Verifiable();
            var succesResponse = new UnlimitedObject(@"<DataList>Success</DataList>");

            _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(succesResponse);
        }

        private Mock<IEnvironmentRepository> SetupForDeleteServer()
        {
            CreateResourceRepo();
            var securityContext = GetMockSecurityContext();
            var workspaceRepo = GetworkspaceItemRespository();
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.Get(It.IsAny<string>())).Returns(_environmentModel.Object);
            mock.Setup(s => s.Remove(It.IsAny<IEnvironmentModel>()))
                .Callback<IEnvironmentModel>(s =>
                                             Assert.AreEqual(_environmentModel.Object, s))
                .Verifiable();
            _popupController = new Mock<IPopupController>();
            _resourceDependencyService = new Mock<IResourceDependencyService>();
            _eventAggregator = new Mock<IEventAggregator>();
            _eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()))
                            .Callback<object>(m =>
                            {
                                var removeMsg = (EnvironmentDeletedMessage)m;
                                Assert.AreEqual(_environmentModel.Object, removeMsg.EnvironmentModel);
                            })
                            .Verifiable();

            _importServiceContext =
                CompositionInitializer.InitializeMockedMainViewModel(securityContext: securityContext,
                                                                     environmentRepo: mock.Object,
                                                                     workspaceItemRepository: workspaceRepo,
                                                                     popupController: _popupController,
                                                                     resourceDepService: _resourceDependencyService,
                                                                     aggregator: _eventAggregator);

            ImportService.CurrentContext = _importServiceContext;
            _mainViewModel = new MainViewModel(mock.Object, false);
            SetupForDelete();
            _firstResource.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            _firstResource.Setup(r => r.ServerResourceType).Returns("Server");
            _firstResource.Setup(r => r.ConnectionString)
                          .Returns(TestResourceStringsTest.ResourceToHydrateConnectionString1);
            _environmentConnection = new Mock<IEnvironmentConnection>();
            _environmentConnection.Setup(c => c.AppServerUri)
                                  .Returns(new Uri(TestResourceStringsTest.ResourceToHydrateActualAppUri));
            _environmentModel.Setup(r => r.Connection).Returns(_environmentConnection.Object);
            return mock;
        }

        #endregion Methods used by tests

        #region Commands

        [TestMethod]
        public void DisplayAboutDialogueCommandExpectsWindowManagerShowingIDialogueViewModel()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _windowManager.Setup(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null)).Verifiable();
                _mainViewModel.DisplayAboutDialogueCommand.Execute(null);
                _windowManager.Verify(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null), Times.Once());
            }
        }

        [TestMethod]
        public void AddStudioShortcutsPageCommandExpectsShortKeysActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _mainViewModel.AddStudioShortcutsPageCommand.Execute(null);
                var shortkeyUri = FileHelper.GetFullPath(StringResources.Uri_Studio_Shortcut_Keys_Document);
                var helpctx = _mainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
                Assert.IsTrue(helpctx.Uri == shortkeyUri);
            }
        }

        [TestMethod]
        public void SettingsSaveCancelMessageExpectsPreviousContextActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var datalistchannelmock = new Mock<INetworkDataListChannel>();
                datalistchannelmock.SetupGet(s => s.ServerID).Returns(_serverID);
                _environmentModel.SetupGet(e => e.DataListChannel).Returns(datalistchannelmock.Object);
                _mainViewModel.Handle(new SetActiveEnvironmentMessage(_environmentModel.Object));
                _mainViewModel.SettingsCommand.Execute(null);

                var notActiveCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
                _mainViewModel.ActivateItem(notActiveCtx);

                var msg = new SettingsSaveCancelMessage(_environmentModel.Object);
                _mainViewModel.Handle(msg);

                var activeCtx = _mainViewModel.ActiveItem;
                Assert.IsTrue(activeCtx.Equals(notActiveCtx));
            }
        }

        [TestMethod]
        public void NewResourceCommandExpectsWebControllerDisplayDialogue()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _webController.Setup(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false)).Verifiable();
                _mainViewModel.Handle(new SetActiveEnvironmentMessage(_environmentModel.Object));
                _mainViewModel.NewResourceCommand.Execute("Service");
                _webController.Verify(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false), Times.Once());
            }
        }

        [TestMethod]
        public void StartFeedbackCommandCommandExpectsFeedbackInvoked()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _feedbackInvoker.Setup(
                    i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()))
                                .Verifiable();
                _mainViewModel.StartFeedbackCommand.Execute(null);
                _feedbackInvoker.Verify(
                    i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()),
                    Times.Once());
            }
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStartedWhenNotInProgress()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _feedbackInvoker.Setup(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>())).Verifiable();
                _mainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
                _feedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Once());
            }
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStppedtInProgress()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                var mockAction = new Mock<IAsyncFeedbackAction>();
                mockAction.Setup(a => a.StartFeedback()).Verifiable();
                _feedbackInvoker.SetupGet(i => i.CurrentAction).Returns(mockAction.Object);
                _mainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
                _feedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Never());

                // PBI 9598 - 2013.06.10 - TWR : added null parameter
                mockAction.Verify(a => a.FinishFeedBack(null), Times.Once());
            }
        }


        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void DeployAllCommandWithCurrentResourceAndOpenDeploytabExpectsSelectItemInDeployMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVmWithEmptyRepo();

                _eventAggregator.Setup(e => e.Publish(It.IsAny<SelectItemInDeployMessage>()))
                                .Callback<object>((o =>
                                {
                                    var m = (SelectItemInDeployMessage)o;
                                    var r = (IEnvironmentModel)m.Value;
                                    Assert.IsTrue(r.ID.Equals(_secondResource.Object.Environment.ID));
                                })).Verifiable();

                _mainViewModel.DeployAllCommand.Execute(null);
                AddAdditionalContext();
                var ctx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);
                _mainViewModel.ActivateItem(ctx);
                _mainViewModel.DeployAllCommand.Execute(null);
                var activectx = _mainViewModel.ActiveItem;
                Assert.IsTrue(activectx.WorkSurfaceKey.Equals(
                    WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources)));

                _eventAggregator.Verify(e => e.Publish(It.IsAny<SelectItemInDeployMessage>()), Times.Once());
            }
        }

        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void DeployAllCommandWithoutCurrentResourceExpectsDeplouViewModelActive()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVmWithEmptyRepo();
                _mainViewModel.Handle(new SetActiveEnvironmentMessage(_environmentModel.Object));
                _mainViewModel.DeployAllCommand.Execute(null);
                var activectx = _mainViewModel.ActiveItem;
                Assert.IsTrue(activectx.WorkSurfaceKey.Equals(
                    WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources)));
            }
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteServerResourceOnLocalHostAlsoDeletesFromEnvironmentRepoAndExplorerTree()
        {
            lock (syncroot)
            {
                //---------Setup------
                var mock = SetupForDeleteServer();
                _environmentModel.Setup(s => s.IsLocalHost()).Returns(true);

                //---------Execute------
                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);

                //---------Verify------
                mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Once());
                _eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Once());
            }
        }

        [TestMethod]
        public void DeleteServerResourceOnOtherServerDoesntDeleteFromEnvironmentRepoAndExplorerTree()
        {
            lock (syncroot)
            {
                //---------Setup------
                var mock = SetupForDeleteServer();
                _environmentConnection.Setup(c => c.DisplayName).Returns("NotLocalHost");
                _eventAggregator = new Mock<IEventAggregator>();
                _eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>())).Verifiable();

                //---------Execute------
                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);

                //---------Verify------
                mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
                _eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
            }
        }

        [TestMethod]
        public void DeleteResourceConfirmedWithNoResponseExpectNoMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(() => null);

                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);
                _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Never());
            }
        }

        [TestMethod]
        public void DeleteResourceConfirmedWithInvalidResponseExpectNoMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                var response = new UnlimitedObject(@"<DataList>Invalid</DataList>");
                _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(response);


                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);
                _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Never());
            }
        }

        [TestMethod]
        public void DeleteResourceConfirmedExpectRemoveNavigationResourceMessage()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();

                _eventAggregator.Setup(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()))
                                .Callback<object>((o =>
                                {
                                    var m = (RemoveNavigationResourceMessage)o;
                                    Assert.IsTrue(m.ResourceModel.Equals(_firstResource.Object));
                                }));

                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);
                _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Once());
            }
        }

        [TestMethod]
        public void DeleteResourceConfirmedExpectContextRemoved()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                var msg = new DeleteResourceMessage(_firstResource.Object, true);
                _mainViewModel.Handle(msg);
                _resourceDependencyService.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
            }
        }

        [TestMethod]
        public void DeleteResourceWithConfirmExpectsDependencyServiceCalled()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
                var msg = new DeleteResourceMessage(_firstResource.Object, true);
                _mainViewModel.Handle(msg);
                _resourceDependencyService.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
            }
        }

        [TestMethod]
        public void DeleteResourceWithDeclineExpectsDependencyServiceCalled()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
                var msg = new DeleteResourceMessage(_firstResource.Object, false);
                _mainViewModel.Handle(msg);
                _resourceDependencyService.Verify(s => s.HasDependencies(_firstResource.Object), Times.Never());
            }
        }

        [TestMethod]
        public void DeleteResourceWithNullResourceExpectsNoPoupShown()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                SetupForDelete();
                var msg = new DeleteResourceMessage(null, false);
                _mainViewModel.Handle(msg);
                _popupController.Verify(s => s.Show(), Times.Never());
            }
        }

        #endregion delete

        #region ShowStartPage

        // PBI 9512 - 2013.06.07 - TWR: added
        [TestMethod]
        public void MainViewModelShowStartPageExpectedGetsLatestFirst()
        {
            lock (syncroot)
            {
                CreateFullExportsAndVm();
                _mainViewModel.LatestGetter.Invoked += (sender, args) => Assert.IsTrue(true);
                _mainViewModel.ShowStartPage();
            }
        }


        #endregion

        #region DeactivateItem

        // PBI 9405 - 2013.06.13 - Massimo.Guerrera
        [TestMethod]
        public void MainViewModelDeactivateItemWithPreviousItemNotOpenExpectedNoActiveItem()
        {
            lock (syncroot)
            {
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
                wsiRepo.Setup(r => r.Write()).Verifiable();

                #region Setup ImportService - GRRR!

                var importServiceContext = new ImportServiceContext();
                ImportService.CurrentContext = importServiceContext;
                ImportService.Initialize(new List<ComposablePartCatalog>
                {
                    new FullTestAggregateCatalog()
                });
                ImportService.AddExportedValueToContainer(wsiRepo.Object);
                ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);

                #endregion

                var envRepo = new Mock<IEnvironmentRepository>();
                var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, false);
                mockMainViewModel.EventAggregator = ImportService.GetExportValue<IEventAggregator>();
                var resourceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();

                #region Setup WorkSurfaceContextViewModel1

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.Environment).Returns(env.Object);
                resourceModel.Setup(m => m.ID).Returns(resourceID);



                var workflowHelper = new Mock<IWorkflowHelper>();
                var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
                var contextViewModel1 = new WorkSurfaceContextViewModel(
                    new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                    designerViewModel);

                #endregion

                mockMainViewModel.Items.Add(contextViewModel1);

                serverID = Guid.NewGuid();
                resourceID = Guid.NewGuid();

                mockMainViewModel.PopupProvider = Dev2MockFactory.CreateIPopup(MessageBoxResult.No).Object;

                mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
                mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
                mockMainViewModel.CallDeactivate(mockMainViewModel.Items[1]);
                mockMainViewModel.CallDeactivate(mockMainViewModel.Items[0]);
                Assert.AreEqual(null, mockMainViewModel.ActiveItem);
            }
        }

        // PBI 9405 - 2013.06.13 - Massimo.Guerrera
        [TestMethod]
        public void MainViewModelDeactivateItemWithPreviousItemOpenExpectedActiveItemToBePreviousItem()
        {
            lock (syncroot)
            {
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
                wsiRepo.Setup(r => r.Write()).Verifiable();

                #region Setup ImportService - GRRR!

                var importServiceContext = new ImportServiceContext();
                ImportService.CurrentContext = importServiceContext;
                ImportService.Initialize(new List<ComposablePartCatalog>
                {
                    new FullTestAggregateCatalog()
                });
                ImportService.AddExportedValueToContainer(wsiRepo.Object);
                ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);

                #endregion

                var envRepo = new Mock<IEnvironmentRepository>();
                var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, false);
                mockMainViewModel.EventAggregator = ImportService.GetExportValue<IEventAggregator>();
                var resourceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();

                #region Setup WorkSurfaceContextViewModel1

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.Environment).Returns(env.Object);
                resourceModel.Setup(m => m.ID).Returns(resourceID);



                var workflowHelper = new Mock<IWorkflowHelper>();
                var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
                var contextViewModel1 = new WorkSurfaceContextViewModel(
                    new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                    designerViewModel);

                #endregion

                mockMainViewModel.Items.Add(contextViewModel1);

                serverID = Guid.NewGuid();
                resourceID = Guid.NewGuid();

                mockMainViewModel.PopupProvider = Dev2MockFactory.CreateIPopup(MessageBoxResult.No).Object;

                mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
                mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
                mockMainViewModel.CallDeactivate(mockMainViewModel.Items[1]);
                Assert.AreEqual(mockMainViewModel.Items[0], mockMainViewModel.ActiveItem);
            }
        }

        #endregion

        #region OnDeactivate

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesWorkspaceItems()
        {
            lock (syncroot)
            {
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
                wsiRepo.Setup(r => r.Write()).Verifiable();

                #region Setup ImportService - GRRR!

                var importServiceContext = new ImportServiceContext();
                ImportService.CurrentContext = importServiceContext;
                ImportService.Initialize(new List<ComposablePartCatalog>
                {
                    new FullTestAggregateCatalog()
                });
                ImportService.AddExportedValueToContainer(wsiRepo.Object);
                ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);

                #endregion

                var envRepo = new Mock<IEnvironmentRepository>();
                var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);

                viewModel.TestClose();
                wsiRepo.Verify(r => r.Write());
            }
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesResourceModels()
        {
            lock (syncroot)
            {
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
                wsiRepo.Setup(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b))).Verifiable();

                SetupImportServiceForPersistenceTests(wsiRepo);

                var resourceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();

                #region Setup resourceModel

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.Environment).Returns(env.Object);
                resourceModel.Setup(m => m.ID).Returns(resourceID);

                #endregion

                var workflowHelper = new Mock<IWorkflowHelper>();
                var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
                var contextViewModel = new WorkSurfaceContextViewModel(
                    new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                    designerViewModel);

                var envRepo = new Mock<IEnvironmentRepository>();
                var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);
                viewModel.Items.Add(contextViewModel);

                viewModel.TestClose();

                wsiRepo.Verify(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b)));
                resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()));
            }
        }

        #endregion

        #region Constructor

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedLoadsWorkspaceItems()
        {
            lock (syncroot)
            {
                var workspaceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();
                var resourceName = "TestResource_" + Guid.NewGuid();
                var resourceID = Guid.NewGuid();

                var wsi = new WorkspaceItem(workspaceID, serverID, Guid.Empty, resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
                wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

                SetupImportServiceForPersistenceTests(wsiRepo);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
                resourceModel.Setup(m => m.ID).Returns(resourceID);
                resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));
                resourceRepo.Setup(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Verifiable();

                var dsfChannel = new Mock<IStudioClientContext>();
                dsfChannel.Setup(c => c.WorkspaceID).Returns(workspaceID);
                dsfChannel.Setup(c => c.ServerID).Returns(serverID);

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);
                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

                resourceModel.Setup(m => m.Environment).Returns(env.Object);

                var envRepo = new Mock<IEnvironmentRepository>();
                envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));

                var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);

                resourceRepo.Verify(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>()));
                wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()));

                Assert.AreEqual(2, viewModel.Items.Count); // 1 extra for the help tab!
                var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
                Assert.IsNotNull(expected);
            }
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedNotLoadsWorkspaceItemsWithDifferentEnvID()
        {
            lock (syncroot)
            {
                var workspaceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();
                var resourceName = "TestResource_" + Guid.NewGuid();
                var resourceID = Guid.NewGuid();

                var wsi = new WorkspaceItem(workspaceID, serverID, Guid.NewGuid(), resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
                wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

                SetupImportServiceForPersistenceTests(wsiRepo);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
                resourceModel.Setup(m => m.ID).Returns(resourceID);
                resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));
                resourceRepo.Setup(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Verifiable();

                var dsfChannel = new Mock<IStudioClientContext>();
                dsfChannel.Setup(c => c.WorkspaceID).Returns(workspaceID);
                dsfChannel.Setup(c => c.ServerID).Returns(serverID);

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);
                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

                resourceModel.Setup(m => m.Environment).Returns(env.Object);

                var envRepo = new Mock<IEnvironmentRepository>();
                envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));

                var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);

                resourceRepo.Verify(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>()), Times.Never());
                wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()), Times.Never());

                Assert.AreEqual(1, viewModel.Items.Count); // 1 extra for the help tab!
                var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
                Assert.IsNull(expected);
            }
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedNotLoadsWorkspaceItemsWithSameEnvID()
        {
            lock (syncroot)
            {
                var workspaceID = Guid.NewGuid();
                var serverID = Guid.NewGuid();
                var resourceName = "TestResource_" + Guid.NewGuid();
                var resourceID = Guid.NewGuid();

                Guid environmentID = Guid.NewGuid();
                var wsi = new WorkspaceItem(workspaceID, serverID, environmentID, resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
                var wsiRepo = new Mock<IWorkspaceItemRepository>();
                wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
                wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

                SetupImportServiceForPersistenceTests(wsiRepo);

                var resourceModel = new Mock<IContextualResourceModel>();
                resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
                resourceModel.Setup(m => m.ID).Returns(resourceID);
                resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

                var resourceRepo = new Mock<IResourceRepository>();
                resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));
                resourceRepo.Setup(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Verifiable();

                var dsfChannel = new Mock<IStudioClientContext>();
                dsfChannel.Setup(c => c.WorkspaceID).Returns(workspaceID);
                dsfChannel.Setup(c => c.ServerID).Returns(serverID);

                var envConn = new Mock<IEnvironmentConnection>();
                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);
                env.Setup(e => e.Connection).Returns(envConn.Object);
                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
                env.Setup(e => e.ID).Returns(environmentID);

                resourceModel.Setup(m => m.Environment).Returns(env.Object);

                var envRepo = new Mock<IEnvironmentRepository>();
                envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));

                var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);

                resourceRepo.Verify(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>()), Times.AtLeastOnce());
                wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()), Times.AtLeastOnce());

                Assert.AreEqual(2, viewModel.Items.Count); // 1 extra for the help tab!
                var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
                Assert.IsNotNull(expected);
            }
        }

        #endregion

        #region SetupImportServiceForPersistenceTests

        static void SetupImportServiceForPersistenceTests(Mock<IWorkspaceItemRepository> wsiRepo)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            ImportService.AddExportedValueToContainer(wsiRepo.Object);
            ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);
            ImportService.AddExportedValueToContainer(new Mock<IWindowManager>().Object);
            ImportService.AddExportedValueToContainer(new Mock<IPopupController>().Object);
            ImportService.AddExportedValueToContainer(new Mock<IWizardEngine>().Object);

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.UserIdentity).Returns(new GenericIdentity("TestUser"));
            ImportService.AddExportedValueToContainer(securityContext.Object);
        }

        #endregion

        #region BrowserPopupController

        // BUG 9798 - 2013.06.25 - TWR : added
        [TestMethod]
        public void MainViewModelShowCommunityPageExpectedInvokesConstructorsBrowserPopupController()
        {
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Verifiable();

            #region Setup ImportService - GRRR!

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);

            #endregion

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());

            var vm = new MainViewModel(envRepo.Object, false, popupController.Object);
            vm.ShowCommunityPage();

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()));
        }

        // BUG 9798 - 2013.06.25 - TWR : added
        [TestMethod]
        public void MainViewModelConstructorWithNullBrowserPopupControllerExpectedCreatesExternalBrowserPopupController()
        {
            #region Setup ImportService - GRRR!

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);

            #endregion

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());

            var vm = new MainViewModel(envRepo.Object, false);
            Assert.IsInstanceOfType(vm.BrowserPopupController, typeof(ExternalBrowserPopupController));
        }

        #endregion

        #region ActiveEnvironment

        [TestMethod]
        public void SetActiveEnvironmentCallsUpdateActiveEnvironmentMessageExpectedUpdateActiveEnvironmentMessagePublished()
        {
            lock (syncroot)
            {
                Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();
                CreateFullExportsAndVmWithEmptyRepo();
                _mainViewModel.Handle(new SetActiveEnvironmentMessage(mockEnv.Object));
                _eventAggregator.Verify(c => c.Publish(It.IsAny<UpdateActiveEnvironmentMessage>()), Times.Once());
            }
        }

        #endregion

    }
}
