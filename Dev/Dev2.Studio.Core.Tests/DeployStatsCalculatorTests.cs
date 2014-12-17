
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class DeployStatsCalculatorTests
    {
        readonly Action<System.Action, DispatcherPriority> _Invoke = (a, b) => { };
        #region Class Members

        static readonly DeployStatsCalculator DeployStatsCalculator = new DeployStatsCalculator();


        #endregion Class Members

        #region Initialization

        Mock<IContextualResourceModel> CreateMockResourceModel(Guid resourceId = new Guid(), string mockResourceName = "")
        {
            var mockResourceModel = CreateResourceModel(new Mock<IEventPublisher>().Object, resourceId);
            if(!string.IsNullOrEmpty(mockResourceName))
            {
                mockResourceModel.Setup(c => c.ResourceName).Returns(mockResourceName);
            }
            return mockResourceModel;

        }

        protected StudioResourceRepository CreateModels(bool isChecked, Mock<IContextualResourceModel> mockResourceModel, out IEnvironmentModel environmentModel, out IExplorerItemModel resourceVm, out IExplorerItemModel rootItem)
        {
            Mock<IContextualResourceModel> resourceModel = mockResourceModel;

            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var serverItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server, EnvironmentId = environmentModel.ID, ResourcePath = "", ResourceId = Guid.NewGuid() };
            rootItem = serverItemModel;
            ExplorerItemModel workflowsFolder = new ExplorerItemModel { DisplayName = "WORKFLOWS", ResourceType = ResourceType.Folder, ResourcePath = "WORKFLOWS", EnvironmentId = mockEnvironmentModel.Object.ID, ResourceId = Guid.NewGuid() };
            serverItemModel.Children.Add(workflowsFolder);

            var studioResourceRepository = new StudioResourceRepository(serverItemModel, _Invoke);
            resourceModel.Setup(model => model.Category).Returns("WORKFLOWS\\" + resourceModel.Object.DisplayName);
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            // ReSharper restore ObjectCreationAsStatement
            IEnvironmentModel internalEnvironmentModel = environmentModel;
            studioResourceRepository.GetCurrentEnvironment = () => internalEnvironmentModel.ID;
            //studioResourceRepository.AddResouceItem(resourceModel.Object);
            resourceVm = workflowsFolder.Children[0];
            resourceVm.IsChecked = isChecked;
            return studioResourceRepository;
        }

        static Mock<IContextualResourceModel> CreateResourceModel(IEventPublisher serverEvents, Guid resourceId = new Guid())
        {
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            resourceModel.Setup(r => r.Category).Returns("Testing");
            resourceModel.Setup(r => r.DisplayName).Returns("TestingWF");
            resourceModel.Setup(r => r.ResourceName).Returns("TestingWF");
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(serverEvents);
            resourceModel.Setup(r => r.ID).Returns(resourceId);
            return resourceModel;
        }

        #endregion Initialization

        #region Test Methods

        #region CalculateStats

        [TestMethod]
        public void DeploySummayCalculateStats()
        {
            List<string> exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            List<string> blankCategories = new List<string>();
            var conn = new Mock<IConnectControlSingleton>().Object;
            var studio = new Mock<IStudioResourceRepository>().Object;

           
            List<ExplorerItemModel> items = new List<ExplorerItemModel>();
            var vm1 = new ExplorerItemModel(conn,studio) { ResourcePath = "Services", ResourceType = ResourceType.DbService, IsChecked = true };
            var vm2 = new ExplorerItemModel(conn, studio) { ResourceType = ResourceType.WorkflowService, ResourcePath = "Workflows", IsChecked = true };
            var vm3 = new ExplorerItemModel(conn, studio) { ResourcePath = "Test", ResourceType = ResourceType.Folder, IsChecked = true };
            var vm4 = new ExplorerItemModel(conn, studio) { ResourceType = ResourceType.Server, IsChecked = false };

            items.Add(vm1);
            items.Add(vm2);
            items.Add(vm3);
            items.Add(vm4);

            Dictionary<string, Func<IExplorerItemModel, bool>> predicates = new Dictionary<string, Func<IExplorerItemModel, bool>> { { "Services", n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.DbService | ResourceType.PluginService | ResourceType.WebService, blankCategories, exclusionCategories) }, { "Workflows", n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.WorkflowService, blankCategories, exclusionCategories) }, { "Sources", n => DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(n, ResourceType.DbSource | ResourceType.PluginSource | ResourceType.WebSource | ResourceType.ServerSource | ResourceType.EmailSource, blankCategories, exclusionCategories) }, { "Unknown", n => DeployStatsCalculator.SelectForDeployPredicate(n) } };

            ObservableCollection<DeployStatsTO> expected = new ObservableCollection<DeployStatsTO> { new DeployStatsTO("Services", "1"), new DeployStatsTO("Workflows", "1"), new DeployStatsTO("Sources", "0"), new DeployStatsTO("Unknown", "0") };

            const int expectedDeployItemCount = 2;
            int actualDeployItemCount;
            ObservableCollection<DeployStatsTO> actual = new ObservableCollection<DeployStatsTO>();

            DeployStatsCalculator.CalculateStats(items, predicates, actual, out actualDeployItemCount);

            CollectionAssert.AreEqual(expected, actual, new DeployStatsTOComparer());
            Assert.AreEqual(expectedDeployItemCount, actualDeployItemCount);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsCalculator_Calculate")]
        public void DeployStatsCalculator_Calculate_HasFolders_Expect_NoFoldersCounted()
        {
            //------------Setup for test--------------------------


            var conn = new Mock<IConnectControlSingleton>().Object;
            var studio = new Mock<IStudioResourceRepository>().Object;
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel(conn,studio){ResourceType = ResourceType.Folder,IsChecked = true};
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);
            Assert.IsFalse(actual);

            
        }
        #endregion CalculateStats

        #region SelectForDeployPredicate

        [TestMethod]
        public void SelectForDeployPredicate_NullNavigationItemViewModel_Expected_False()
        {
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(null);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_UncheckedNavigationItemViewModel_Expected_False()
        {
            var conn = new Mock<IConnectControlSingleton>().Object;
            var studio = new Mock<IStudioResourceRepository>().Object;
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel(conn,studio);

            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_NullResourceModelOnNavigationItemViewModel_Expected_False()
        {
            var conn = new Mock<IConnectControlSingleton>().Object;
            var studio = new Mock<IStudioResourceRepository>().Object;
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel(conn,studio);
            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicate_ValidNavigationItemViewModel_Expected_True()
        {
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { IsChecked = true , ResourcePath = "bob"};

            bool actual = DeployStatsCalculator.SelectForDeployPredicate(navigationItemViewModel);

            Assert.IsTrue(actual);
        }

        #endregion SelectForDeployPredicate

        #region SelectForDeployPredicateWithTypeAndCategories

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullNavigationItemViewModel_Expected_False()
        {

            bool actual = DeployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(null, ResourceType.Unknown, new List<string>(), new List<string>());

            Assert.IsFalse(actual);
        }

        [TestMethod]

        public void SelectForDeployPredicateWithTypeAndCategories_UnCheckedNavigationItemViewModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.Parent.IsChecked = false;

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(rootItem, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_TypeMismatch_Expected_False()
        {
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();

            bool actual = DeployStatsCalculator
                .SelectForDeployPredicateWithTypeAndCategories(navigationItemViewModel, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NoCategories_Expected_True()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = true;

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string>());

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InInclusionCategories_Expected_True()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = true;
            resourceVm.ResourcePath = "Testing";

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVm, ResourceType.WorkflowService, new List<string> { "Testing" }, new List<string>());

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInInclusionCategories_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = true;

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVm, ResourceType.WorkflowService, new List<string> { "TestingCake" }, new List<string>());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_InExclusionCategories_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = true;
            resourceVm.ResourcePath = "Testing";

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string> { "Testing" });

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SelectForDeployPredicateWithTypeAndCategories_NotInExclusionCategories_Expected_True()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = true;

            bool actual = DeployStatsCalculator.SelectForDeployPredicateWithTypeAndCategories(resourceVm, ResourceType.WorkflowService, new List<string>(), new List<string> { "TestingCake" });

            Assert.IsTrue(actual);
        }

        #endregion SelectForDeployPredicateWithTypeAndCategories

        #region DeploySummaryPredicateExisting

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullNavigationItemViewModel_Expected_False()
        {

            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(null, null);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullEnvironmentModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);
            resourceVm.IsChecked = true;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(null, null);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_UnCheckedNavigationItemViewModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            StudioResourceRepository studioResourceRepository = CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            resourceVm.IsChecked = false;

            var navigationViewModel = CreateDeployNavigationViewModel(environmentModel, studioResourceRepository);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(resourceVm, navigationViewModel);

            Assert.IsFalse(actual);
        }

        static DeployNavigationViewModel CreateDeployNavigationViewModel(IEnvironmentModel environmentModel, StudioResourceRepository studioResourceRepository)
        {
            return CreateDeployNavigationViewModel(environmentModel, new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IEnvironmentRepository>().Object, studioResourceRepository);
        }

        static DeployNavigationViewModel CreateDeployNavigationViewModel(IEnvironmentModel environmentModel, IEventAggregator eventAggregator, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, StudioResourceRepository studioResourceRepository)
        {
            DeployNavigationViewModel navigationViewModel = new DeployNavigationViewModel(eventAggregator, asyncWorker, environmentRepository, studioResourceRepository, true, new Mock<IConnectControlSingleton>().Object)
            {
                Environment = environmentModel
            };
            return navigationViewModel;
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourceOnNavigationItemViewModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            StudioResourceRepository studioResourceRepository = CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();


            var navigationViewModel = CreateDeployNavigationViewModel(environmentModel, studioResourceRepository);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(navigationItemViewModel, navigationViewModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_NullResourcesOnEnvironmentModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            StudioResourceRepository studioResourceRepository = CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);


            resourceVm.IsChecked = true;

            var navigationViewModel = CreateDeployNavigationViewModel(environmentModel, studioResourceRepository);
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(null, navigationViewModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentContainsResourceWithSameIDButDifferentName_ExpectedTrue()
        {
            Guid resourceGuid = Guid.NewGuid();
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel(resourceGuid, "OtherResource");
            IEnvironmentModel environmentModel;

            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            StudioResourceRepository studioResourceRepository = CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);


            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType.WorkflowService, resourceGuid);


            environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var navigationViewModel = CreateDeployNavigationViewModel(environmentModel, studioResourceRepository);
            ExplorerItemModel explorerItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server };
            explorerItemModel.Children.Add(resourceVm);
            navigationViewModel.ExplorerItemModels.Add(explorerItemModel);
            navigationViewModel.Environment = environmentModel;
            resourceVm.IsChecked = true;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(resourceVm, navigationViewModel);

            Assert.IsTrue(actual);
            Assert.IsTrue(DeployStatsCalculator.ConflictingResources.Count > 0);
        }

        [TestMethod]
        public void DeploySummaryPredicateExisting_EnvironmentDoesntContainResource_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            StudioResourceRepository studioResourceRepository = CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType.WorkflowService, Guid.NewGuid());



            environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            var navigationViewModel = CreateDeployNavigationViewModel(environmentModel, studioResourceRepository);
            resourceVm.IsChecked = true;
            bool actual = DeployStatsCalculator.DeploySummaryPredicateExisting(null, navigationViewModel);

            Assert.IsFalse(actual);
        }

        #endregion DeploySummaryPredicateExisting

        #region DeploySummaryPredicateNew

        [TestMethod]
        public void DeploySummaryPredicateNew_NullNavigationItemViewModel_Expected_False()
        {

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(null, null);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullEnvironmentModel_Expected_False()
        {
            Mock<IContextualResourceModel> mockResourceModel = CreateMockResourceModel();
            IEnvironmentModel environmentModel;
            IExplorerItemModel resourceVm;
            IExplorerItemModel rootItem;
            CreateModels(false, mockResourceModel, out environmentModel, out resourceVm, out rootItem);

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            resourceVm.IsChecked = true;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(null, null);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_UnCheckedNavigationItemViewModel_Expected_False()
        {

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(null, environmentModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourceOnNavigationItemViewModel_Expected_False()
        {


            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel().Object;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_NullResourcesOnEnvironmentModel_Expected_False()
        {

            Dev2MockFactory.SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);

            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();

            Mock<IEnvironmentModel> mockEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
            mockEnvironmentModel.Setup(e => e.ResourceRepository).Returns<object>(null);

            IEnvironmentModel environmentModel = mockEnvironmentModel.Object;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentContainsResource_Expected_False()
        {

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);

            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel();

            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void DeploySummaryPredicateNew_EnvironmentDoesntContainResource_Expected_True()
        {

            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
       
            ExplorerItemModel navigationItemViewModel = new ExplorerItemModel { ResourcePath = "bob", IsChecked = true };
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>(), new List<IResourceModel>()).Object;

            bool actual = DeployStatsCalculator.DeploySummaryPredicateNew(navigationItemViewModel, environmentModel);

            Assert.IsTrue(actual);
        }

        #endregion DeploySummaryPredicateNew

        #endregion Test Methods
    }
}
