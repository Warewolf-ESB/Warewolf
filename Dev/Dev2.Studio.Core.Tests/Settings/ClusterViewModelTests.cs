/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Settings.Clusters;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Client;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Options;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [TestCategory("Studio Settings Core")]
    public class ClusterViewModelTests
    {

        [TestInitialize]
        public void SetupForTest()
        {
            
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var mockPopupController = new Mock<IPopupController>();
            var viewModel = new ClusterViewModel(new Mock<IResourceRepository>().Object, new Mock<IServer>().Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
            viewModel.CloseHelpCommand.Execute(null);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_ResourceType()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            var viewModel = new ClusterViewModel(new Mock<IResourceRepository>().Object, new Mock<IServer>().Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(IServerSource), viewModel.ResourceType);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_ClusterSettings()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();

            var mockServer = new Mock<IServer>();

            var clusterSettingsData = new ClusterSettingsData
            {
                Key = "zxcvzxcv", 
                LeaderServerResource = new NamedGuid { Name = "test", Value = Guid.NewGuid(),}, 
                LeaderServerKey = "asdfasdf"
            };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(mockServer.Object))
                .Returns(clusterSettingsData);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, mockServer.Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ClusterSettings);
            Assert.AreEqual(clusterSettingsData.Key, viewModel.ClusterSettings.Key);
            Assert.AreEqual(clusterSettingsData.LeaderServerResource, viewModel.ClusterSettings.LeaderServerResource);
            Assert.AreEqual(clusterSettingsData.LeaderServerKey, viewModel.ClusterSettings.LeaderServerKey);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_CopyKeyCommand()
        {
            var expected = new ClusterSettingsData
            {
                Key = "mykey",
                LeaderServerKey = "leaderkey",
                LeaderServerResource = new NamedGuid("some resource", Guid.NewGuid()),
            };

            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            var resourceRepository = new Mock<IResourceRepository>();
            resourceRepository.Setup(o => o.GetClusterSettings(It.IsAny<IServer>())).Returns(expected);
            var viewModel = new ClusterViewModel(resourceRepository.Object, new Mock<IServer>().Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            var canExecute = viewModel.CopyKeyCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_IsTestKeyEnabled()
        {
            var mockPopupController = new Mock<IPopupController>();
            
            var mockHubProxyWrapper = new Mock<IHubProxyWrapper>();
            var mockSubscriptionWrapper = new Mock<ISubscriptionWrapper>();
            mockHubProxyWrapper.Setup(o => o.Subscribe("Notify")).Returns(mockSubscriptionWrapper.Object);

            var mockHubConnectionWrapper = new Mock<IHubConnectionWrapper>();
            mockHubConnectionWrapper.Setup(o => o.CreateHubProxy(It.IsAny<string>()))
                .Returns(mockHubProxyWrapper.Object);

            var listClient = new ObservableDistributedListClient<ServerFollower>(mockHubConnectionWrapper.Object, "");

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ServerFollowerList)
                .Returns(listClient);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var leaderServerResource = new NamedGuid {Name = "test", Value = Guid.NewGuid(),};
            var clusterSettingsData = new ClusterSettingsData
            {
                LeaderServerResource = leaderServerResource, 
            };
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(mockServer.Object))
                .Returns(clusterSettingsData);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, mockServer.Object, mockPopupController.Object);
            viewModel.SetItem(viewModel);
            Assert.IsFalse(viewModel.IsValidKey);
            Assert.IsFalse(viewModel.IsTestKeyEnabled);

            viewModel.LeaderServerOptions.Leader = leaderServerResource;
            viewModel.ClusterSettings.LeaderServerKey = "asdfasdf";
            
            Assert.IsTrue(viewModel.IsTestKeyEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_TestClusterKey()
        {
            var expected = new ClusterSettingsData
            {
                Key = "mykey",
                LeaderServerKey = "leaderkey",
                LeaderServerResource = new NamedGuid("some resource", Guid.NewGuid()),
            };

            //------------Setup for test--------------------------
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(It.IsAny<IServer>())).Returns(expected);
            mockResourceRepository
                .Setup(o => o.TestClusterSettings(It.IsAny<IServer>(), It.IsAny<ClusterSettingsData>())).Returns(
                    new ExecuteMessage
                    {
                        HasError = false,
                    });
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(model => model.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(),
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            
            CustomContainer.Register(mockPopupController.Object);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, new Mock<IServer>().Object, mockPopupController.Object);
            
            Assert.IsFalse(viewModel.IsValidKey);
            
            viewModel.TestKeyCommand.Execute(viewModel.ClusterSettings.LeaderServerKey);
            mockPopupController.Verify(model => model.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            Assert.IsTrue(viewModel.IsValidKey);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_LeaderServerOptions_IsDirty()
        {
            var mockPopupController = new Mock<IPopupController>();
            
            var mockHubProxyWrapper = new Mock<IHubProxyWrapper>();
            var mockSubscriptionWrapper = new Mock<ISubscriptionWrapper>();
            mockHubProxyWrapper.Setup(o => o.Subscribe("Notify")).Returns(mockSubscriptionWrapper.Object);

            var mockHubConnectionWrapper = new Mock<IHubConnectionWrapper>();
            mockHubConnectionWrapper.Setup(o => o.CreateHubProxy(It.IsAny<string>()))
                .Returns(mockHubProxyWrapper.Object);

            var listClient = new ObservableDistributedListClient<ServerFollower>(mockHubConnectionWrapper.Object, "");

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ServerFollowerList)
                .Returns(listClient);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var leaderServerResource = new NamedGuid {Name = "test", Value = Guid.NewGuid(),};
            var leaderServerResource2 = new NamedGuid {Name = "test2", Value = Guid.NewGuid(),};
            var clusterSettingsData = new ClusterSettingsData
            {
                LeaderServerResource = leaderServerResource, 
            };
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(mockServer.Object))
                .Returns(clusterSettingsData);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, mockServer.Object, mockPopupController.Object);

            viewModel.SetItem(viewModel);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.LeaderServerOptions.Leader = leaderServerResource2;
            Assert.IsFalse(viewModel.IsValidKey);
            Assert.IsTrue(viewModel.IsDirty);
            
            viewModel.LeaderServerOptions.Leader = leaderServerResource;
            
            Assert.IsFalse(viewModel.IsDirty);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_ClusterSettings_IsDirty()
        {
            var mockPopupController = new Mock<IPopupController>();

            var mockHubProxyWrapper = new Mock<IHubProxyWrapper>();
            var mockSubscriptionWrapper = new Mock<ISubscriptionWrapper>();
            mockHubProxyWrapper.Setup(o => o.Subscribe("Notify")).Returns(mockSubscriptionWrapper.Object);

            var mockHubConnectionWrapper = new Mock<IHubConnectionWrapper>();
            mockHubConnectionWrapper.Setup(o => o.CreateHubProxy(It.IsAny<string>()))
                .Returns(mockHubProxyWrapper.Object);

            var listClient = new ObservableDistributedListClient<ServerFollower>(mockHubConnectionWrapper.Object, "");

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ServerFollowerList)
                .Returns(listClient);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var leaderServerResource = new NamedGuid {Name = "test", Value = Guid.NewGuid(),};
            var leaderServerResource2 = new NamedGuid {Name = "test2", Value = Guid.NewGuid(),};
            var clusterSettingsData = new ClusterSettingsData
            {
                LeaderServerResource = leaderServerResource, 
            };
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(mockServer.Object))
                .Returns(clusterSettingsData);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, mockServer.Object, mockPopupController.Object);

            viewModel.SetItem(viewModel);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.ClusterSettings.LeaderServerKey = "asdfasdf";
            
            Assert.IsTrue(viewModel.IsDirty);
            
            viewModel.ClusterSettings.LeaderServerKey = "";
            
            Assert.IsFalse(viewModel.IsDirty);
        }
        
        [TestMethod]
        public void ClusterViewModel_ServerOptions()
        {
            //-------------------------Arrange-----------------------
            var expectedId = Guid.NewGuid();
            const string expectedName = "ServerName";
            var connection = new Data.ServiceModel.Connection {ResourceID = expectedId, ResourceName = expectedName};
            var expected = new List<Data.ServiceModel.Connection>
            {
                connection,
            };
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o =>
                o.FindSourcesByType<Data.ServiceModel.Connection>(It.IsAny<IServer>(), It.IsAny<enSourceType>())).Returns(expected);
            
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            
            CustomContainer.Register(mockShellViewModel.Object);
            
            //-------------------------Act---------------------------
            var serverOptions = new LeaderServerOptions();
            var result = OptionConvertor.Convert(serverOptions);

            //-------------------------Assert------------------------
            Assert.IsNotNull(result);
            var optionSourceCombobox = result[0] as OptionSourceCombobox;
            Assert.IsNotNull(optionSourceCombobox);
            Assert.AreEqual("Leader", optionSourceCombobox.Name);
            Assert.AreEqual(expectedName, optionSourceCombobox.Options[0].Name);
            Assert.AreEqual(expectedId, optionSourceCombobox.Options[0].Value);
            
            mockResourceRepository.Verify(o => 
                o.FindSourcesByType<Data.ServiceModel.Connection>(It.IsAny<IServer>(), It.IsAny<enSourceType>()), Times.Exactly(1));
        }
    }
}
