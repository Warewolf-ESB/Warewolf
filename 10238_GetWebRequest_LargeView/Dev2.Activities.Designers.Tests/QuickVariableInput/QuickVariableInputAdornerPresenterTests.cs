using Dev2.Activities.Adorners;
using Dev2.Activities.QuickVariableInput;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    [TestClass]
    [Ignore] // Views have StaticResources which cannot be found by the test harness!
    public class QuickVariableInputAdornerPresenterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputAdornerPresenter_Constructor")]
        public void QuickVariableInputAdornerPresenter_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var qviAdornerPresenter = new QuickVariableInputAdornerPresenter();

            //------------Assert Results-------------------------
            Assert.AreEqual(OverlayType.QuickVariableInput, qviAdornerPresenter.OverlayType);
            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png", qviAdornerPresenter.ImageSourceUri);
            Assert.AreEqual("Open Quick Variable Input", qviAdornerPresenter.ToolTip);
            Assert.AreEqual("Close Quick Variable Input", qviAdornerPresenter.ExpandedToolTip);
            Assert.IsInstanceOfType(qviAdornerPresenter.Content, typeof(QuickVariableInputView));
        }
    }
}
