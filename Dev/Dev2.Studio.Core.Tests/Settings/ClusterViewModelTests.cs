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
using Dev2.Common.Interfaces.Help;
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
        public void ClusterViewModel_NewServerCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.NewServerSource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockShellViewModel.Object);

            //------------Execute Test---------------------------
            var clusterViewModel = new ClusterViewModel();
            clusterViewModel.NewServerCommand.Execute(null);
            mockShellViewModel.Verify(model => model.NewServerSource(It.IsAny<string>()), Times.Once());
        }
    }
}
