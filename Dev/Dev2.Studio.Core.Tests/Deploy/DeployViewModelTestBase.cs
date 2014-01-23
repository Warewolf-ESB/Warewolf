using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;
using Moq;

namespace Dev2.Core.Tests.Deploy
{
    public abstract class DeployViewModelTestBase
    {
        protected static ImportServiceContext OkayContext = new Mock<ImportServiceContext>().Object;

        #region Setup DeploymentViewModel
        protected static DeployStatsCalculator SetupDeployViewModel(out DeployViewModel deployViewModel)
        {
            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var destination = EnviromentRepositoryTest.CreateMockEnvironment();

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { source.Object, destination.Object });

            var repo = new TestEnvironmentRespository(source.Object, destination.Object);

            var deployStatsCalculator = new DeployStatsCalculator();

            deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, deployStatsCalculator)
            {
                SelectedSourceServer = source.Object,
                SelectedDestinationServer = destination.Object
            };

            var mockEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            deployViewModel.Target.AddEnvironment(mockEnv.Object);
            deployViewModel.Source.AddEnvironment(mockEnv.Object);

            return deployStatsCalculator;
        }
        #endregion


        protected static void SetupResources(DeployStatsCalculator deployStatsCalculator, bool isChecked)
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var rootVm = new RootTreeViewModel(eventAggregator);
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);

            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var environmentVm = new EnvironmentTreeViewModel(eventAggregator, rootVm, mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var serviceTypeVm = new ServiceTypeTreeViewModel(eventAggregator, environmentVm, ResourceType.WorkflowService);
            var categoryVm = new CategoryTreeViewModel(eventAggregator, serviceTypeVm, resourceModel.Object.Category, resourceModel.Object.ResourceType);
            var resourceVm = new ResourceTreeViewModel(eventAggregator, categoryVm, resourceModel.Object);
            resourceVm.IsChecked = isChecked;
            ResourceTreeViewModel vm = resourceVm;
            vm.DataContext = resourceModel.Object;
            IEnvironmentModel environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;
            NavigationViewModel navVm = new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, Guid.NewGuid(), EnvironmentRepository.Instance);
            navVm.Environments.Add(environmentModel);
            deployStatsCalculator.DeploySummaryPredicateExisting(resourceVm, navVm);
        }

        protected static DeployViewModel SetupDeployViewModel(out Mock<IEnvironmentModel> destEnv, out Mock<IEnvironmentModel> destServer)
        {
            ImportService.CurrentContext = OkayContext;

            var destConnection = new Mock<IEnvironmentConnection>();

            destEnv = new Mock<IEnvironmentModel>();
            destEnv.Setup(e => e.Connection).Returns(destConnection.Object);

            destServer = destEnv;

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.Fetch(It.IsAny<IEnvironmentModel>())).Returns(destEnv.Object);

            var servers = new List<IEnvironmentModel> { destEnv.Object };
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            // ReSharper disable once RedundantAssignment
            int deployItemCount;
            var statsCalc = new Mock<IDeployStatsCalculator>();
            statsCalc.Setup(c => c.CalculateStats(It.IsAny<IEnumerable<ITreeNode>>(), It.IsAny<Dictionary<string, Func<ITreeNode, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out deployItemCount));

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, envRepo.Object, new Mock<IEventAggregator>().Object, statsCalc.Object);
            return deployViewModel;
        }

        #region CreateEnvironmentRepositoryMock

        protected static Guid SetupVmForMessages(out IEnvironmentModel server, out DeployViewModel vm, Mock<IEventAggregator> mockEventAggregator = null)
        {
            ImportService.CurrentContext = new Mock<ImportServiceContext>().Object;
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            var envID = env.Object.ID;
            server = env.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { server });
            var repo = CreateEnvironmentRepositoryMock();
            if(mockEventAggregator == null)
            {
                mockEventAggregator = new Mock<IEventAggregator>();
            }
            vm = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo.Object, mockEventAggregator.Object);
            return envID;
        }

        protected static Mock<IEnvironmentRepository> CreateEnvironmentRepositoryMock()
        {
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);


            return repo;
        }

        #endregion
    }
}
