
using CefSharp.Wpf;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added    
    public partial class BrowserPopup
    {
        public BrowserPopup()
        {
            InitializeComponent();

            Closed += OnPopupClosed;
            Loaded += (s, e) => _webView.Focus();
        }

        public WebView WebView { get { return _webView; } }

        void OnPopupClosed(object sender, System.EventArgs eventArgs)
        {
            _webView.Dispose();
        }
    }
}
