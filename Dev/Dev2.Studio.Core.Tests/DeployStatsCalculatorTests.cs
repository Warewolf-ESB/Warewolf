using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployStatsCalculatorTests
    {
        Mock<IEnvironmentModel> _mockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        RootTreeViewModel _rootVm;
        ResourceTreeViewModel _resourceVm;
        CategoryTreeViewModel _categoryVm;
        ServiceTypeTreeViewModel _serviceTypeVm;
        EnvironmentTreeViewModel _environmentVm;
        ImportServiceContext _importContext;
        #region Class Members

        static readonly DeployStatsCalculator DeployStatsCalculator = new DeployStatsCalculator();


        #endregion Class Members

        #region Initialization

        void Setup(Guid resourceId = new Guid(), string mockResourceName = "")
        {
            _importContext = new Mock<ImportServiceContext>().Object;

            var eventAggregator = new Mock<IEventAggregator>().Object;
            var serverEvents = new Mock<IEventPublisher>().Object;
            
            _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(e => e.Connection.ServerEvents).Returns(serverEvents);

            _mockResourceModel = CreateResourceModel(serverEvents, resourceId);
            if(!string.IsNullOrEmpty(mockResourceName))
            {
                _mockResourceModel.Setup(c => c.ResourceName).Returns(mockResourceName);
            }

            _rootVm = new RootTreeViewModel(eventAggregator);

            _environmentVm = new EnvironmentTreeViewModel(eventAggregator, _rootVm, _mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            _serviceTypeVm = new ServiceTypeTreeViewModel(eventAggregator, _environmentVm, ResourceType.WorkflowService);
            _categoryVm = new CategoryTreeViewModel(eventAggregator, _serviceTypeVm, _mockResourceModel.Object.Category, _mockResourceModel.Object.ResourceType);
            _resourceVm = new ResourceTreeViewModel(eventAggregator, _categoryVm, _mockResourceModel.Object);
        }

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceId = new Guid())
        {
            return CreateResourceModel(new Mock<IEventPublisher>().Object, resourceId);
        }

        static Mock<IContextualResourceModel> CreateResourceModel(IEventPublisher serverEvents, Guid resourceId = new Guid())
        {
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.Setup(r => r.Category).Returns("Testing");
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(serverEvents);
            resourceModel.Setup(r => r.ID).Returns(resourceId);
            return resourceModel;
        }

        #endregion Initialization

        #region Test Methods

        #region CalculateStats

        [TestMethod]
        public void CalculateStats()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            List<string> exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            List<string> websiteCategories = new List<string> { "Website" };
            List<string> webpageCategories = new List<string> { "Human Interface Workflow", "Webpage" };
            List<string> blankCategories = new List<string>();

            var eventAggregator = new Mock<IEventAggregator>();

            List<ITreeNode> items = new List<ITreeNode>();
            var vm1 = new ResourceTreeViewModel(eventAggregator.Object, null, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object);
            vm1.IsChecked = true;
            var vm2 = new ResourceTreeViewModel(eventAggregator.Object, null, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object);
            vm2.IsChecked = false;
            var vm3 = new ResourceTreeViewModel(eventAggregator.Object, null, Dev2MockFactory.SetupResourceModelMock(ResourceType.Service).Object);
            vm3.IsChecked = true;
            var vm4 = new ResourceTreeViewModel(eventAggregator.Object, null, Dev2MockFactory.SetupResourceModelMock(ResourceType.Service).Object);
            vm4.IsChecked = false;

            items.Add(vm1);
            items.Add(vm2);
            items.Add(vm3);
            items.Add(vm4);

            Dictionary<string, Func<ITreeNode, bool>> predicates = new Dictionary<string, Func<ITreeNode, bool>>();
            predicates.Add("Services", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.Service, blankCategories, exclusionCategories)));
            predicates.Add("Workflows", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, blankCategories, exclusionCategories)));
            predicates.Add("Sources", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.Source, blankCategories, exclusionCategories)));
            predicates.Add("Webpages", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, webpageCategories, blankCategories)));
            predicates.Add("Websites", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, websiteCategories, blankCategories)));
            predicates.Add("Unknown", new Func<ITreeNode, bool>(n => DeployStatsCalculator.SelectForDeployPredicate(n)));

            ObservableCollection<DeployStatsTO> expected = new ObservableCollection<DeployStatsTO>();
            expected.Add(new DeployStatsTO("Services", "1"));
            expected.Add(new DeployStatsTO("Workflows", "1"));
            expected.Add(new DeployStatsTO("Sources", "0"));
            expected.Add(new DeployStatsTO("Webpages", "0"));
            expected.Add(new DeployStatsTO("Websites", "0"));
            expected.Add(new DeployStatsTO("Unknown", "0"));

            int expectedDeployItemCount = 2;
            int actualDeployItemCount;
            ObservableCollection<DeployStatsTO> actual = new ObservableCollection<DeployStatsTO>();

            DeployStatsCalculator.CalculateStats(items, predicates, actual, out actualDeployItemCount);

            CollectionAssert.AreEqual(expected, actual, new DeployStatsTOComparer());
            Assert.AreEqual(expectedDeployItemCount, actualDeployItemCount); //BUG 8816, Added an extra assert to ensure the deploy item count is correct
        }

        #endregion CalculateStats

        #region SelectForDeployPredicate

        [TestMethod]
        public void SelectForDeployPredicate_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_UncheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object);

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_NullResourceModelOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, CreateResourceModel().Object);
            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_ValidNavigationItemViewModel_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService).Object);

            navigationItemViewModel.IsChecked = true;

            bool expected = true;
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectForDeployPredicate

        #region SelectForDeployPredicateWithTypeAndCategories

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = DeployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(null, ResourceType.Unknown, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]

        public void SelectForDeployPredicateWithTypeAndCategories_UnCheckedNavigationItemViewModel_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.TreeParent.IsChecked = false;

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_rootVm, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, CreateResourceModel().Object);

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_TypeMismatch_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, Dev2MockFactory.SetupResourceModelMock(ResourceType.HumanInterfaceProcess).Object);

            bool expected = false;
            bool actual = DeployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NoCategories_Expected_True()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _rootVm.IsChecked = true;

            bool expected = true;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InInclusionCategories_Expected_True()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = true;

            bool expected = true;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_resourceVm, ResourceType.WorkflowService, new List<string> { "Testing" }, new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInInclusionCategories_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = true;

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_resourceVm, ResourceType.WorkflowService, new List<string> { "TestingCake" }, new List<string>());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InExclusionCategories_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = true;

            bool expected = false;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string> { "Testing" });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInExclusionCategories_Expected_True()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = true;

            bool expected = true;
            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(_resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string> { "TestingCake" });

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectForDeployPredicateWithTypeAndCategories

        #region DeploySummaryPredicateExisting

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(null, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullEnvironmentModel_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;
            _resourceVm.IsChecked = true;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(_resourceVm, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_UnCheckedNavigationItemViewModel_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = false;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            NavigationViewModel navigationViewModel = CreateNavigationViewModel(Guid.NewGuid());
            navigationViewModel.Environments.Add(environmentModel);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(_resourceVm, navigationViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, CreateResourceModel().Object);

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            NavigationViewModel navigationViewModel = CreateNavigationViewModel(Guid.NewGuid());
            navigationViewModel.Environments.Add(environmentModel);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(navigationItemViewModel, navigationViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourcesOnEnvironmentModel_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            _resourceVm.IsChecked = true;

            Mock<IEnvironmentModel> mockEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
            mockEnvironmentModel.Setup(e => e.ResourceRepository).Returns<object>(null);

            IEnvironmentModel environmentModel = mockEnvironmentModel.Object;

            bool expected = false;
            NavigationViewModel navigationViewModel = CreateNavigationViewModel(Guid.NewGuid());
            navigationViewModel.Environments.Add(environmentModel);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(_resourceVm, navigationViewModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentContainsResourceWithSameIDButDifferentName_ExpectedTrue()
        {
            Guid resourceGuid = Guid.NewGuid();
            Setup(resourceGuid, "OtherResource");
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, resourceGuid);

            _resourceVm.IsChecked = true;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            bool expected = true;
            NavigationViewModel navigationViewModel = CreateNavigationViewModel(Guid.NewGuid());
            navigationViewModel.Environments.Add(environmentModel);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(_resourceVm, navigationViewModel);

            Assert.AreEqual(expected, actual);
            Assert.IsTrue(DeployStatsCalculator.ConflictingResources.Count > 0);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentDoesntContainResource_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            _resourceVm.IsChecked = true;
            ResourceTreeViewModel vm = _resourceVm;

            vm.DataContext = resourceModel.Object;

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            bool expected = false;
            NavigationViewModel navigationViewModel = CreateNavigationViewModel(Guid.NewGuid());
            navigationViewModel.Environments.Add(environmentModel);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(_resourceVm, navigationViewModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion DeploySummaryPredicateExisting

        #region DeploySummaryPredicateNew

        [TestMethod]
        public void DeploySummaryPredicateNew_NullNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(null, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullEnvironmentModel_Expected_False()
        {
            Setup();
            ImportService.CurrentContext = _importContext;

            var eventAggregator = new Mock<IEventAggregator>().Object;

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var resourceModel = CreateResourceModel();
            _environmentVm = new EnvironmentTreeViewModel(eventAggregator, _rootVm, envModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            _serviceTypeVm = new ServiceTypeTreeViewModel(eventAggregator, _environmentVm, ResourceType.WorkflowService);
            _categoryVm = new CategoryTreeViewModel(eventAggregator, _serviceTypeVm, "Test Category", _mockResourceModel.Object.ResourceType);
            _resourceVm = new ResourceTreeViewModel(eventAggregator, _categoryVm, resourceModel.Object);

            _resourceVm.IsChecked = true;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(_resourceVm, null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_UnCheckedNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(_resourceVm, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;


            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, CreateResourceModel().Object);
            //TreeViewModelFactory.Create(
            //    null,
            //    null, false);

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourcesOnEnvironmentModel_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, resourceModel.Object);

            Mock<IEnvironmentModel> mockEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
            mockEnvironmentModel.Setup(e => e.ResourceRepository).Returns<object>(null);

            IEnvironmentModel environmentModel = mockEnvironmentModel.Object;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentContainsResource_Expected_False()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, resourceModel.Object);

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            bool expected = false;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentDoesntContainResource_Expected_True()
        {
            ImportService.CurrentContext = _importContext;

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            ITreeNode navigationItemViewModel = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object, resourceModel.Object);

            navigationItemViewModel.IsChecked = true;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            bool expected = true;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.AreEqual(expected, actual);
        }

        #endregion DeploySummaryPredicateNew

        #endregion Test Methods

        static NavigationViewModel CreateNavigationViewModel(Guid? context = null)
        {
            return new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, context, EnvironmentRepository.Instance);
        }
    }
}
