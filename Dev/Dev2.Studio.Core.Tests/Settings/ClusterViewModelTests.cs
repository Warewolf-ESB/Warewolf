/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Settings.Clusters;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var viewModel = new ClusterViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
            viewModel.CloseHelpCommand.Execute(null);
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_CopyKeyCommand()
        {
            //------------Setup for test--------------------------
            var viewModel = new ClusterViewModel();
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
            
            var viewModel = new ClusterViewModel();
            
            viewModel.TestKeyCommand.Execute(null);
            mockPopupController.Verify(model => model.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_Filter()
        {
            //------------Setup for test--------------------------
            var viewModel = new ClusterViewModel();
            //------------Execute Test---------------------------
            Assert.AreEqual(6, viewModel.Followers.Count());

            viewModel.Filter = "One";
            Assert.AreEqual(1, viewModel.Followers.Count());
            
            viewModel.Filter = "";
            Assert.AreEqual(6, viewModel.Followers.Count());
            
            viewModel.Filter = "one";
            Assert.AreEqual(1, viewModel.Followers.Count());
            
            viewModel.Filter = "";
            Assert.AreEqual(6, viewModel.Followers.Count());
            
            viewModel.Filter = "ONE";
            Assert.AreEqual(1, viewModel.Followers.Count());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterViewModel))]
        public void ClusterViewModel_Servers()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var clusterViewModel = new ClusterViewModel();
            Assert.AreEqual(6, clusterViewModel.Servers.Count);
        }
    }
}
