using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Dev2.Diagnostics;
using Dev2.Services.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class ActivityBaseClassTests
    {
        private static Grid _overlayContent = new Grid();

        private string _iconLocation =
            "pack://application:,,,/Dev2.Studio.Core.Tests;component/Activities/TestImage.png";

        private static ButtonBase _button = new AdornerToggleButton();

        [TestMethod]
        [TestCategory("ActivityViewModelBase_UnitTest")]
        [Description("Base activity view model can initialize")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityViewModelBase_Constructor_EmptyModelItem_ViewModelConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockModel = new Mock<ModelItem>();

            //exe
            var vm = new TestActivityViewModel(mockModel.Object);

            //assert
            Assert.IsInstanceOfType(vm, typeof(ActivityViewModelBase), "Activity view model base cannot initialize");
            Assert.AreEqual(OverlayType.None, vm.ActiveOverlay, "Base activity view model did not initialize its ActiveOverlay property correctly");
            Assert.AreEqual(OverlayType.None, vm.PreviousOverlayType, "Base activity view model did not initialize its PreviousOverlayType property correctly");
        }

        [TestMethod]
        [TestCategory("ActivityCollectionViewModelBase_UnitTest")]
        [Description("Collection view model base can delete rows from the datagrid")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionViewModelBase_DeleteRow_RowAllowsDelete_RowDeleted()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO>() { new ActivityDTO(), new ActivityDTO(), new ActivityDTO(), new ActivityDTO() });
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);
            var vm = new TestActivityCollectionViewModelBase<ActivityDTO>(mockModel.Object) { SelectedIndex = 2 };

            //exe
            Assert.AreEqual(4, vm.CountRows(), "Collection view model base did not initialize as expected");
            vm.DeleteItem();

            //assert
            Assert.AreEqual(3, vm.CountRows(), "Collection view model base cannot delete datagrid rows");
        }

        [TestMethod]
        [TestCategory("ActivityCollectionViewModelBase_UnitTest")]
        [Description("Collection view model base can insert rows into the datagrid")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionViewModelBase_InsertRow_BlankRow_RowInserted()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO>() { new ActivityDTO(), new ActivityDTO(), new ActivityDTO() });
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);
            var vm = new TestActivityCollectionViewModelBase<ActivityDTO>(mockModel.Object) { SelectedIndex = 2 };

            //exe
            Assert.AreEqual(3, vm.CountRows(), "Collection view model base did not initialize as expected");
            vm.InsertItem();

            //assert
            Assert.AreEqual(4, vm.CountRows(), "Collection view model base cannot insert datagrid rows");
        }

        [TestMethod]
        [TestCategory("ActivityViewModelBase_UnitTest")]
        [Description("Base activity view model OnModelItemChanged event changed the view model ModelItem")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityViewModelBase_OnModelItemChanged_ViewModelModelItemChanged()
        // ReSharper restore InconsistentNaming
        {
            //init
            var firstMockModel = new Mock<ModelItem>();
            firstMockModel.Setup(c => c.Name).Returns("First Model Item");
            var vm = new TestActivityViewModel(firstMockModel.Object);
            var secondMockModel = new Mock<ModelItem>();
            secondMockModel.Setup(c => c.Name).Returns("Replacement Model Item");

            //exe
            Assert.AreEqual("First Model Item", vm.ModelItem.Name, "Base activity view model cannot initialize it's model item");
            vm.OnModelItemChanged(secondMockModel.Object);

            //assert
            Assert.AreEqual("Replacement Model Item", vm.ModelItem.Name, "Base activity view model OnModelItemChanged event does not change the view models model item!");
        }

        [TestMethod]
        [Description("Tests that the icon gets sets when the IconLocation property is changed")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_SetIconLocation_ExpectsIconSet()
        {
            /*************************Test*************************/
            var testActivity = new testActivityDesigner
                {
                    IconLocation = _iconLocation
                };

            /*************************Assert*************************/
            Assert.IsNotNull(testActivity.Icon.Drawing);
            Assert.AreEqual(testActivity.IconLocation, _iconLocation);
        }

        [TestMethod]
        [Description("When the is adornerbuttonsShownProperty is set to true expect the adornoptions panel to show its content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_IsAdornerButtonsShown_ExpectsOptionsAdornerShowContent()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);

            /*************************Test*************************/
            testDesigner.IsAdornerButtonsShown = true;

            /*************************Assert*************************/
            adorner.Verify(a => a.ShowContent(), Times.Once());
           
        }

        [TestMethod]
        [Description("When the is adornerbuttonsShownProperty is set to false expect the adornoptions panel to hide its content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_IsAdornerButtonsShownFAlse_ExpectsOptionsAdornerHideContent()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);

            /*************************Test*************************/
            testDesigner.IsAdornerButtonsShown = true;
            testDesigner.IsAdornerButtonsShown = false;

            /*************************Assert*************************/
            adorner.Verify(a => a.ShowContent(), Times.Once());
            adorner.Verify(a => a.HideContent(), Times.Once());
            Assert.IsFalse(testDesigner.IsAdornerButtonsShown);
        }

        [TestMethod]
        [Description("When the overlay type is set to none, hide the overlayadorner")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_OverlayTypeNone_ExpectsOverlayAdornerHidden()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);

            /*************************Test*************************/
            testDesigner.ActiveOverlay = OverlayType.LargeView;

            /*************************Assert*************************/
            adorner.Verify(a => a.ChangeContent(It.IsAny<object>(), It.IsAny<string>()), Times.Once());
            Assert.AreEqual(testDesigner.ActiveOverlay, OverlayType.LargeView);
        }

        [TestMethod]
        [Description("When the overlay type is set to type with no adornerpresenter, hide the overlayadorner content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_OverlayTypeNotInColelction_ExpectsOverlayAdornerHidden()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);

            /*************************Test*************************/
            testDesigner.ActiveOverlay = OverlayType.Mappings;

            /*************************Assert*************************/
            adorner.Verify(a => a.ChangeContent(It.IsAny<object>(), It.IsAny<string>()), Times.Never());
            adorner.Verify(a => a.HideContent(), Times.Once());
            Assert.AreEqual(testDesigner.ActiveOverlay, OverlayType.None);
        }

        [TestMethod]
        [Description("When the overlay type is set to type with adornerpresenter, show overlayadorner content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_OverlayTypeInColelction_ExpectsOverlayContentShown()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);

            /*************************Test*************************/
            testDesigner.ActiveOverlay = OverlayType.LargeView;

            /*************************Assert*************************/
            adorner.Verify(a => a.ChangeContent(presenter.Object.Content, It.IsAny<string>()), Times.Once());
            Assert.AreEqual(testDesigner.ActiveOverlay, OverlayType.LargeView);
        }

        [TestMethod]
        [Description("When an activitydesigner gets initialized, test that the datacontext gets set to viewmodel")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_Initialized_ExpectsDataContextSet()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);

            /*************************Test*************************/
            testDesigner.Initialize(mockUIElementProvider.Object);

            /*************************Assert*************************/
            
            Assert.AreEqual(testDesigner.DataContext.GetType(), typeof(TestActivityViewModel));
        }

        [TestMethod]
        [Description("When the overlay type on the viewmodel is set, the adorner overlay adorner content is shown.")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_OverlayTypeOnViewModelSet_ExpectsOverlayContentShown()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);

            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);
            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);

            /*************************Test*************************/
            var vm = (TestActivityViewModel) testDesigner.DataContext;
            vm.ActiveOverlay = OverlayType.LargeView;

            /*************************Assert*************************/
            adorner.Verify(a => a.ChangeContent(presenter.Object.Content, It.IsAny<string>()), Times.Once());
            Assert.AreEqual(testDesigner.ActiveOverlay, OverlayType.LargeView);
        }

        [TestMethod]
        [Description("When the IsHelpViewCollapsed is set to false on the ViewModel, The IsCollapsed property on the UserConfigurationService is also set to false.")]
        [Owner("Tshepo Ntlhokoa")]       
        public void ActivityDesignerBase_IsHelpViewCollapsedonViewModelSetToFalse_ExpectsIsCollapsedPropertyIsAlsoSetToFalse()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);

            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);
            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);
            var expected = false;

            /*************************Test*************************/
            var vm = (TestActivityViewModel)testDesigner.DataContext;
            vm.IsHelpViewCollapsed = expected;

            /*************************Assert*************************/
            Assert.AreEqual(UserConfigurationService.Instance.Help.IsCollapsed[vm.ModelItem.ItemType], expected);
        }

        [TestMethod]
        [Description("When the IsHelpViewCollapsed is set to true on the ViewModel, The IsCollapsed property on the UserConfigurationService is also set to true.")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerBase_IsHelpViewCollapsedonViewModelSetToTrue_ExpectsIsCollapsedPropertyIsAlsoSetToTrue()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);

            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);
            var adorner = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(adorner.Object);
            var expected = true;

            /*************************Test*************************/
            var vm = (TestActivityViewModel)testDesigner.DataContext;
            vm.IsHelpViewCollapsed = expected;

            /*************************Assert*************************/
            Assert.AreEqual(UserConfigurationService.Instance.Help.IsCollapsed[vm.ModelItem.ItemType], expected);
        }

        [TestMethod]
        [Description("When an adorner is added, the button is added to options")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_AddornerAdded_ExpectsAdornerOptionsAddButtonCalled()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);

            testDesigner.Initialize(mockUIElementProvider.Object);

            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);

            /*************************Test*************************/
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            /*************************Assert*************************/
            adorner.Verify(a => a.AddButton(_button,true), Times.Once());
            adorner.Verify(a => a.RemoveButton(_button), Times.Never());
        }

        [TestMethod]
        [Description("When an adorner is removed, the button is removed from options")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_AddornerRemoved_ExpectsAdornerOptionsRemoveButtonCalled()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);

            testDesigner.Initialize(mockUIElementProvider.Object);

            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            /*************************Test*************************/
            testDesigner.Adorners.Remove(presenter.Object);

            /*************************Assert*************************/
            adorner.Verify(a => a.AddButton(_button, true), Times.Once());
            adorner.Verify(a => a.RemoveButton(_button), Times.Once());
        }

        [TestMethod]
        [Description("When an adorner is removed, the button is removed from options")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_ButtonSelected_ExpectsContentShown()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);

            testDesigner.Initialize(mockUIElementProvider.Object);

            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);
            var overlay = GetOverlayAdorner(testDesigner);
            testDesigner.SetOverlaydorner(overlay.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            /*************************Test*************************/
            testDesigner.CallShowContent(_button);

            /*************************Assert*************************/
            overlay.Verify(a => a.ChangeContent(presenter.Object.Content, It.IsAny<string>()), Times.Once());
            Assert.AreEqual(testDesigner.ActiveOverlay, OverlayType.LargeView);
        }

        [TestMethod]
        [Description("When the activity is selected and is the primary selection the adornoptions panel is expected to show its content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_SelectionChangedPrimary_ExpectsOptionsAdornerShowContent()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var adorner = GetOptionsAdorner(testDesigner);

            testDesigner.SetOptionsAdorner(adorner.Object);
            var selection = new Selection(new[] { testDesigner.ModelItem });

            /*************************Test*************************/
            testDesigner.CallSelectionChange(selection);

            /*************************Assert*************************/
            adorner.Verify(a => a.ShowContent(), Times.Once());
            Assert.IsTrue(testDesigner.IsAdornerButtonsShown);
        }

        [TestMethod]
        [Description("When the activity is selected but is not the primary selection the adornoptions panel is expected to hide its content")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_SelectionChangedNotPrimary_ExpectsOptionsAdornerHideContent()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);
            testDesigner.Initialize(mockUIElementProvider.Object);
            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);
            var selection = new Selection();

            /*************************Test*************************/
            testDesigner.IsAdornerButtonsShown = true;
            testDesigner.CallSelectionChange(selection);         

            /*************************Assert*************************/
            adorner.Verify(a => a.HideContent(), Times.Once());
            Assert.IsFalse(testDesigner.IsAdornerButtonsShown);
        }

        #region helpers

        private static Mock<AdornerPresenterBase> GetMockAdornerPresenter()
        {
            var moq = new Mock<AdornerPresenterBase>();
            moq.Setup(m => m.OverlayType).Returns(OverlayType.LargeView);
            moq.Object.Content = _overlayContent;
            moq.Setup(m => m.Button).Returns(_button);
            return moq;
        }
    
        private static Mock<AbstractOverlayAdorner> GetOverlayAdorner(UIElement adornedElement)
        {
            var moq = new Mock<AbstractOverlayAdorner>(adornedElement);
            moq.Setup(m => m.ChangeContent(It.IsAny<object>(), It.IsAny<string>()))
                .Callback<object, string>((o,s) => moq.Setup(mo => mo.Content).Returns(o));
            return moq;
        }

        private static Mock<AbstractOptionsAdorner> GetOptionsAdorner(UIElement adornedElement)
        {
            var moq = new Mock<AbstractOptionsAdorner>(adornedElement);
            moq.Setup(a => a.ShowContent()).Verifiable();
            moq.Setup(a => a.HideContent()).Verifiable();
            moq.Setup(a => a.AddButton(It.IsAny<ButtonBase>(), false)).Verifiable();
            moq.Setup(a => a.RemoveButton(It.IsAny<ButtonBase>())).Verifiable();
            return moq;
        }

        private static Mock<IUIElementProvider> GetMockUIElementProvider(ActivityDesigner testDesigner)
        {
            var mockUIElementProvider = new Mock<IUIElementProvider>();
            mockUIElementProvider.Setup(p => p.GetColoursBorder(testDesigner)).Returns(new Border());
            mockUIElementProvider.Setup(p => p.GetDisplayNameWidthSetter(testDesigner)).Returns(new Rectangle());
            mockUIElementProvider.Setup(p => p.GetTitleTextBox(testDesigner)).Returns(new TextBox());
            return mockUIElementProvider;
        }

        private static testActivityDesigner GetTestActivityDesigner()
        {
            var ec = new EditingContext();
            var vs = new WorkflowViewStateService(ec);
            ec.Services.Publish(vs);
            var testDesigner = new testActivityDesigner();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var testActivity = new testActivity(debugDispatcher.Object);
            var mtm = new ModelTreeManager(ec);
            mtm.Load(testActivity);
            var mi = mtm.Root;
            testDesigner.ModelItem = mi;
            return testDesigner;
        }

        #endregion helpers
    }

}
