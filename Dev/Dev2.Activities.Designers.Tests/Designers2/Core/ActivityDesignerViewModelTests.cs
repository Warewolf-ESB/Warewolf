using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;
using Dev2.Activities.Designers.Tests.Designers2.Core.Stubs;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Service;
using Dev2.Collections;
using Dev2.Core.Tests;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ActivityDesignerViewModelTests
    {

        [TestMethod]
        [TestCategory("ActivityDesignerViewModel_UnitTest")]
        [Description("Base activity view model can initialize")]
        [Owner("Ashley Lewis")]
        public void ActivityDesignerViewModel_Constructor_EmptyModelItem_ViewModelConstructed()
        {
            //init
            var mockModel = new Mock<ModelItem>();

            //exe
            var vm = new TestActivityDesignerViewModel(mockModel.Object);

            //assert
            Assert.IsInstanceOfType(vm, typeof(ActivityDesignerViewModel), "Activity view model base cannot initialize");
        }

        [TestMethod]
        [TestCategory("ActivityDesignerViewModel_ShowHelp")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerViewModel_ShowHelp_SetToTrue_SetInitialFocusIsCalled()
        {
            var mockModel = new Mock<ModelItem>();
            var vm = new TestActivityDesignerViewModel(mockModel.Object);
            var wasCalled = false;

            vm.SetIntialFocusAction(() =>
            {
                wasCalled = true;
            });

            vm.ShowHelp = true;

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_AddTitleBarHelpToggle")]
        public void ActivityDesignerViewModel_AddTitleBarHelpToggle_Added()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.TestAddTitleBarHelpToggle();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);

            var toggle = viewModel.TitleBarToggles[0];

            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", toggle.CollapseImageSourceUri);
            Assert.AreEqual("Close Help", toggle.CollapseToolTip);
            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", toggle.ExpandImageSourceUri);
            Assert.AreEqual("Open Help", toggle.ExpandToolTip);
            Assert.AreEqual("HelpToggle", toggle.AutomationID);

            var binding = BindingOperations.GetBinding(viewModel, ActivityDesignerViewModel.ShowHelpProperty);
            Assert.IsNotNull(binding);
            Assert.AreEqual(toggle, binding.Source);
            Assert.AreEqual(ActivityDesignerToggle.IsCheckedProperty.Name, binding.Path.Path);
            Assert.AreEqual(BindingMode.TwoWay, binding.Mode);
        }
        
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Collapse")]
        public void ActivityDesignerViewModel_Collapse_SmallViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object) { ShowLarge = true };

            //------------Execute Test---------------------------
            viewModel.Collapse();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsSmallViewActive);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityDesignerViewModel_Collapse")]
        public void ActivityDesignerViewModel_Collapse_HelpButtonGetsRemovedOnCollapse()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            Mock<IContextualResourceModel> setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock();
            ErrorInfo errorInfo = new ErrorInfo { InstanceID = new Guid() };

            IObservableReadOnlyList<IErrorInfo> testErrors = new ObservableReadOnlyList<IErrorInfo> { errorInfo };
            setupResourceModelMock.Setup(c => c.Errors).Returns(testErrors);
            setupResourceModelMock.Setup(c => c.GetErrors(It.IsAny<Guid>())).Returns(new List<IErrorInfo> { errorInfo });
            var viewModel = new ServiceDesignerViewModel(mockModelItem.Object, setupResourceModelMock.Object);

            Assert.AreEqual(2, viewModel.TitleBarToggles.Count);

            viewModel.ShowLarge = true;

            Assert.AreEqual(3, viewModel.TitleBarToggles.Count);

            //------------Execute Test---------------------------
            viewModel.Collapse();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Restore")]
        public void ActivityDesignerViewModel_RestoreFromPreviouslyViewedLargeView_LargeViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);


            viewModel.PreviousView = ActivityDesignerViewModel.ShowLargeProperty.Name;

            //------------Execute Test---------------------------
            viewModel.Restore();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Expand")]
        public void ActivityDesignerViewModel_Expand_LargeViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.Expand();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsSelectedAndSmallViewNotActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsSelectedAndSmallViewActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsSelectedAndSmallViewNotActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsSelectedAndSmallViewActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_IsSelected_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_NotIsSelected_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_IsSelected_Front()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Front, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_NotIsSelected_Back()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Back, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsMouseOverAndSmallViewNotActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsMouseOverAndSmallViewActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsMouseOverAndSmallViewNotActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsMouseOverAndSmallViewActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_IsMouseOver_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_NotIsMouseOver_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_IsMouseOver_Front()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Front, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_NotIsMouseOver_Back()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Back, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ShowItemHelpCommand")]
        public void ActivityDesignerViewModel_ShowItemHelpCommand_InvokesGivenAction()
        {
            //------------Setup for test--------------------------
            Type showExampleType = null;
            var mockModelItem = GenerateMockModelItem();

            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object, type =>
            {
                showExampleType = type;
            });

            //------------Execute Test---------------------------
            viewModel.ShowItemHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(showExampleType);
            Assert.AreEqual(mockModelItem.Object.ItemType, showExampleType);
        }

        static Mock<ModelItem> GenerateMockModelItem()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var displayName = new Mock<ModelProperty>();
            displayName.Setup(p => p.ComputedValue).Returns("Activity");
            properties.Add("DisplayName", displayName);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayName.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(mi => mi.ItemType).Returns(typeof(TestActivity));
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            return mockModelItem;
        }
    }

}
