using System;
using System.Activities.Presentation.Model;
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

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.Upload
{
    public class DropBoxUploadViewModel : FileActivityDesignerViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<DropBoxSource> _sources;
        private string _fromPath;
        private string _toPath;
        private string _result;
        private bool _overWriteMode;
        private bool _addMode;
        private readonly IDropboxSourceManager _sourceManager;

        public DropBoxUploadViewModel(ModelItem modelItem)
            : this(modelItem, new DropboxSourceManager())
        {
            this.RunViewSetup();
        }

        public DropBoxUploadViewModel(ModelItem modelItem, IDropboxSourceManager sourceManager)
            : base(modelItem,"File Or Folder", String.Empty)
        {
            _sourceManager = sourceManager;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), p => IsDropboxSourceSelected);
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateOAuthSource);
            // ReSharper disable once VirtualMemberCallInContructor
            Sources = LoadOAuthSources();
            AddTitleBarLargeToggle();
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Dropbox_Upload;
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
                _fromPath = GetProperty<string>();
                return _fromPath;

            }
            set
            {
                _fromPath = value;
                SetProperty(_fromPath);
                OnPropertyChanged();
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
        public bool OverWriteMode
        {
            get
            {
                _overWriteMode = GetProperty<bool>();
                return _overWriteMode;
            }
            set
            {
                _overWriteMode = value;
                SetProperty(_overWriteMode);
                OnPropertyChanged();
            }
        }
        public bool AddMode
        {
            get
            {
                _addMode = GetProperty<bool>();
                return _addMode;
            }
            set
            {
                _addMode = value;
                SetProperty(_addMode);
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
            if(shellViewModel == null)
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}