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
using Dev2.Settings.Clusters;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
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
                Key = "zxcvzxcv", LeaderServerResourceId = Guid.NewGuid(), LeaderServerKey = "asdfasdf"
            };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.GetClusterSettings(mockServer.Object))
                .Returns(clusterSettingsData);
            
            var viewModel = new ClusterViewModel(mockResourceRepository.Object, mockServer.Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ClusterSettings);
            Assert.AreEqual(clusterSettingsData.Key, viewModel.ClusterSettings.Key);
            Assert.AreEqual(clusterSettingsData.LeaderServerResourceId, viewModel.ClusterSettings.LeaderServerResourceId);
            Assert.AreEqual(clusterSettingsData.LeaderServerKey, viewModel.ClusterSettings.LeaderServerKey);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_CopyKeyCommand()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            var viewModel = new ClusterViewModel(new Mock<IResourceRepository>().Object, new Mock<IServer>().Object, mockPopupController.Object);
            //------------Execute Test---------------------------
            var canExecute = viewModel.CopyKeyCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_TestClusterKey()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(model => model.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(),
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            
            CustomContainer.Register(mockPopupController.Object);
            
            var viewModel = new ClusterViewModel(new Mock<IResourceRepository>().Object, new Mock<IServer>().Object, mockPopupController.Object);
            
            viewModel.TestKeyCommand.Execute(null);
            mockPopupController.Verify(model => model.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
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
