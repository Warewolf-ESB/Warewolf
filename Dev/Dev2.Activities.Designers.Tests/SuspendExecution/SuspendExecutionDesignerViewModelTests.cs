/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SuspendExecution;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.SuspendExecution
{
    [TestClass]
    public class SuspendExecutionDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_Constructor_ModelItemIsValid_Initialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.AreEqual(enSuspendOption.SuspendUntil, viewModel.SuspendOption);
            Assert.AreEqual("Suspend until:", viewModel.SelectedSuspendOption);
            Assert.AreEqual(viewModel.SuspendOptionWatermark, "Date Time");
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_UpdateHelp_ShouldCallToHelpViewModel()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new SuspendExecutionDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_Constructor_ModelItemIsValid_SuspendOptionsItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            Assert.AreEqual(6, viewModel.SuspendOptions.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendUntil()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend until:";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendUntil, viewModel.SuspendOption);
            Assert.AreEqual("Date Time", viewModel.SuspendOptionWatermark);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendForSeconds()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend for Second(s):";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendForSeconds, viewModel.SuspendOption);
            Assert.AreEqual("Second(s)", viewModel.SuspendOptionWatermark);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendForMinutes()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend for Minute(s):";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendForMinutes, viewModel.SuspendOption);
            Assert.AreEqual("Minute(s)", viewModel.SuspendOptionWatermark);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendForHours()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend for Hour(s):";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendForHours, viewModel.SuspendOption);
            Assert.AreEqual("Hour(s)", viewModel.SuspendOptionWatermark);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendForDays()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend for Day(s):";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendForDays, viewModel.SuspendOption);
            Assert.AreEqual("Day(s)", viewModel.SuspendOptionWatermark);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionDesignerViewModel))]
        public void SuspendExecutionDesignerViewModel_SelectedSuspendOption_SuspendForMonths()
        {
            var modelItem = CreateModelItem();
            var viewModel = new SuspendExecutionDesignerViewModel(modelItem);
            const string expectedValue = "Suspend for Month(s):";
            viewModel.SelectedSuspendOption = expectedValue;
            Assert.AreEqual(enSuspendOption.SuspendForMonths, viewModel.SuspendOption);
            Assert.AreEqual("Month(s)", viewModel.SuspendOptionWatermark);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new SuspendExecutionActivity());
        }
    }
}
