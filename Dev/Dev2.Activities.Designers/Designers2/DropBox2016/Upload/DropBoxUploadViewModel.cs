using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Designers2.DropBox2016.Upload
{
    public class DropBoxUploadViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        private ObservableCollection<OauthSource> _sources;
        private IEnvironmentModel _environmentModel;
        private IEventAggregator _eventPublisher;
        private bool _isRefreshing;

        public DropBoxUploadViewModel(ModelItem modelItem)
            : this(modelItem,EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public DropBoxUploadViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
           _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), o => IsDropboxSourceSelected);
             Sources = LoadOAuthSources();
             SetSelectedOAuthSource(SelectedSource);
             AddTitleBarLargeToggle();
        }
        public readonly OauthSource NewOAuthSource = new OauthSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "New OAuth Source..."
        };
        public readonly OauthSource SelectOAuthSource = new OauthSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "Select a OAuth Source..."
        };
       
       
        public OauthSource SelectedSource
        {
            get { return GetProperty<OauthSource>(); }
            // ReSharper disable once ExplicitCallerInfoArgument
            set
            {
                if (value == NewOAuthSource && !_isRefreshing)
                {
                    CreateOAuthSource();
                    return;
                }
                SetProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
           
                OnPropertyChanged("IsDropboxSourceSelected");
                OnPropertyChanged("SelectedSource");
            }
        }
      
        public ObservableCollection<OauthSource> Sources
        {
            get
            {
                return _sources;
            }
            private set
            {
                _sources = value;
                OnPropertyChanged("Sources");
            }
        }
        public RelayCommand EditDropboxSourceCommand { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource != null && SelectedSource != SelectOAuthSource && SelectedSource != NewOAuthSource;
            }
        }
        private void EditDropBoxSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectOAuthSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);

        }
        void CreateOAuthSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            _isRefreshing = true;
            Sources = LoadOAuthSources();
            var newOAuthSource = Sources.FirstOrDefault(source => source.IsNewResource);
            SetSelectedOAuthSource(newOAuthSource);
            _isRefreshing = false;
        }
        void SetSelectedOAuthSource(OauthSource oAuthSource)
        {
            var selectOAuthSource = oAuthSource == null ? null : Sources.FirstOrDefault(d => d.ResourceID == oAuthSource.ResourceID);
            if (selectOAuthSource == null)
            {
                if (Sources.FirstOrDefault(d => d.Equals(SelectOAuthSource)) == null)
                {
                    Sources.Insert(0, SelectOAuthSource);
                }
                selectOAuthSource = SelectOAuthSource;
            }
            SelectedSource = selectOAuthSource;
        }
        ObservableCollection<OauthSource> LoadOAuthSources()
        {
            var oauthSources = _environmentModel.ResourceRepository.FindSourcesByType<OauthSource>(_environmentModel, enSourceType.OauthSource);
            oauthSources.Insert(0, NewOAuthSource);
            return oauthSources.ToObservableCollection();
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}