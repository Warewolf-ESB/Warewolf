
using System.Windows;
using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Views.Help;

namespace Dev2.ViewModels.Help
{
    public class HelpViewWrapper : IHelpViewWrapper
    {
        public HelpViewWrapper(HelpView view)
        {
            HelpView = view;
        }

        public HelpView HelpView { get; private set; }

        public WebBrowser WebBrowser
        {
            get
            {
                return HelpView.WebBrowserHost;
            }
        }

        public CircularProgressBar CircularProgressBar
        {
            get
            {
                return HelpView.CircularProgressBar;
            }
        }

        public Visibility WebBrowserVisibility  
        {
            get
            {
                return WebBrowser.Visibility;
            }
            set
            {
                WebBrowser.Visibility = value;
            }
        }

         public Visibility CircularProgressBarVisibility  
        {
            get
            {
                return CircularProgressBar.Visibility;
            }
            set
            {
                CircularProgressBar.Visibility = value;
            }
        }

        public void Navigate(string uri)
        {
            HelpView.WebBrowserHost.Navigate(uri);
        }
    }
}
