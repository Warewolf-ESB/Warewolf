#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;






namespace Dev2.Activities.Designers2.DropBox2016.DropboxFile
{
    public class DropBoxFileListDesignerViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        ObservableCollection<DropBoxSource> _sources;
        string _toPath;
        string _result;
        List<string> _files;
        bool _includeMediaInfo;
        bool _isRecursive;
        bool _includeDeleted;
        bool _isFilesSelected;
        bool _isFoldersSelected;
        bool _isFilesAndFoldersSelected;
        readonly IDropboxSourceManager _sourceManager;
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
            
            set
            {
                SetProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDropboxSourceSelected");
                
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


        void EditDropBoxSource()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var activeServer = shellViewModel.ActiveServer;
            if (activeServer != null)
            {
                shellViewModel.OpenResource(SelectedSource.ResourceID, activeServer.EnvironmentID, activeServer);
            }
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
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged() => OnPropertyChanged(null);
        protected void OnPropertyChanged(string propertyName)
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