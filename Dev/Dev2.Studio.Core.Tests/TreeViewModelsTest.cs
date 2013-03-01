#region

using System;
using System.Linq;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

#endregion

namespace Dev2.Core.Tests
{
    /// <summary>
    ///     Summary description for Base
    /// </summary>
    [TestClass]
    public class TreeViewModelsTest
    {
        #region Variables

        CategoryTreeViewModel categoryVM;
        CategoryTreeViewModel categoryVM2;
        EnvironmentTreeViewModel environmentVM;
        Mock<IEnvironmentModel> mockEnvironmentModel;
        Mock<IContextualResourceModel> mockResourceModel;
        Mock<IContextualResourceModel> mockResourceModel2;
        ResourceTreeViewModel resourceVM;
        ResourceTreeViewModel resourceVM2;
        RootTreeViewModel rootVM;
        ServiceTypeTreeViewModel serviceTypeVM;
        ServiceTypeTreeViewModel serviceTypeVM2;

        private static object _testGuard = new object();

        #endregion

        /// <summary>
        ///     Gets or sets the result context which provides
        ///     information about and functionality for the current result run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Initialize

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(_testGuard);

            CompositionInitializer.DeployViewModelOkayTest();
            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.DsfAddress).Returns(new Uri("http://127.0.0.1/"));

            mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");

            mockResourceModel2 = new Mock<IContextualResourceModel>();
            mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");

            rootVM = TreeViewModelFactory.Create() as RootTreeViewModel;
            environmentVM = TreeViewModelFactory.Create(mockEnvironmentModel.Object, rootVM) as EnvironmentTreeViewModel;
            serviceTypeVM = TreeViewModelFactory.Create(ResourceType.WorkflowService, environmentVM) as ServiceTypeTreeViewModel;
            serviceTypeVM2 = TreeViewModelFactory.Create(ResourceType.Service, environmentVM) as ServiceTypeTreeViewModel;

            categoryVM = TreeViewModelFactory.CreateCategory(mockResourceModel.Object.Category,
                mockResourceModel.Object.ResourceType, serviceTypeVM) as CategoryTreeViewModel;

            categoryVM2 = TreeViewModelFactory.CreateCategory(mockResourceModel2.Object.Category,
                mockResourceModel2.Object.ResourceType, serviceTypeVM2) as CategoryTreeViewModel;

            resourceVM = TreeViewModelFactory.Create(mockResourceModel.Object, categoryVM, false) as ResourceTreeViewModel;
            resourceVM2 = TreeViewModelFactory.Create(mockResourceModel2.Object, categoryVM2, false) as ResourceTreeViewModel;
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion

        #region Root

        [TestMethod]
        public void CheckRoot_Expected_AllChildrenChecked()
        {
            resourceVM.IsChecked = false;
            resourceVM2.IsChecked = false;
            rootVM.IsChecked = true;

            Assert.IsTrue(rootVM.IsChecked == true);
            Assert.IsTrue(environmentVM.IsChecked == true);
            Assert.IsTrue(serviceTypeVM.IsChecked == true);
            Assert.IsTrue(serviceTypeVM2.IsChecked == true);
            Assert.IsTrue(categoryVM.IsChecked == true);
            Assert.IsTrue(categoryVM2.IsChecked == true);
            Assert.IsTrue(resourceVM.IsChecked == true);
            Assert.IsTrue(resourceVM2.IsChecked == true);
        }

        [TestMethod]
        public void UncheckRoot_Expected_AllChildrenUnChecked()
        {
            resourceVM.IsChecked = true;
            resourceVM2.IsChecked = true;
            rootVM.IsChecked = false;

            Assert.IsTrue(rootVM.IsChecked == false);
            Assert.IsTrue(environmentVM.IsChecked == false);
            Assert.IsTrue(serviceTypeVM.IsChecked == false);
            Assert.IsTrue(serviceTypeVM2.IsChecked == false);
            Assert.IsTrue(categoryVM.IsChecked == false);
            Assert.IsTrue(categoryVM2.IsChecked == false);
            Assert.IsTrue(resourceVM.IsChecked == false);
            Assert.IsTrue(resourceVM2.IsChecked == false);
        }

        [TestMethod]
        public void OneCheckedChildAndOneUnCheckedChild_Expected_PartiallyCheckedRoot()
        {
            resourceVM.IsChecked = true;
            resourceVM2.IsChecked = false;

            Assert.IsTrue(rootVM.IsChecked == null);
            Assert.IsTrue(environmentVM.IsChecked == null);
            Assert.IsTrue(serviceTypeVM.IsChecked == true);
            Assert.IsTrue(serviceTypeVM2.IsChecked == false);
            Assert.IsTrue(categoryVM.IsChecked == true);
            Assert.IsTrue(categoryVM2.IsChecked == false);
            Assert.IsTrue(resourceVM.IsChecked == true);
            Assert.IsTrue(resourceVM2.IsChecked == false);
        }

        [TestMethod]
        public void RootNodeFindChildByEnvironment_Expected_RightEnvironmentNode()
        {
            var environment = mockEnvironmentModel.Object;
            var child = rootVM.FindChild(environment);
            Assert.IsTrue(Equals(child, environmentVM));
        }

        [TestMethod]
        public void RootNodeFindChildByResourceModel_Expected_RightChildNode()
        {
            var child = rootVM.FindChild(mockResourceModel.Object);
            Assert.IsTrue(ReferenceEquals(child, resourceVM));
        }

        [TestMethod]
        public void TestGetChildCountFromRoot_Expected_RecursiveTotal()
        {
            var childCount = rootVM.ChildrenCount;
            Assert.IsTrue(childCount == 2);
        }

        [TestMethod]
        public void TestGetChildCount_WherePredicateIsNull_Expected_AllChildren()
        {
            var childCount = rootVM.GetChildren(null).ToList().Count;
            Assert.IsTrue(childCount == 7);
        }

        [TestMethod]
        public void TestGetChildren_Expected_FirstChildMatchingPredicate()
        {
            var child = rootVM.GetChildren(c => c.DisplayName == "Mock").ToList();
            Assert.IsTrue(child.Count == 1);
            Assert.IsTrue(child.First().DisplayName == "Mock");
        }

        [TestMethod]
        public void TestFiler_Were_FiterIsSet_Expected_CheckedNodesAndParentCategoriesArentFiltered_()
        {
            ITreeNode parent =
                TreeViewModelFactory.CreateCategory("More", ResourceType.WorkflowService, null);

            ITreeNode checkedNonMatchingNode =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake").Object,
                    parent, false);

            checkedNonMatchingNode.IsChecked = true;

            ITreeNode nonMatchingNode =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1111").Object,
                    parent, false);

            ITreeNode matchingNode =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "Match").Object,
                    parent, false);

            parent.FilterText = ("Match");

            Assert.IsTrue(nonMatchingNode.IsFiltered);
            Assert.IsFalse(checkedNonMatchingNode.IsFiltered);
            Assert.IsFalse(matchingNode.IsFiltered);
            Assert.IsFalse(parent.IsFiltered);
        }

        [TestMethod]
        public void TestFiler_Were_AllNodesFiltered_Expected_CheckedNodesAndParentCategoriestFiltered_()
        {
            var rootVM = TreeViewModelFactory.Create() as RootTreeViewModel;
            var environmentVM = TreeViewModelFactory.Create(mockEnvironmentModel.Object, rootVM) as EnvironmentTreeViewModel;
            var serviceTypeVM = TreeViewModelFactory.Create(ResourceType.WorkflowService, environmentVM) as ServiceTypeTreeViewModel;
            var categoryVM = TreeViewModelFactory.CreateCategory(mockResourceModel.Object.Category,
                mockResourceModel.Object.ResourceType, serviceTypeVM);
            var resource1 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1").Object;
            var resource2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake2").Object;
            var resourceVM1 = TreeViewModelFactory.Create(resource1, categoryVM, false);
            var resourceVM2 = TreeViewModelFactory.Create(resource2, categoryVM, false);

            resourceVM1.IsChecked = false;
            resourceVM2.IsChecked = false;

            categoryVM.FilterText = ("Match");

            Assert.IsTrue(resourceVM1.IsFiltered);
            Assert.IsTrue(resourceVM2.IsFiltered);
            Assert.IsTrue(categoryVM.IsFiltered);
        }

        [TestMethod]
        public void TestFilter_Were_AllNodesCheckedAndFilter_Expected_CheckedNodesAndParentCategoriestNotFiltered()
        {
            var rootVM = TreeViewModelFactory.Create() as RootTreeViewModel;
            var environmentVM = TreeViewModelFactory.Create(mockEnvironmentModel.Object, rootVM) as EnvironmentTreeViewModel;
            var serviceTypeVM = TreeViewModelFactory.Create(ResourceType.WorkflowService, environmentVM) as ServiceTypeTreeViewModel;
            var categoryVM = TreeViewModelFactory.CreateCategory(mockResourceModel.Object.Category,
                mockResourceModel.Object.ResourceType, serviceTypeVM);
            var resource1 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1").Object;
            var resource2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake2").Object;
            var resourceVM1 = TreeViewModelFactory.Create(resource1, categoryVM, false);
            var resourceVM2 = TreeViewModelFactory.Create(resource2, categoryVM, false);

            resourceVM1.IsChecked = false;
            resourceVM2.IsChecked = false;

            categoryVM.FilterText = "cake";

            Assert.IsFalse(resourceVM1.IsFiltered);
            Assert.IsFalse(resourceVM2.IsFiltered);
            Assert.IsFalse(categoryVM.IsFiltered);
        }

        #endregion Root

        #region Environment

        [TestMethod]
        public void EnvironmentNodeFindChildByResourceType_Expected_RightServiceTypeNode()
        {
            var child = environmentVM.FindChild(ResourceType.WorkflowService);
            Assert.IsTrue(ReferenceEquals(child, serviceTypeVM));
        }

        [TestMethod]
        public void EnvironmentNodeCantDisconnectLocalHost()
        {
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(c => c.EnvironmentConnection).Returns(new EnvironmentConnection());
            mockEnvironmentModel.SetupGet(c => c.Name).Returns(StringResources.DefaultEnvironmentName);         
            Assert.IsTrue(environmentVM.CanDisconnect == false);
        }

        [TestMethod]
        public void EnvironmentNodeCanDisconnectOtherHosts()
        {
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(c => c.EnvironmentConnection).Returns(new EnvironmentConnection());
            mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");
            Assert.IsTrue(environmentVM.CanDisconnect == true);
        }

        [TestMethod]
        public void EnvironmentNodeConnectCommand_Expected_EnvironmentModelConnectMethodExecuted()
        {
            mockEnvironmentModel.Setup(c => c.Connect()).Verifiable();
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(false);
            var cmd = environmentVM.ConnectCommand;
            cmd.Execute(null);

            mockEnvironmentModel.Verify(c => c.Connect(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentNodeDisconnectCommand_Expected_EnvironmentModelDisconnectMethodExecuted()
        {
            mockEnvironmentModel.Setup(c => c.Disconnect()).Verifiable();
            mockEnvironmentModel.SetupGet(c => c.EnvironmentConnection).Returns(new EnvironmentConnection());
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            var cmd = environmentVM.DisconnectCommand;
            cmd.Execute(null);

            mockEnvironmentModel.Verify(c => c.Disconnect(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentNodeRemoveCommand_Expected_MediatorRemoveServerFromExplorerMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.RemoveServerFromExplorer, o => messageRecieved = true);
            environmentVM.RemoveCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        #endregion Environment

        #region ServicType

        [TestMethod]
        public void ServiceNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = serviceTypeVM.EnvironmentModel;
            var model2 = environmentVM.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }

        [TestMethod]
        public void ServiceTypeNodeFindChildByString_Expected_RightCategoryNode()
        {
            var child = serviceTypeVM.FindChild("Testing");
            Assert.IsTrue(ReferenceEquals(child, categoryVM));
        }

        [TestMethod]
        public void TestGetChildCountFromService_Expected_RecursiveTotal()
        {
            var childCount = serviceTypeVM.ChildrenCount;
            Assert.IsTrue(childCount == 1);
        }

        #endregion ServiceType

        #region Category

        [TestMethod]
        public void AddChildExpectChildAdded()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing3");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");

            var count = categoryVM2.ChildrenCount;

            var toAdd = TreeViewModelFactory.Create(mockResource3.Object, null, false);

            categoryVM2.Add(toAdd);

            Assert.IsTrue(categoryVM2.ChildrenCount == count + 1);
            Assert.IsTrue(ReferenceEquals(toAdd.TreeParent, categoryVM2));
        }

        [TestMethod]
        public void RemoveChildExpectChildRemoved()
        {
            var count = categoryVM2.ChildrenCount;
            var toRemove = categoryVM2.GetChildren(c => true).FirstOrDefault();
            categoryVM2.Remove(toRemove);
            Assert.IsTrue(categoryVM2.ChildrenCount == count - 1);
            Assert.IsTrue(toRemove.TreeParent == null);
        }

        [TestMethod]
        public void CategoryNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = categoryVM.EnvironmentModel;
            var model2 = environmentVM.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }

        [TestMethod]
        public void CategoryNodeFindChildByName_Expected_RightChildNode()
        {
            var child = categoryVM.FindChild("Mock");
            Assert.IsTrue(ReferenceEquals(child, resourceVM));
        }

        [TestMethod]
        public void TestGetChildCountFromCategory_Expected_RecursiveTotal()
        {
            var childCount = categoryVM.ChildrenCount;
            Assert.IsTrue(childCount == 1);
        }    
        #endregion Category

        #region Resource

        [TestMethod]
        public void TestGetChildCountFromResource_Expected_NoCount()
        {
            var childCount = resourceVM.ChildrenCount;
            Assert.IsTrue(childCount == 0);
        }

        [TestMethod]
        public void ResourceNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = resourceVM.EnvironmentModel;
            var model2 = environmentVM.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }


        [TestMethod]
        public void ResourceNodeDebugCommand_Expected_MediatorDebugResourceMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.DebugResource, o => messageRecieved = true);
            resourceVM.DebugCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeDeleteCommand_With_Source_Expected_MediatorDeleteSourceExplorerResourceMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteSourceExplorerResource, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            resourceVM.DeleteCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeDeleteCommand_With_WorkflowService_Expected_MediatorDeleteWorkflowExplorerResourceMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteWorkflowExplorerResource, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceVM.DeleteCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeDeleteCommand_With_Service_Expected_MediatorDeleteServiceExplorerResourceMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteServiceExplorerResource, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            resourceVM.DeleteCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeCreateWizardCommand_Expected_WizardEngineCreateResourceMEthodExecuted()
        {
            var mockWizardEngine = new Mock<IWizardEngine>();
            mockWizardEngine.Setup(e => e.CreateResourceWizard(mockResourceModel.Object))
                            .Callback<object>(o =>
                            {
                                var resource = (IContextualResourceModel)o;
                                Assert.IsTrue(ReferenceEquals(resource, mockResourceModel.Object));
                            }).Verifiable();

            var newResourceVM = TreeViewModelFactory.Create(mockResourceModel.Object, null, false)
                                as ResourceTreeViewModel;

            newResourceVM.WizardEngine = mockWizardEngine.Object;

            newResourceVM.CreateWizardCommand.Execute(null);
            mockWizardEngine.Verify(e => e.CreateResourceWizard(It.IsAny<IContextualResourceModel>()), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeManualEditCommand_With_Source_Expected_MediatorShowEditResourceWizardMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            resourceVM.ManualEditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeManualEditCommand_With_WorkflowService_Expected_MediatorAddWorkflowDesignerMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceVM.ManualEditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeManualEditCommand_With_Service_Expected_MediatorShowEditResourceWizardMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            resourceVM.ManualEditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_ResourceWizard_Expected_WizardEngineEditWizardExecuted()
        {
            var mockWizardEngine = new Mock<IWizardEngine>();
            mockWizardEngine.Setup(c => c.IsResourceWizard(It.IsAny<IContextualResourceModel>()))
                            .Returns(true);
            mockWizardEngine.Setup(e => e.EditWizard(mockResourceModel.Object))
                            .Callback<object>(o =>
                            {
                                var resource = (IContextualResourceModel)o;
                                Assert.IsTrue(ReferenceEquals(resource, mockResourceModel.Object));
                            }).Verifiable();

            var newResourceVM = TreeViewModelFactory.Create(mockResourceModel.Object, null, true)
                                as ResourceTreeViewModel;

            newResourceVM.WizardEngine = mockWizardEngine.Object;

            newResourceVM.EditCommand.Execute(null);
            mockWizardEngine.Verify(e => e.EditWizard(It.IsAny<IContextualResourceModel>()), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_Source_Expected_MediatorShowEditResourceWizardMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            resourceVM.EditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_WorkflowService_Expected_MediatorAddWorkflowDesignerMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceVM.EditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_Service_Expected_MediatorShowEditResourceWizardMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            resourceVM.EditCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeShowPropertiesCommand_Expected_MediatorShowEditResourceWizardMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            resourceVM.ShowPropertiesCommand.Execute(null);

            Assert.IsTrue(messageRecieved);

            messageRecieved = false;
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            resourceVM.ShowPropertiesCommand.Execute(null);

            Assert.IsTrue(messageRecieved);

            messageRecieved = false;
            mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            resourceVM.ShowPropertiesCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeShowDependenciesCommand_Expected_MediatorShowDependencyGraphMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowDependencyGraph, o => messageRecieved = true);
            resourceVM.ShowDependenciesCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeHelpCommand_Expected_MediatorAddHelpDocumentMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddHelpDocument, o => messageRecieved = true);
            resourceVM.HelpCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void ResourceNodeBuildCommand_Expected_EditCommandExecuted_And_MediatorSaveResourceMessage()
        {
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.SaveResource, o => messageRecieved = true);
            resourceVM.BuildCommand.Execute(null);

            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeWorkflowService_Expected_AddWorkflowDesignerMediatorMessage()
        {
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => messageRecieved = true);
            resourceVM.EditCommand.Execute(mockResourceModel);

            Assert.IsTrue(messageRecieved);
        }


        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeSource_Expected_ShowEditResourceWizardMediatorMessage()
        {
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            resourceVM.EditCommand.Execute(mockResourceModel);

            //Assert
            Assert.IsTrue(messageRecieved);
        }

        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeService_Expected_ShowEditResourceWizardMediatorMessage()
        {
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            bool messageRecieved = false;
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, o => messageRecieved = true);
            resourceVM.EditCommand.Execute(mockResourceModel);

            //Assert
            Assert.IsTrue(messageRecieved);
        }

        //
        //Juries - Bug 8427
        //
        [TestMethod]
        public void ResourceParentCheckedDoesNotCheckFilteredChildren()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing3");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");
            var toAdd = TreeViewModelFactory.Create(mockResource3.Object, categoryVM2, false);

            categoryVM2.IsChecked = true;
            Assert.IsTrue(categoryVM2.Children.Count(c => c.IsChecked == true) == 2);

            categoryVM2.IsChecked = false;
            rootVM.FilterText = "Mock3";
            categoryVM2.IsChecked = true;

            Assert.IsTrue(categoryVM2.Children.Count(c => c.IsChecked == true) == 1);
        }

        //
        //Juries - Bug 8427
        //
        [TestMethod]
        public void FilterChangedResultingInItemNotFilteredUpdatesParentState()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing2");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");
            var toAdd = TreeViewModelFactory.Create(mockResource3.Object, categoryVM2, false);

            Assert.IsTrue(categoryVM2.ChildrenCount == 2);

            toAdd.IsChecked = true;

            rootVM.FilterText = "Mock3";

            Assert.IsTrue(categoryVM2.IsChecked == true);

            rootVM.FilterText = "";

            Assert.IsTrue(categoryVM2.IsChecked == null);
        }
        #endregion Resource

    }
}