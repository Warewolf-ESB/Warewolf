using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Composition;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Deploy;
using Dev2.Studio.Factory;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DeployStatsCalculatorTests
    {
        private Mock<IEnvironmentModel> mockEnvironmentModel;
        private Mock<IContextualResourceModel> mockResourceModel;
        RootTreeViewModel rootVM;
        ResourceTreeViewModel resourceVM;
        CategoryTreeViewModel categoryVM;
        ServiceTypeTreeViewModel serviceTypeVM;
        EnvironmentTreeViewModel environmentVM;
        #region Class Members

        private static DeployStatsCalculator _deployStatsCalculator = new DeployStatsCalculator();
        private static ImportServiceContext _importContext;

        #endregion Class Members

        #region Initialization

        [TestInitialize]
        public void TestInit()
        {
            _importContext = CompositionInitializer.DeployViewModelOkayTest();
            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            rootVM = TreeViewModelFactory.Create() as RootTreeViewModel;
            environmentVM = TreeViewModelFactory.Create(mockEnvironmentModel.Object, rootVM) as EnvironmentTreeViewModel;
            serviceTypeVM = TreeViewModelFactory.Create(ResourceType.WorkflowService, environmentVM) as ServiceTypeTreeViewModel;
            categoryVM = TreeViewModelFactory.CreateCategory(mockResourceModel.Object.Category,
                                                                 mockResourceModel.Object.ResourceType, serviceTypeVM) as CategoryTreeViewModel;
            resourceVM = TreeViewModelFactory.Create(mockResourceModel.Object, categoryVM, false) as ResourceTreeViewModel;
        }

        #endregion Initialization

        #region Test Methods

        #region CalculateStats

        [TestMethod]
        public void CalculateStats()
        {
            ImportService.CurrentContext = _importContext;

            List<string> exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            List<string> websiteCategories = new List<string> { "Website" };
            List<string> webpageCategories = new List<string> { "Human Interface Workflow", "Webpage" };
            List<string> blankCategories = new List<string>();

            List<ITreeNode> items = new List<ITreeNode>();
            var vm1 =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object, null, false);
            vm1.IsChecked = true;
            var vm2 =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object, null, true);
            vm2.IsChecked = false;
            var vm3 =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.Service).Object, null, true);
            vm3.IsChecked = true;
            var vm4 =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.Service).Object, null, false);
            vm4.IsChecked = false;

            items.Add(vm1);
            items.Add(vm2);
            items.Add(vm3);
            items.Add(vm4);
      
            Dictionary<string, Func<ITreeNode, bool>> predicates = new Dictionary<string, Func<ITreeNode, bool>>();
            predicates.Add("Services", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.Service, blankCategories, exclusionCategories)));
            predicates.Add("Workflows", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, blankCategories, exclusionCategories)));
            predicates.Add("Sources", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.Source, blankCategories, exclusionCategories)));
            predicates.Add("Webpages", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, webpageCategories, blankCategories)));
            predicates.Add("Websites", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, websiteCategories, blankCategories)));
            predicates.Add("Unknown", new Func<ITreeNode, bool>(n => _deployStatsCalculator.SelectForDeployPredicate(n)));

            ObservableCollection<DeployStatsTO> expected = new ObservableCollection<DeployStatsTO>();
            expected.Add(new DeployStatsTO("Services", "1"));
            expected.Add(new DeployStatsTO("Workflows", "1"));
            expected.Add(new DeployStatsTO("Sources", "0"));
            expected.Add(new DeployStatsTO("Webpages", "0"));
            expected.Add(new DeployStatsTO("Websites", "0"));
            expected.Add(new DeployStatsTO("Unknown", "0"));

            ObservableCollection<DeployStatsTO> actual = new ObservableCollection<DeployStatsTO>();

            _deployStatsCalculator.CalculateStats(items, predicates, actual);

            CollectionAssert.AreEqual(expected, actual, new DeployStatsTOComparer());
        }

        #endregion CalculateStats

        #region SelectForDeployPredicate

        [TestMethod]
        public void SelectForDeployPredicate_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_UncheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object, 
                    null, false);

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_NullResourceModelOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel =
            TreeViewModelFactory.Create(null,
                                        null, false);
            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_ValidNavigationItemViewModel_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object,
                    null, false);

            navigationItemViewModel.IsChecked = true;

            bool expected = true;
            bool actual = _deployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectForDeployPredicate

        #region SelectForDeployPredicateWithTypeAndCategories

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = _deployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(null, ResourceType.Unknown, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        
        public void SelectForDeployPredicateWithTypeAndCategories_UnCheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.TreeParent.IsChecked = false;

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(rootVM, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    null,
                    null, false);

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_TypeMismatch_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    Dev2MockFactory.SetupResourceModelMock(ResourceType.HumanInterfaceProcess).Object,
                    null, false);

            bool expected = false;
            bool actual = _deployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NoCategories_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            rootVM.IsChecked = true;

            bool expected = true;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVM, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InInclusionCategories_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = true;

            bool expected = true;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVM, ResourceType.WorkflowService, new List<string> { "Testing" }, new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInInclusionCategories_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = true;

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVM, ResourceType.WorkflowService, new List<string> { "TestingCake" }, new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InExclusionCategories_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = true;

            bool expected = false;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVM, ResourceType.WorkflowService, new List<string>(), new List<string> { "Testing" });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInExclusionCategories_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = true;

            bool expected = true;
            bool actual = _deployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVM, ResourceType.WorkflowService, new List<string>(), new List<string> { "TestingCake" });

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectForDeployPredicateWithTypeAndCategories

        #region DeploySummaryPredicateExisting

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(null, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullEnvironmentModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;
            resourceVM.IsChecked = true;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(resourceVM, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_UnCheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = false;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(resourceVM, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    null,
                    null, false);

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourcesOnEnvironmentModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            resourceVM.IsChecked = true;
       
            Mock<IEnvironmentModel> mockEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
            mockEnvironmentModel.Setup(e => e.Resources).Returns<object>(null);

            IEnvironmentModel environmentModel = mockEnvironmentModel.Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(resourceVM, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentContainsResource_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            resourceVM.IsChecked = true;
            ResourceTreeViewModel vm = resourceVM as ResourceTreeViewModel;

            vm.DataContext = resourceModel.Object;

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            bool expected = true;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(resourceVM, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentDoesntContainResource_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            resourceVM.IsChecked = true;
            ResourceTreeViewModel vm = resourceVM as ResourceTreeViewModel;

            vm.DataContext = resourceModel.Object;
            
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateExisting(resourceVM, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion DeploySummaryPredicateExisting

        #region DeploySummaryPredicateNew

        [TestMethod]
        public void DeploySummaryPredicateNew_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(null, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullEnvironmentModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> _mockResourceModel = new Mock<IContextualResourceModel>();
            environmentVM = TreeViewModelFactory.Create(null, rootVM) as EnvironmentTreeViewModel;
            serviceTypeVM = TreeViewModelFactory.Create(ResourceType.WorkflowService, environmentVM) as ServiceTypeTreeViewModel;
            categoryVM = TreeViewModelFactory.CreateCategory(mockResourceModel.Object.Category,
                                                                 mockResourceModel.Object.ResourceType, serviceTypeVM) as CategoryTreeViewModel;
            resourceVM = TreeViewModelFactory.Create(_mockResourceModel.Object, categoryVM, false) as ResourceTreeViewModel;

            resourceVM.IsChecked = true;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(resourceVM, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_UnCheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(resourceVM, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    null,
                    null, false);
            
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourcesOnEnvironmentModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            
            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    resourceModel.Object,
                    null, false); 
            
            Mock<IEnvironmentModel> mockEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
            mockEnvironmentModel.Setup(e => e.Resources).Returns<object>(null);

            IEnvironmentModel environmentModel = mockEnvironmentModel.Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentContainsResource_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
           
            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    resourceModel.Object,
                    null, false); 
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            bool expected = false;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentDoesntContainResource_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            ITreeNode navigationItemViewModel =
                TreeViewModelFactory.Create(
                    resourceModel.Object,
                    null, false);
            navigationItemViewModel.IsChecked = true;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            bool expected = true;
            bool actual = _deployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion DeploySummaryPredicateNew

        #endregion Test Methods
    }
}
