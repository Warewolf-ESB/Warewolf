﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Studio.Interfaces;







namespace Dev2.Activities.Designers2.DropBox2016.Delete
{
    public class DropBoxDeleteViewModel : FileActivityDesignerViewModel, INotifyPropertyChanged
    {
        ObservableCollection<DropBoxSource> _sources;
        readonly IDropboxSourceManager _sourceManager;
        string _deletePath;
        string _result;


        public DropBoxDeleteViewModel(ModelItem modelItem)
           : this(modelItem, new DropboxSourceManager())
        {
            this.RunViewSetup();
        }
        public DropBoxDeleteViewModel(ModelItem modelItem, IDropboxSourceManager sourceManager)
            : base(modelItem, "File Or Folder", String.Empty)
        {
            _sourceManager = sourceManager;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), p => IsDropboxSourceSelected);
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateOAuthSource);
            
            Sources = LoadOAuthSources();
            AddTitleBarLargeToggle();
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Dropbox_Delete;
        }

        public ICommand NewSourceCommand { get; set; }
        public IResourceCatalog ResourceManager => ResourceCatalog.Instance;

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

        public string DeletePath
        {
            get
            {
                _deletePath = GetProperty<string>();
                return _deletePath;
            }
            set
            {
                _deletePath = value;
                SetProperty(value);
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
                SetProperty(value);
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<DropBoxSource> LoadOAuthSources()
        {
            Sources = _sourceManager.FetchSources<DropBoxSource>().ToObservableCollection();
            return Sources;
        }
    }
}
