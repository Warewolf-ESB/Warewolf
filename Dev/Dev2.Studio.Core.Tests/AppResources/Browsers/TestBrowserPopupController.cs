using System.Windows;
using System.Windows.Media.Imaging;
using Dev2.Studio.Core.AppResources.Browsers;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    public class TestBrowserPopupController : BrowserPopupController
    {
        public TestBrowserPopupController()
            : base(new Window { Title = "Test Title", Icon = new BitmapImage() }, false)
        {
        }

        public int ShowDialogHitCount { get; set; }
        public BrowserPopup Popup { get; set; }

        protected override void ShowDialog(BrowserPopup popup)
        {
            ShowDialogHitCount++;
            Popup = popup;
        }
    }
}
