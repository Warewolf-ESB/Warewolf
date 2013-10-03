using System;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    [TestClass]
    public class ConnectControlTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControl_BuildConnectViewModel_WithNullDeployResourceAndNullActiveEnvironment_DoesNotCreateConnectViewModel()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            ConnectControl.BuildConnectControlViewModel(null, null);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControl_BuildConnectViewModel_WithDeployResourceWithNullEnvironmentAndNullActiveEnvironment_DoesNotCreateConnectViewModel()
        {
            //------------Setup for test--------------------------
            var resourceModel = new ResourceModel(null);
            //------------Execute Test---------------------------
            ConnectControl.BuildConnectControlViewModel(resourceModel, null);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        public void ConnectControl_BuildConnectViewModel_WithDeployResourceWithNullEnvironmentAndActiveEnvironment_CreateConnectViewModelWithActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceModel = new ResourceModel(null);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = ConnectControl.BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(mainViewModelActiveEnvironment,controlViewModel.ActiveEnvironment);
        } 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        public void ConnectControl_BuildConnectViewModel_WithDeployResourceAsResourceModelWithEnvironmentAndActiveEnvironment_CreateConnectViewModelWithResourceModelEnvironmentAsActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceModelEnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var resourceModel = new ResourceModel(resourceModelEnvironmentModel);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = ConnectControl.BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(resourceModelEnvironmentModel, controlViewModel.ActiveEnvironment);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControl_BuildConnectViewModel")]
        public void ConnectControl_BuildConnectViewModel_WithDeployResourceAsAbstractTreeViewModelWithEnvironmentAndActiveEnvironment_CreateConnectViewModelWithAbstractTreeViewModelEnvironmentAsActiveEnvironment()
        {
            //------------Setup for test--------------------------
            var treeViewModelEnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var resourceModel = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object,null,treeViewModelEnvironmentModel);
            var mainViewModelActiveEnvironment = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var controlViewModel = ConnectControl.BuildConnectControlViewModel(resourceModel, mainViewModelActiveEnvironment);
            //------------Assert Results-------------------------
            Assert.IsNotNull(controlViewModel);
            Assert.IsNotNull(controlViewModel.ActiveEnvironment);
            Assert.AreEqual(treeViewModelEnvironmentModel, controlViewModel.ActiveEnvironment);
        }
    }
}
