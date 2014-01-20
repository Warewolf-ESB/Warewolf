using Caliburn.Micro;
using CefSharp.Wpf;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Help;
using System.ComponentModel.Composition;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Help
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpViewModel : BaseWorkSurfaceViewModel,
        IHandle<TabClosedMessage>
    {
        private WebView _browser;
        private string _uri;

        public HelpViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public HelpViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
        }

        public override WorkSurfaceContext WorkSurfaceContext
        {
            get { return WorkSurfaceContext.Help; }
        }

        public string Uri
        {
            get
            {
                return _uri;
            }
            set
            {
                if(_uri == value) return;

                _uri = value;
                NotifyOfPropertyChange(() => Uri);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(!(view is HelpView)) return;

            var helpView = (HelpView)view;
            _browser = helpView.BDSBrowser;

            // PBI 9512 - 2013.06.07 - TWR: refactored
            _browser.LoadSafe(Uri);
        }

        public void Handle(TabClosedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(!message.Context.Equals(this)) return;

            EventPublisher.Unsubscribe(this);
            _browser.Dispose();
        }
    }
}
