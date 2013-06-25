using System.ComponentModel.Composition;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Messages;

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
