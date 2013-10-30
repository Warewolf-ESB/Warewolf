using System.Runtime.CompilerServices;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.ViewModels.WorkSurface;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel
    {
        bool _showLogging;
        bool _showSecurity = true;
        bool _selectionChanging;

        public SettingsViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
        }

        public bool ShowLogging
        {
            get { return _showLogging; }
            set
            {
                if(value.Equals(_showLogging))
                {
                    return;
                }
                _showLogging = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowLogging);
            }
        }

        public bool ShowSecurity
        {
            get { return _showSecurity; }
            set
            {
                if(value.Equals(_showSecurity))
                {
                    return;
                }
                _showSecurity = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowSecurity);
            }
        }

        void OnSelectionChanged([CallerMemberName] string propertyName = null)
        {
            if(_selectionChanging)
            {
                return;
            }

            _selectionChanging = true;
            switch(propertyName)
            {
                case "ShowLogging":
                    ShowSecurity = !ShowLogging;
                    break;

                case "ShowSecurity":
                    // TODO: Remove this when logging is enabled!
                    if(!ShowSecurity)
                    {
                        ShowSecurity = true;
                    }
                    // TODO: Add this when logging is enabled!
                    //ShowLogging = !ShowSecurity;
                    break;
            }
            _selectionChanging = false;
        }
    }
}
