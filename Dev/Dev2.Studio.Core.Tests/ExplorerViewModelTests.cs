using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ExplorerViewModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsNullExceptionForEnvironmentRepo()
        {
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            var vm = new ExplorerViewModel(null);
        }

        [TestMethod]
        public void HandlesAddServerToExplorerMessage()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<AddServerToExplorerMessage>));
        }

        [TestMethod]
        public void HandlesRemoveEnvironmentMessage()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<RemoveEnvironmentMessage>));
        }

        public void RemoveEnvironmentMessageRemovesEnvironmentFromNavigationViewModel()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);

            var msg = new RemoveEnvironmentMessage(mockEnvironment.Object);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 0);
            
        }

        public void RemoveEnvironmentMessageDoesNotRemovesEnvironmentFromRepository()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //------Assert---------
            Assert.AreEqual(vm.EnvironmentRepository.All().Count, 1);

            var msg = new RemoveEnvironmentMessage(mockEnvironment.Object);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.EnvironmentRepository.All().Count, 1);
        }


        [TestMethod]
        public void HandlesUpdateExplorerMessage()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //-----Assert-----
            Assert.IsInstanceOfType(vm, typeof(IHandle<UpdateExplorerMessage>));
        }

        [TestMethod]
        public void AddServerToExplorerMessageWithCorrectContextExpectsEnvironmentAdded()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 1);  

            //------Execute---------
            var secondEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            repo.Save(secondEnvironment.Object);

            var msg = new AddServerToExplorerMessage(secondEnvironment.Object, vm.Context);
            vm.Handle(msg);

            //------Assert---------
            Assert.AreEqual(vm.NavigationViewModel.Environments.Count, 2);
        }

        [TestMethod]
        public void AddServerToExplorerMessageWithInCorrectContextExpectsEnvironmentNotAdded()
        {
            //------Setup---------
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            var repo = GetEnvironmentRepository(mockEnvironment);
            var vm = new ExplorerViewModel(repo);

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


        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object);
            repo.IsLoaded = true;
            return repo;
        }

        
    }
}
