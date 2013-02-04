using System.ComponentModel.Composition;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Messages;

namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : IHandle<TabClosedMessage>
    {
        public HelpWindow(string uri = null)
        {
            InitializeComponent();
            ImportService.SatisfyImports(this);
            BDSBrowser.Initialize(uri);
            EventAggregator.Subscribe(this);
        }

        #region Properties

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        #endregion Properties

        #region Implementation of IHandle<TabClosedMessage>

        public void Handle(TabClosedMessage message)
        {
            if (message.Context.Equals(this))
            {
                EventAggregator.Unsubscribe(this);
                BDSBrowser.Dispose();
            }
        }

        #endregion
    }
}
