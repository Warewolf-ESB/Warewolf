using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.Upload
{
    public class DropBoxUploadViewModel : FileActivityDesignerViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<OauthSource> _sources;
        private readonly IEnvironmentModel _environmentModel;
        private readonly IEventAggregator _eventPublisher;
        private string _fromPath;
        private string _toPath;
        private string _result;
        private bool _overWriteMode;
        private bool _addMode;

        [ExcludeFromCodeCoverage]
        public DropBoxUploadViewModel(ModelItem modelItem)
            : this(modelItem, EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public DropBoxUploadViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem,"File Or Folder", String.Empty)
        {
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), p => IsDropboxSourceSelected);
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateOAuthSource);
            // ReSharper disable once VirtualMemberCallInContructor
            _sources = LoadOAuthSources();
            AddTitleBarLargeToggle();
            IsDropboxSourceWizardSourceMessagePulished = false;
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
        }
        public ICommand NewSourceCommand { get; set; }
        public OauthSource SelectedSource
        {
            get
            {
                var oauthSource = GetModelPropertyName() as OauthSource;
                return oauthSource ?? GetProperty<OauthSource>();
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            set
            {
                SetModelItemProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDropboxSourceSelected");
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("SelectedSource");
            }
        }

        private void SetModelItemProperty(object value, [CallerMemberName]string propName = null)
        {
            ModelItem.SetProperty(propName, value);
        }

        private object GetModelPropertyName([CallerMemberName]string propName = null)
        {
            var propertyValue = ModelItem.GetProperty(propName);
            return propertyValue ?? string.Empty;
        }
        public virtual ObservableCollection<OauthSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                SetProperty(value);
                _sources = value;
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("Sources");
            }
        }
        
        public RelayCommand EditDropboxSourceCommand { get; private set; }
        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource != null;
            }
        }
        public string FromPath
        {
            get
            {
                _fromPath = GetModelPropertyName().ToString();
                return _fromPath;

            }
            set
            {
                _fromPath = value;
                SetModelItemProperty(_fromPath);
                OnPropertyChanged();
            }
        }
        public string ToPath
        {
            get
            {
                _toPath = GetModelPropertyName().ToString();
                return _toPath;
            }
            set
            {
                _toPath = value;
                SetModelItemProperty(_toPath);
                OnPropertyChanged();
            }
        }
        public string Result
        {
            get
            {
                _result = GetModelPropertyName().ToString();
                return _result;
            }
            set
            {
                _result = value;
                SetModelItemProperty(_result);
                OnPropertyChanged();
            }
        }
        public bool OverWriteMode
        {
            get
            {
                _overWriteMode = Convert.ToBoolean(GetModelPropertyName());
                return _overWriteMode;
            }
            set
            {
                _overWriteMode = value;
                SetModelItemProperty(_overWriteMode);
                OnPropertyChanged();
            }
        }
        public bool AddMode
        {
            get
            {
                _addMode = Convert.ToBoolean(GetModelPropertyName());
                return _addMode;
            }
            set
            {
                _addMode = value;
                SetModelItemProperty(_addMode);
                OnPropertyChanged();
            }
        }
      

        private void EditDropBoxSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);

        }

        public void CreateOAuthSource()
        {
            IsDropboxSourceWizardSourceMessagePulished = false;
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            _sources = LoadOAuthSources();
            IsDropboxSourceWizardSourceMessagePulished = true;
            OnPropertyChanged("Sources");
        }
        //Used by specs
        public bool IsDropboxSourceWizardSourceMessagePulished { get; set; }

        public virtual ObservableCollection<OauthSource> LoadOAuthSources()
        {
            var oauthSources = _environmentModel.ResourceRepository.FindSourcesByType<OauthSource>(_environmentModel, enSourceType.OauthSource);
            return oauthSources.ToObservableCollection();
        }

        #region Overrides of ActivityDesignerViewModel
        [ExcludeFromCodeCoverage]
        public override void Validate()
        {
        }
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }


}