using System;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExplorerViewModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsNullExceptionForEnvironmentRepo()
        {
            new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null);
        }

        static void Setup()
        {
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            ImportService.CurrentContext = ctx;
        }

        [TestMethod]
        public void HandlesAddServerToExplorerMessage()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<AddServerToExplorerMessage>));
        }


        [TestMethod]
        public void HandlesRemoveEnvironmentMessage()
        {
            //------Setup---------
           // Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<RemoveEnvironmentMessage>));
        }

        [TestMethod]
        public void HandlesEnvironmentDeletedMessage()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<EnvironmentDeletedMessage>));
        }

        [TestMethod]
        public void RemoveEnvironmentMessageRemovesEnvironmentFromNavigationViewModel()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            var msg = new RemoveEnvironmentMessage(mockEnvironment.Object, vm.Context);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 0);

        }

        [TestMethod]
        public void EnvironmentDeletedMessageRemovesEnvironmentFromNavigationViewModel()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            var msg = new EnvironmentDeletedMessage(mockEnvironment.Object);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 0);

        }

        [TestMethod]
        public void RemoveEnvironmentMessageRemovesFromRepo()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();
            mockEnvironment.Setup(e => e.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);

            //------Assert---------
            Assert.AreEqual(vm.EnvironmentRepository.All().Count, 1);

            var msg = new RemoveEnvironmentMessage(mockEnvironment.Object, vm.Context);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.EnvironmentRepository.All().Count, 1);
        }


        [TestMethod]
        public void HandlesUpdateExplorerMessage()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<UpdateExplorerMessage>));
        }

        [TestMethod]
        public void AddServerToExplorerMessageWithCorrectContextExpectsEnvironmentAdded()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            //------Execute---------
            var secondEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            repo.Save(secondEnvironment.Object);

            var msg = new AddServerToExplorerMessage(secondEnvironment.Object, vm.Context);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(2, vm.NavigationViewModel.Environments.Count);
        }

        [TestMethod]
        public void AddServerToExplorerMessageWithInCorrectContextExpectsEnvironmentNotAdded()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            //------Execute---------
            var secondEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            repo.Save(secondEnvironment.Object);

            var msg = new AddServerToExplorerMessage(secondEnvironment.Object, Guid.NewGuid());
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);
        }

        [TestMethod]
        public void AddServerToExplorerMessageWithForceConnectTrueExpectsEnvironmentToConnect()
        {
            //------Setup---------
            //Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo); vm.LoadEnvironments();

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            //------Execute---------
            var secondEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            secondEnvironment.Setup(c => c.Connect()).Verifiable();
            repo.Save(secondEnvironment.Object);

            var msg = new AddServerToExplorerMessage(secondEnvironment.Object, vm.Context, true);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(2, vm.NavigationViewModel.Environments.Count);
            secondEnvironment.Verify(c => c.Connect(), Times.Once());
        }


        [TestMethod]
        [TestCategory("ExplorerViewModel_Constructor")]
        [Description("ExplorerViewModel Constructor must subscribe to NavigationViewModel.LoadResourcesCompleted event and unsubscribe from it on invocation.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ExplorerViewModel_UnitTest_Constructor_SubscribesUnsubscribesNavigationViewModelLoadResourcesCompleted()
        // ReSharper restore InconsistentNaming
        {
            //CompositionInitializer.InitializeForMeflessBaseViewModel();

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var actionHitCount = 0;
            var action = new System.Action(() =>
            {
                actionHitCount++;
            });

            // Create view model with connected localhost - should invoke our action
            var viewModel = new ExplorerViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, false, enDsfActivityType.All, action);
            viewModel.LoadEnvironments();
            Assert.AreEqual(1, actionHitCount, "Constructor did not subscribe to NavigationViewModel.LoadResourcesCompleted.");


            // Add new server - should not invoke our action a second time
            var newServer = new Mock<IEnvironmentModel>();
            newServer.Setup(e => e.ID).Returns(Guid.NewGuid);
            newServer.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var newServerMessage = new AddServerToExplorerMessage(newServer.Object, viewModel.Context);
            viewModel.Handle(newServerMessage);

            Assert.AreEqual(1, actionHitCount, "NavigationViewModel.LoadResourcesCompleted Event handler did not unsubscribe itself.");
        }


        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object);
            repo.IsLoaded = true;
            return repo;
        }
    }
}
