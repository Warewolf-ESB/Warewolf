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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Diagnostics.Test;
using Dev2.PerformanceCounters.Management;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Clusters;
using Dev2.Settings.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
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
    }
}
