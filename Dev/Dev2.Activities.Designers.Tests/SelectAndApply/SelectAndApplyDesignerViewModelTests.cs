using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.SelectAndApply
{
    [TestClass]
    public class SelectAndApplyDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SelectAndApplyDesignerViewModel))]
        public void SelectAndApplyDesignerViewModel_DataFuncDisplayName_DataFuncIcon()
        {
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);
            //------------Execute Test---------------------------
            using (var viewModel = new SelectAndApplyDesignerViewModel(CreateModelItem(), mockMainViewModel.Object))
            {
                //------------Assert Results-------------------------
                Assert.IsTrue(viewModel.HasLargeView);
                Assert.AreEqual("Assign", viewModel.DataFuncDisplayName);
                mockApplicationAdapter.Verify(model => model.TryFindResource("Explorer-WorkflowService"), Times.Once());
                Assert.IsNull(viewModel.DataFuncIcon);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SelectAndApplyDesignerViewModel))]
        public void SelectAndApplyDesignerViewModel_NullDataFunc()
        {
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);
            //------------Execute Test---------------------------
            using (var viewModel = new SelectAndApplyDesignerViewModel(CreateIncorrectModelItem(), mockMainViewModel.Object))
            {
                //------------Assert Results-------------------------
                Assert.IsTrue(viewModel.HasLargeView);
                Assert.AreEqual("", viewModel.DataFuncDisplayName);
                Assert.IsNull(viewModel.DataFuncIcon);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SelectAndApplyDesignerViewModel))]
        public void SelectAndApplyDesignerViewModel_ShouldCall_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            using (var viewModel = new SelectAndApplyDesignerViewModel(CreateEmptyModelItem(), mockMainViewModel.Object))
            {
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
                //------------Assert Results-------------------------
                mockHelpViewModel.Verify(model => model.UpdateHelpText("help"), Times.Once());
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_LoopConstruct_Select_and_Apply, viewModel.HelpText);
                Assert.IsTrue(viewModel.HasLargeView);
                Assert.AreEqual("", viewModel.DataFuncDisplayName);
                Assert.IsNull(viewModel.DataFuncIcon);
            }
        }

        ModelItem CreateModelItem()
        {
            var uniqueId = Guid.NewGuid().ToString();
            var commonAssign = CommonAssign();

            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                ApplyActivityFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };

            return ModelItemUtils.CreateModelItem(dsfSelectAndApplyActivity);
        }

        ModelItem CreateEmptyModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSelectAndApplyActivity());
        }

        ModelItem CreateIncorrectModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
        }

        DsfMultiAssignActivity CommonAssign(Guid? uniqueId = null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }
    }
}
