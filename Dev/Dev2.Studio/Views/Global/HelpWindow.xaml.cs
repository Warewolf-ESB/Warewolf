using Dev2.Studio.Core.AppResources.Browsers;

namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow
    {
        public HelpWindow(string uri = null)
        {
            InitializeComponent();
            BDSBrowser.Initialize(uri);
        }
    }
}
