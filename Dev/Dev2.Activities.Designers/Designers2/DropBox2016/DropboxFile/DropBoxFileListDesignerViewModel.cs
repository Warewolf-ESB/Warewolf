using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
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
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.DropboxFile
{
    public class DropBoxFileListDesignerViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        private ObservableCollection<OauthSource> _sources;
        private readonly IEnvironmentModel _environmentModel;
        private readonly IEventAggregator _eventPublisher;
        private string _toPath;
        private string _result;
        private List<string> _files;
        private bool _includeMediaInfo;
        private bool _isRecursive;
        private bool _includeDeleted;
        private bool _isFilesSelected;
        private bool _isFoldersSelected;
        private bool _isFilesAndFoldersSelected;

        public DropBoxFileListDesignerViewModel(ModelItem modelItem)
            : this(modelItem, EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public DropBoxFileListDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
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
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
            IsFilesSelected = true;
            IncludeDeleted = false;
            IsRecursive = false;
            IncludeMediaInfo = false;

        }
        public ICommand NewSourceCommand { get; set; }
        public OauthSource SelectedSource
        {
            get
            {
                var oauthSource = GetModelPropertyName<OauthSource>();
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

        private T GetModelPropertyName<T>([CallerMemberName]string propName = null)
        {
            var propertyValue = ModelItem.GetProperty<T>(propName);
            return propertyValue;
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

        public string ToPath
        {
            get
            {
                _toPath = GetModelPropertyName<string>();
                return _toPath;
            }
            set
            {
                _toPath = value;
                SetModelItemProperty(_toPath);
                OnPropertyChanged();
            }
        }
        public virtual List<string> Files
        {
            get
            {
                _files = GetModelPropertyName<List<string>>();
                return _files;
            }
        }

        public bool IncludeMediaInfo
        {
            get
            {
                _includeMediaInfo = GetModelPropertyName<bool>();
                return _includeMediaInfo;
            }
            set
            {
                _includeMediaInfo = value;
                SetModelItemProperty(_includeMediaInfo);
                OnPropertyChanged();
            }
        }

        public bool IsRecursive
        {
            get
            {
                _isRecursive = GetModelPropertyName<bool>();
                return _isRecursive;
            }
            set
            {
                _isRecursive = value;
                SetModelItemProperty(_isRecursive);
                OnPropertyChanged();
            }
        }

        public bool IncludeDeleted
        {
            get
            {
                _includeDeleted = GetModelPropertyName<bool>();
                return _includeDeleted;
            }
            set
            {
                _includeDeleted = value;
                SetModelItemProperty(_includeDeleted);
                OnPropertyChanged();
            }
        }


        public string Result
        {
            get
            {
                _result = GetModelPropertyName<string>();
                return _result;
            }
            set
            {
                _result = value;
                SetModelItemProperty(_result);
                OnPropertyChanged();
            }
        }

        public bool IsFilesSelected
        {
            get
            {
                _isFilesSelected = GetModelPropertyName<bool>();
                return _isFilesSelected;
            }
            set
            {
                _isFilesSelected = value;
                SetModelItemProperty(_isFilesSelected);
                OnPropertyChanged();
            }
        }
        public bool IsFoldersSelected
        {
            get
            {
                _isFoldersSelected = GetModelPropertyName<bool>();
                return _isFoldersSelected;
            }
            set
            {
                _isFoldersSelected = value;
                SetModelItemProperty(_isFoldersSelected);
                OnPropertyChanged();
            }
        }

        public bool IsFilesAndFoldersSelected
        {
            get
            {
                _isFilesAndFoldersSelected = GetModelPropertyName<bool>();
                return _isFilesAndFoldersSelected;
            }
            set
            {
                _isFilesAndFoldersSelected = value;
                SetModelItemProperty(_isFilesAndFoldersSelected);
                OnPropertyChanged();
            }
        }


        private void EditDropBoxSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);

        }

        public void CreateOAuthSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            _sources = LoadOAuthSources();
            OnPropertyChanged("Sources");
        }

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
            if (propertyName != null && propertyName.ToUpper() == "SelectedSource".ToUpper())
            {
                ToPath = String.Empty;
            }
        }


    }


}