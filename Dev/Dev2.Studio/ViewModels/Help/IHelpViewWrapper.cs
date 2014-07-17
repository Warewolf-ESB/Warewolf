using System.Windows;
using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Views.Help;

namespace Dev2.ViewModels.Help
{
    public interface IHelpViewWrapper
    {
        HelpView HelpView { get; }
        WebBrowser WebBrowser { get; }
        CircularProgressBar CircularProgressBar { get; }
        Visibility WebBrowserVisibility { get; set; }
        Visibility CircularProgressBarVisibility { get; set; }
        void Navigate(string uri);
    }
}