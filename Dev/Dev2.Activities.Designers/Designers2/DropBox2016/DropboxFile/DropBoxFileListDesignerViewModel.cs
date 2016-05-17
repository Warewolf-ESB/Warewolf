using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Hosting;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.DropboxFile
{
    public class DropBoxFileListDesignerViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        private ObservableCollection<DropBoxSource> _sources;
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
            : this(modelItem, EventPublishers.Aggregator, ResourceCatalog.Instance)
        {
            this.RunViewSetup();
        }

        public DropBoxFileListDesignerViewModel(ModelItem modelItem, IEventAggregator eventPublisher, IResourceCatalog resourceCatalog)
            : base(modelItem)
        {
            _eventPublisher = eventPublisher;
            ThumbVisibility = Visibility.Visible;
            Catalog = resourceCatalog;
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
        public IResourceCatalog Catalog { get; set; }
        public ICommand NewSourceCommand { get; set; }
        public DropBoxSource SelectedSource
        {
            get
            {
                var oauthSource = GetProperty<DropBoxSource>();
                return oauthSource ?? GetProperty<DropBoxSource>();
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            set
            {
                SetProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDropboxSourceSelected");
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("SelectedSource");
            }
        }
       
        public virtual ObservableCollection<DropBoxSource> Sources
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
                _toPath = GetProperty<string>();
                return _toPath;
            }
            set
            {
                _toPath = value;
                SetProperty(_toPath);
                OnPropertyChanged();
            }
        }
        public virtual List<string> Files
        {
            get
            {
                _files = GetProperty<List<string>>();
                return _files;
            }
        }

        public bool IncludeMediaInfo
        {
            get
            {
                _includeMediaInfo = GetProperty<bool>();
                return _includeMediaInfo;
            }
            set
            {
                _includeMediaInfo = value;
                SetProperty(_includeMediaInfo);
                OnPropertyChanged();
            }
        }

        public bool IsRecursive
        {
            get
            {
                _isRecursive = GetProperty<bool>();
                return _isRecursive;
            }
            set
            {
                _isRecursive = value;
                SetProperty(_isRecursive);
                OnPropertyChanged();
            }
        }

        public bool IncludeDeleted
        {
            get
            {
                _includeDeleted = GetProperty<bool>();
                return _includeDeleted;
            }
            set
            {
                _includeDeleted = value;
                SetProperty(_includeDeleted);
                OnPropertyChanged();
            }
        }


        public string Result
        {
            get
            {
                _result = GetProperty<string>();
                return _result;
            }
            set
            {
                _result = value;
                SetProperty(_result);
                OnPropertyChanged();
            }
        }

        public bool IsFilesSelected
        {
            get
            {
                _isFilesSelected = GetProperty<bool>();
                return _isFilesSelected;
            }
            set
            {
                _isFilesSelected = value;
                SetProperty(_isFilesSelected);
                OnPropertyChanged();
            }
        }
        public bool IsFoldersSelected
        {
            get
            {
                _isFoldersSelected = GetProperty<bool>();
                return _isFoldersSelected;
            }
            set
            {
                _isFoldersSelected = value;
                SetProperty(_isFoldersSelected);
                OnPropertyChanged();
            }
        }

        public bool IsFilesAndFoldersSelected
        {
            get
            {
                _isFilesAndFoldersSelected = GetProperty<bool>();
                return _isFilesAndFoldersSelected;
            }
            set
            {
                _isFilesAndFoldersSelected = value;
                SetProperty(_isFilesAndFoldersSelected);
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

        public virtual ObservableCollection<DropBoxSource> LoadOAuthSources()
        {
            Dispatcher.Invoke(() =>
            {
                _sources = Catalog.GetResourceList<DropBoxSource>(GlobalConstants.ServerWorkspaceID)
                    .Cast<DropBoxSource>()
                    .ToObservableCollection();
            });
            return _sources;
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