using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Help;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Help
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpViewModel : BaseWorkSurfaceViewModel,
        IHandle<TabClosedMessage>
    {
        private HelpView _helpView;

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

        public string Uri { get; set; }

        private bool isViewAvailable = true;

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(!(view is HelpView)) return;

            _helpView = (HelpView)view;
            if(!isViewAvailable)
            {
                LoadBrowserUri(Uri);
            }

        }

        public void LoadBrowserUri(string uri)
        {
            Uri = uri;
            if(_helpView == null)
            {
                isViewAvailable = false;
            }
            else
            {
                isViewAvailable = true;
                _helpView.CircularProgressBar.Visibility = Visibility.Collapsed;

                if(_helpView.BDSBrowser != null)
                {
                    _helpView.BDSBrowser.Visibility = Visibility.Visible;
                    _helpView.BDSBrowser.LoadSafe(uri);
                    _helpView.BDSBrowser.InvalidateVisual();
                }
            }
        }

        public void Handle(TabClosedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(!message.Context.Equals(this)) return;

            EventPublisher.Unsubscribe(this);
            _helpView.BDSBrowser.Dispose();
        }
    }
}
