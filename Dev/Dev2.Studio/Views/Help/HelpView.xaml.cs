using Dev2.Studio.Core.AppResources.Browsers;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Help
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpView
    {
        public HelpView()
        {
            InitializeComponent();
            BDSBrowser.Initialize();
        }
    }
}
