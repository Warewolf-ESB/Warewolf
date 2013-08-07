using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Dev2.Diagnostics;
using Dev2.Studio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class ActivityBaseClassTests
    {
        private static App _app = null;
        private static Grid _overlayContent = new Grid();

        private string _iconLocation =
            "pack://application:,,,/Dev2.Studio.Core.Tests;component/Activities/TestImage.png";

        private static ButtonBase _button = new AdornerToggleButton();

        [TestMethod]
        [Description("Tests that the icon gets sets when the IconLocation property is changed")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_SetIconLocation_ExpectsIconSet()
        {
            /*************************Test*************************/
            var testActivity = new TestActivityDesigner
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
            Assert.IsTrue(testDesigner.IsAdornerButtonsShown);
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
        [Description("When an adorner is added, the button is added to options")]
        [Owner("Jurie Smit")]
        public void ActivityDesignerBase_AddornerAdded_ExpectsAdornerOptionsAddButtonCalled()
        {
            /*************************Setup*************************/
            var testDesigner = GetTestActivityDesigner();
            var mockUIElementProvider = GetMockUIElementProvider(testDesigner);

            testDesigner.BeginInit();
            testDesigner.Initialize(mockUIElementProvider.Object);
            testDesigner.EndInit();

            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);

            /*************************Test*************************/
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            /*************************Assert*************************/
            adorner.Verify(a => a.AddButton(_button), Times.Once());
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

            testDesigner.BeginInit();
            testDesigner.Initialize(mockUIElementProvider.Object);
            testDesigner.EndInit();

            var adorner = GetOptionsAdorner(testDesigner);
            testDesigner.SetOptionsAdorner(adorner.Object);
            var presenter = GetMockAdornerPresenter();
            testDesigner.Adorners.Add(presenter.Object);

            /*************************Test*************************/
            testDesigner.Adorners.Remove(presenter.Object);

            /*************************Assert*************************/
            adorner.Verify(a => a.AddButton(_button), Times.Once());
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

            testDesigner.BeginInit();
            testDesigner.Initialize(mockUIElementProvider.Object);
            testDesigner.EndInit();

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
            moq.Setup(a => a.AddButton(It.IsAny<ButtonBase>())).Verifiable();
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

        private static TestActivityDesigner GetTestActivityDesigner()
        {
            var ec = new EditingContext();
            var vs = new WorkflowViewStateService(ec);
            //var designerView = Activator.CreateInstance(typeof(DesignerView), ec);
            ec.Services.Publish(vs);
            var testDesigner = new TestActivityDesigner();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var testActivity = new TestActivity(debugDispatcher.Object);
            var mtm = new ModelTreeManager(ec);
            mtm.Load(testActivity);
            var mi = mtm.Root;
            testDesigner.ModelItem = mi;
            return testDesigner;
        }

        #endregion helpers
    }

}
