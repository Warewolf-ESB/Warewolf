using System;
using System.Activities.Presentation.Model;
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
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Designers2.DropBox2016.Upload
{
    public class DropBoxUploadViewModel : FileActivityDesignerViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<DropBoxSource> _sources;
        private readonly IEventAggregator _eventPublisher;
        private string _fromPath;
        private string _toPath;
        private string _result;
        private bool _overWriteMode;
        private bool _addMode;

        [ExcludeFromCodeCoverage]
        public DropBoxUploadViewModel(ModelItem modelItem)
            : this(modelItem, EventPublishers.Aggregator, ResourceCatalog.Instance)
        {
            this.RunViewSetup();
        }

        public DropBoxUploadViewModel(ModelItem modelItem, IEventAggregator eventPublisher, IResourceCatalog resourceCatalog)
            : base(modelItem,"File Or Folder", String.Empty)
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
        }
        public ICommand NewSourceCommand { get; set; }
        public IResourceCatalog Catalog { get; set; }
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
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);

        }

        public void CreateOAuthSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            _sources = LoadOAuthSources();
            OnPropertyChanged("Sources");
        }
        //Used by specs

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
        }


    }


}