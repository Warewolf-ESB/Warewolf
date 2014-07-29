using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

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
            // ReSharper disable ObjectCreationAsStatement
            new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void HandlesRemoveEnvironmentMessage()
        {
            //------Setup---------
            // Setup();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            vm.LoadEnvironments();

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
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<EnvironmentDeletedMessage>));
        }

        [TestMethod]
        public void EnvironmentDeletedMessageRemovesEnvironmentFromNavigationViewModel()
        {
            //------Setup---------
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            vm.OnLoadCompletion(mockEnvironment.Object)(Guid.NewGuid());
            var msg = new EnvironmentDeletedMessage(mockEnvironment.Object);
            //-------Execution-------
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
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            vm.LoadEnvironments();
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
            var vm = new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, repo, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            vm.LoadEnvironments();

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<UpdateExplorerMessage>));
        }

        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            return repo;
        }
    }
}
