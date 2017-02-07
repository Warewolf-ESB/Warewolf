using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
// ReSharper disable UnusedMember.Global

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.DropboxFile
{
    public class DropBoxFileListDesignerViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        private ObservableCollection<DropBoxSource> _sources;
        private string _toPath;
        private string _result;
        private List<string> _files;
        private bool _includeMediaInfo;
        private bool _isRecursive;
        private bool _includeDeleted;
        private bool _isFilesSelected;
        private bool _isFoldersSelected;
        private bool _isFilesAndFoldersSelected;
        private readonly IDropboxSourceManager _sourceManager;
        public DropBoxFileListDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new DropboxSourceManager())
        {
            this.RunViewSetup();
        }

        public DropBoxFileListDesignerViewModel(ModelItem modelItem, IDropboxSourceManager sourceManager)
            : base(modelItem)
        {
            _sourceManager = sourceManager;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), p => IsDropboxSourceSelected);
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateOAuthSource);
            // ReSharper disable once VirtualMemberCallInContructor
            Sources = LoadOAuthSources();
            AddTitleBarLargeToggle();
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
            IsFilesSelected = true;
            IncludeDeleted = false;
            IsRecursive = false;
            IncludeMediaInfo = false;
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Dropbox_List_Contents;
        }

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
        public bool IsDropboxSourceSelected => SelectedSource != null;

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
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var activeServer = shellViewModel.ActiveServer;
            if (activeServer != null)
                shellViewModel.OpenResource(SelectedSource.ResourceID,activeServer.EnvironmentID, activeServer);
        }

        public void CreateOAuthSource()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            if (shellViewModel == null)
            {
                return;
            }
            shellViewModel.NewDropboxSource(string.Empty);
            Sources = LoadOAuthSources();
            OnPropertyChanged(@"Sources");
        }

        public ObservableCollection<DropBoxSource> LoadOAuthSources()
        {
            Sources = _sourceManager.FetchSources<DropBoxSource>().ToObservableCollection();
            return Sources;
        }

        #region Overrides of ActivityDesignerViewModel
        public override void Validate()
        {
        }
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName != null && propertyName.ToUpper() == "SelectedSource".ToUpper())
            {
                ToPath = String.Empty;
            }
        }


    }


}