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
using System.Activities;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.ManualResumption;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.ManualResumption
{
    [TestClass]
    public class ManualResumptionViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionViewModel))]
        public void ManualResumptionViewModel_Constructor_Default_ModelItemIsValid_Initialized()
        {
            var modelItem = CreateEmptyModelItem();
            var viewModel = new ManualResumptionViewModel(modelItem);
            viewModel.Validate();
            Assert.IsTrue(viewModel.HasLargeView);
            Assert.AreEqual("Use the Manual Resumption tool when you need to resume execution of a workflow before the time scheduled in the Suspend Execution tool.", viewModel.HelpText);
            Assert.AreEqual("", viewModel.DataFuncDisplayName);
            Assert.IsNull(viewModel.DataFuncIcon);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionViewModel))]
        public void ManualResumptionViewModel_UpdateHelp_ShouldCallToHelpViewModel()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new ManualResumptionViewModel(CreateEmptyModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionViewModel))]
        public void ManualResumptionViewModel_Constructor_Detailed_ModelItemIsValid_Initialized()
        {
            var modelItem = CreateModelItem();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);

            var viewModel = new ManualResumptionViewModel(modelItem);
            viewModel.Validate();
            Assert.IsTrue(viewModel.HasLargeView);
            Assert.AreEqual("Use the Manual Resumption tool when you need to resume execution of a workflow before the time scheduled in the Suspend Execution tool.", viewModel.HelpText);
            Assert.AreEqual("Assign", viewModel.DataFuncDisplayName);
            mockApplicationAdapter.Verify(model => model.TryFindResource("Explorer-WorkflowService"), Times.Once());
            Assert.IsNull(viewModel.DataFuncIcon);
        }

        private static ModelItem CreateEmptyModelItem()
        {
           return ModelItemUtils.CreateModelItem(new ManualResumptionActivity());
        }

        private static ModelItem CreateModelItem()
        {
            var uniqueId = Guid.NewGuid().ToString();
            var commonAssign = CommonAssign();

            var resumptionActivity = new ManualResumptionActivity
            {
                UniqueID = uniqueId,
                DisplayName = "Inner Activity",
                OverrideDataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };

            return ModelItemUtils.CreateModelItem(resumptionActivity);
        }

        private static DsfMultiAssignActivity CommonAssign(Guid? uniqueId = null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }
    }
}
