using System;
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
        const string testOwner = "Siphamandla Dube";
        const string testCategory = nameof(DsfActivityDropViewModel);

        [TestMethod]
        [Owner(testOwner)]
        [TestCategory(testCategory)]
        public void DsfActivityDropViewModel_Okay_SelectedItem_NotNull_ExpectDialogResult_Okay()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            bool closeRequested = false;  
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new DsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Service))
            {
                dsfActivityDropViewModel.Okay();
                dialogResult = dsfActivityDropViewModel.DialogResult;
                closeRequested =  dsfActivityDropViewModel.CloseRequested;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Okay, dialogResult);
            Assert.IsTrue(closeRequested);
        }

        [TestMethod]
        [Owner(testOwner)]
        [TestCategory(testCategory)]
        public void DsfActivityDropViewModel_Okay_SelectedItem_IsNull_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();

            //mockExplorerViewModel.Setup(o => o.SelectedItem).Returns();
            //mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            bool closeRequested = false;
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
        [Owner(testOwner)]
        [TestCategory(testCategory)]
        public void DsfActivityDropViewModel_SelectedResourceModel_ActivityType_Workflow_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockContextualResourceModel = new Mock<IContextualResourceModel>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            bool closeRequested = false;
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
        [Owner(testOwner)]
        [TestCategory(testCategory)]
        public void DsfActivityDropViewModel_CanOkay_ActivityType_Workflow_ExpectDialogResult_Cancel()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            bool closeRequested = false;
            bool canOkay = false;
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
        [Owner(testOwner)]
        [TestCategory(testCategory)]
        public void DsfActivityDropViewModel_CancelCommand_ActivityType_Workflow_ExpectDialogResult_Okay()
        {
            //---------------------Arrange---------------------
            var mockServer = new Mock<IServer>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();

            mockExplorerViewModel.Setup(o => o.SelectedItem.Server).Returns(mockServer.Object);

            var dialogResult = new ViewModelDialogResults();
            bool closeRequested = false;
            var tittle = "";
            var imageSource = "";
            object sender = new object();
            ICommand cancelCommand;
            //---------------------Act-------------------------
            using (var dsfActivityDropViewModel = new TestDsfActivityDropViewModel(mockExplorerViewModel.Object, Enums.enDsfActivityType.Workflow, sender, mockExplorerTreeItem.Object))
            {
                //dsfActivityDropViewModel.TestSingleEnvironmentExplorerViewModel_SelectedItemChanged(sender, mockExplorerTreeItem.Object);
                cancelCommand = dsfActivityDropViewModel.CancelCommand;
                closeRequested = dsfActivityDropViewModel.CloseRequested;
                tittle = dsfActivityDropViewModel.Title;
                imageSource = dsfActivityDropViewModel.ImageSource;
            }
            //---------------------Assert----------------------
            Assert.AreEqual(ViewModelDialogResults.Okay, dialogResult);
            Assert.IsFalse(closeRequested);
            Assert.AreEqual("Select A Service", tittle);
            Assert.AreEqual("Workflow-32", imageSource);
            //Assert.AreEqual(mockExplorerTreeItem.Object, selectedExplorerItemModel);
        }

        class TestDsfActivityDropViewModel : DsfActivityDropViewModel
        {
            public TestDsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType) 
                : base(explorerViewModel, dsfActivityType)
            {
            }

            public TestDsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType, object sender, IExplorerTreeItem e) 
                : base(explorerViewModel, dsfActivityType)
            {
            }
        }

    }
}
