using Dev2.Studio.Core.AppResources.Browsers;
using System.Windows.Controls;

namespace Dev2.Studio.Views
{
    /// <summary>
    /// Interaction logic for ActivitySettingsView.xaml
    /// </summary>
    public partial class ActivitySettingsView : UserControl
    {
        public ActivitySettingsView()
        {
            InitializeComponent();
            browser.Initialize();
        }

        public void Dispose()
        {

        }
    }
}
