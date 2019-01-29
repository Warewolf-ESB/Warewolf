/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Studio.Tests.ViewModels.Workflow
{
    [TestClass]
    public class DsfActivityDropViewModelTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_Okay_SelectedItem_NotNull_ExpectDialogResult_Okay()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new DsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Service))
            {
                dsfActivityDropViewModel.Okay();
                dialogResult = dsfActivityDropViewModel.DialogResult;
                closeRequested = dsfActivityDropViewModel.CloseRequested;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Okay, dialogResult);
            Assert.IsTrue(closeRequested);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_Okay_SelectedItem_IsNull_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new DsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Service))
            {
                dsfActivityDropViewModel.Okay();
                dialogResult = dsfActivityDropViewModel.DialogResult;
                closeRequested = dsfActivityDropViewModel.CloseRequested;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Cancel, dialogResult);
            Assert.IsFalse(closeRequested);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_SelectedResourceModel_ActivityType_Workflow_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockContextualResourceModel = new Mock<IContextualResourceModel>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            IContextualResourceModel selectedResourceModel;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new TestDsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Workflow))
            {
                dsfActivityDropViewModel.SelectedResourceModel = mockContextualResourceModel.Object;
                dialogResult = dsfActivityDropViewModel.DialogResult;
                closeRequested = dsfActivityDropViewModel.CloseRequested;
                selectedResourceModel = dsfActivityDropViewModel.SelectedResourceModel;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Cancel, dialogResult);
            Assert.IsFalse(closeRequested);
            Assert.AreEqual(mockContextualResourceModel.Object, selectedResourceModel);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_CanOkay_ActivityType_Workflow_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            var canOkay = false;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new TestDsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Workflow))
            {
                canOkay = dsfActivityDropViewModel.CanOkay;
                dialogResult = dsfActivityDropViewModel.DialogResult;
                closeRequested = dsfActivityDropViewModel.CloseRequested;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Cancel, dialogResult);
            Assert.IsFalse(closeRequested);
            Assert.IsFalse(canOkay);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_CancelCommand_ActivityType_Workflow_ExpectDialogResult_Okay()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            var tittle = "";
            var imageSource = "";
            var sender = new object();
            ICommand cancelCommand;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new TestDsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Workflow, sender, mockExplorerTreeItem.Object))
            {
                cancelCommand = dsfActivityDropViewModel.CancelCommand;
                cancelCommand.Execute(null);
                closeRequested = dsfActivityDropViewModel.CloseRequested;
                tittle = dsfActivityDropViewModel.Title;
                imageSource = dsfActivityDropViewModel.ImageSource;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Okay, dialogResult);
            Assert.IsTrue(closeRequested);
            Assert.AreEqual("Select A Service", tittle);
            Assert.AreEqual("Workflow-32", imageSource);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfActivityDropViewModel))]
        public void DsfActivityDropViewModel_Okay_ActivityType_Workflow_ExpectDialogResult_Okayq()
        {
            //---------------------Arrange---------------------
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();

            var dialogResult = new ViewModelDialogResults();
            var closeRequested = false;
            var tittle = "";
            var imageSource = "";
            var sender = new object();
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new TestDsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Workflow, sender, null, null))
            {
                dsfActivityDropViewModel.Okay();
                closeRequested = dsfActivityDropViewModel.CloseRequested;
                tittle = dsfActivityDropViewModel.Title;
                imageSource = dsfActivityDropViewModel.ImageSource;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Okay, dialogResult);
            Assert.IsFalse(closeRequested);
            Assert.AreEqual("Select A Service", tittle);
            Assert.AreEqual("Workflow-32", imageSource);
        }

        class TestDsfActivityDropViewModel : DsfActivityDropViewModel
        {
            private object sender;
            private IExplorerTreeItem @object;

            public TestDsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType)
                : base(explorerViewModel, dsfActivityType)
            {
            }

            public TestDsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType, object sender, IExplorerTreeItem @object) : this(explorerViewModel, dsfActivityType)
            {
                this.sender = sender;
                this.@object = @object;
            }

            public TestDsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType, object sender, IExplorerTreeItem e, IServerRepository @object)
                : base(explorerViewModel, dsfActivityType)
            {
            }
        }

    }
}
