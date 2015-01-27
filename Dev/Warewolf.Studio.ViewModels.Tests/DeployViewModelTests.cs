using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DeployViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void DeployViewModel_Ctor_NullParams_ExpectExceptions()
    
        {
            //------------Setup for test--------------------------

            var expDest = new Mock<IExplorerViewModel>();
            var expSource = new Mock<IExplorerViewModel>();
            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            //------------Execute Test---------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, new Mock<IDeployModelFactory>().Object }, typeof(DeployViewModel));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor")]
        public void DeployViewModel_Ctor_ValidParams_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------

            var expDest = new Mock<IExplorerViewModel>();
            var expSource = new Mock<IExplorerViewModel>();
            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object,new Mock<IDeployModelFactory>().Object);
            Assert.AreEqual(constructred.Destination,expDest.Object);
            Assert.AreEqual(constructred.Source, expSource.Object);
            Assert.AreEqual(prov.Object,constructred.StatsProvider);
            Assert.AreEqual(0,constructred.Predicates.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_NoSelectedItems_ExpectCorrectCallsAreMade()
        {
            //------------Setup for test--------------------------

            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
           
            var destModel = new Mock<IDeployModel>();


            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel>();
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();

            var deployFactory = new Mock<IDeployModelFactory>();
            deployFactory.Setup(a => a.Create(expdestEnv.Object)).Returns(sourceModel.Object);
            deployFactory.Setup(a => a.Create(expSourceEnv.Object)).Returns(destModel.Object);

            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.Deploy();
            destModel.Verify(a=>a.Deploy(It.IsAny<IResource>()),Times.Never());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_ASelectedItem_ConflictsNotHandles_ExpectCorrectCallsAreMade()
        {
            //------------Setup for test--------------------------

            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();


            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);

            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = Guid.NewGuid() } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();

            var deployFactory = new Mock<IDeployModelFactory>();
            deployFactory.Setup(a => a.Create(expdestEnv.Object)).Returns(sourceModel.Object);
            deployFactory.Setup(a => a.Create(expSourceEnv.Object)).Returns(destModel.Object);

            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(false);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.Deploy();
            destModel.Verify(a => a.Deploy(It.IsAny<IResource>()), Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_ASelectedItem_ConflictsHandled_CantDeploy_ExpectCorrectCallsAreMade()
        {
            //------------Setup for test--------------------------
            var resourceToDeploy = new Mock<IResource>();
            var id = Guid.NewGuid();
            resourceToDeploy.Setup(a => a.ResourceID).Returns(id);
            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();


            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);
            expSourceEnv.Setup(a => a.Load()).Returns(new List<IResource> { resourceToDeploy.Object});
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();

            var deployFactory = new Mock<IDeployModelFactory>();
            deployFactory.Setup(a => a.Create(expdestEnv.Object)).Returns(sourceModel.Object);
            deployFactory.Setup(a => a.Create(expSourceEnv.Object)).Returns(destModel.Object);

            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(true);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.Deploy();
            destModel.Verify(a => a.Deploy(It.IsAny<IResource>()), Times.Never());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_ASelectedItem_ConflictsHandled_CanDeploy_ExpectDeploy()
        {
            //------------Setup for test--------------------------
            var resourceToDeploy = new Mock<IResource>();
            var id = Guid.NewGuid();
            resourceToDeploy.Setup(a => a.ResourceID).Returns(id);
            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();
            destModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);

            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);
            expSourceEnv.Setup(a => a.Load()).Returns(new List<IResource> { resourceToDeploy.Object });
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();
            sourceModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);
            var deployFactory = new Mock<IDeployModelFactory>();
            deployFactory.Setup(a => a.Create(expdestEnv.Object)).Returns(destModel.Object);
            deployFactory.Setup(a => a.Create(expSourceEnv.Object)).Returns(sourceModel.Object);

            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(true);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.Deploy();
            destModel.Verify(a => a.Deploy(It.IsAny<IResource>()), Times.Once());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_GetDependencies_()
        {
            //------------Setup for test--------------------------
            var resourceToDeploy = new Mock<IResource>();
            var id = Guid.NewGuid();
            resourceToDeploy.Setup(a => a.ResourceID).Returns(id);
            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();
            destModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);

            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);
            expSourceEnv.Setup(a => a.Load()).Returns(new List<IResource> { resourceToDeploy.Object });
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();
            sourceModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);
            var deployFactory = new Mock<IDeployModelFactory>();
            deployFactory.Setup(a => a.Create(expdestEnv.Object)).Returns(destModel.Object);
            deployFactory.Setup(a => a.Create(expSourceEnv.Object)).Returns(sourceModel.Object);

            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(true);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.Deploy();
            destModel.Verify(a => a.Deploy(It.IsAny<IResource>()), Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_CalculateStats")]
        public void DeployViewModel_CalculateStats_ExpectStatsCalculatorCalled()
        {
            //------------Setup for test--------------------------
            ICollection<IExplorerItemViewModel> sourcevms = new Collection<IExplorerItemViewModel>();
             ICollection<IExplorerItemViewModel> destvms = new Collection<IExplorerItemViewModel>();
            var resourceToDeploy = new Mock<IResource>();
            var id = Guid.NewGuid();
            resourceToDeploy.Setup(a => a.ResourceID).Returns(id);
            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expdestEnvVm.Setup(a => a.Children).Returns(destvms);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();
            destModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);

            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expdSourceEnvVm.Setup(a => a.Children).Returns(sourcevms);
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);
            expSourceEnv.Setup(a => a.Load()).Returns(new List<IResource> { resourceToDeploy.Object });
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();
            sourceModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);
            var deployFactory = new Mock<IDeployModelFactory>();


            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(true);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.CalculateStats();
            prov.Verify(a => a.CalculateStats(sourcevms,destvms,It.IsAny<ICollection<IDeployPredicate>>()));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_SelectDependencies")]
        public void DeployViewModel_SelectDependencies_ExpectStatsCalculatorCalled()
        {
            //------------Setup for test--------------------------
            ICollection<IExplorerItemViewModel> sourcevms = new Collection<IExplorerItemViewModel>();
            ICollection<IExplorerItemViewModel> destvms = new Collection<IExplorerItemViewModel>();
            var resourceToDeploy = new Mock<IResource>();
            var id = Guid.NewGuid();
            resourceToDeploy.Setup(a => a.ResourceID).Returns(id);
            var expDest = new Mock<IExplorerViewModel>();
            var expdestEnv = new Mock<IServer>();
            var expdestEnvVm = new Mock<IEnvironmentViewModel>();
            expDest.Setup(a => a.SelectedServer).Returns(expdestEnv.Object);
            expdestEnvVm.Setup(a => a.Children).Returns(destvms);
            expDest.Setup(a => a.SelectedEnvironment).Returns(expdestEnvVm.Object);
            var destModel = new Mock<IDeployModel>();
            destModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);

            var expSource = new Mock<IExplorerViewModel>();
            var expSourceEnv = new Mock<IServer>();
            var expdSourceEnvVm = new Mock<IEnvironmentViewModel>();
            expdSourceEnvVm.Setup(a => a.Children).Returns(sourcevms);
            expSource.Setup(a => a.SelectedServer).Returns(expSourceEnv.Object);
            expSource.Setup(a => a.SelectedEnvironment).Returns(expdSourceEnvVm.Object);
            expSourceEnv.Setup(a => a.Load()).Returns(new List<IResource> { resourceToDeploy.Object });
            IList<IExplorerItemViewModel> items = new List<IExplorerItemViewModel> { new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id } };
            expSource.Setup(a => a.FindItems(It.IsAny<Func<IExplorerItemViewModel, bool>>())).Returns(items);
            var sourceModel = new Mock<IDeployModel>();
            sourceModel.Setup(a => a.CanDeploy(It.IsAny<IResource>())).Returns(true);
            var deployFactory = new Mock<IDeployModelFactory>();


            var prov = new Mock<IDeployStatsProvider>();
            var conflictHandler = new Mock<IConflictHandlerViewModel>();
            // ReSharper disable MaximumChainedReferences
            conflictHandler.Setup(a => a.HandleConflicts(expdSourceEnvVm.Object, expdestEnvVm.Object)).Returns(true);
            // ReSharper restore MaximumChainedReferences

            //------------Execute Test---------------------------
            var constructred = new DeployViewModel(expSource.Object, expDest.Object, prov.Object, new Collection<IDeployPredicate>(), conflictHandler.Object, deployFactory.Object);
            constructred.SelectDependencies(new ExplorerItemViewModel(new Mock<IShellViewModel>().Object, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { Checked = true, ResourceId = id });
        }


        // ReSharper restore InconsistentNaming
    }



}
