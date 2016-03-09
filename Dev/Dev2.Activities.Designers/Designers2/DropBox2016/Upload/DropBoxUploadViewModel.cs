using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Designers2.DropBox2016.Upload
{
    public class DropBoxUploadViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<OauthSource> _sources;
        private readonly IEnvironmentModel _environmentModel;
        private readonly IEventAggregator _eventPublisher;
        private bool _isRefreshing;
        private string _selectedSourceName;

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
        public static readonly DependencyProperty OverWriteProperty = DependencyProperty.Register("OverWrite", typeof(bool), typeof(DropBoxUploadViewModel), new PropertyMetadata(true));
        public static readonly DependencyProperty UpdateProperty = DependencyProperty.Register("Update", typeof(bool), typeof(DropBoxUploadViewModel), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty AddProperty = DependencyProperty.Register("Add", typeof(bool), typeof(DropBoxUploadViewModel), new PropertyMetadata(default(bool)));

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
                // ReSharper disable once RedundantArgumentDefaultValue
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
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("Sources");
            }
        }
        public RelayCommand EditDropboxSourceCommand { get; private set; }
        //public event PropertyChangedEventHandler PropertyChanged;
        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource != null && SelectedSource != SelectOAuthSource && SelectedSource != NewOAuthSource;
            }
        }
        public string FromPath { get; set; }
        public string ToPath { get; set; }
        public string Result { get; set; }
        public string SelectedSourceName
        {
            get
            {
                if(SelectedSource == null)
                {
                    return string.Empty;
                }
                _selectedSourceName = string.Format(SelectedSource.ResourceName);
                return _selectedSourceName;
            }
        }
        public bool OverWrite
        {
            get
            {
                return (bool)GetValue(OverWriteProperty);
            }
            set
            {
                SetValue(OverWriteProperty, value);
            }
        }
        public bool Update
        {
            get
            {
                return (bool)GetValue(UpdateProperty);
            }
            set
            {
                SetValue(UpdateProperty, value);
            }
        }
        public bool Add
        {
            get
            {
                return (bool)GetValue(AddProperty);
            }
            set
            {
                SetValue(AddProperty, value);
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

        public void SetSelectedOAuthSource(OauthSource oAuthSource)
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
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected   void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

       


    }
}