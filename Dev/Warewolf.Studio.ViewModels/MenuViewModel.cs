using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase, IMenuViewModel, IMenuView
    {

        bool _hasNewVersion;
        bool _panelLockedOpen;
        bool _panelOpen;
        int _buttonWidth;
        readonly IShellViewModel _viewModel;
        bool _isOverLock;

        public MenuViewModel(IShellViewModel shellViewModel)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            _viewModel = shellViewModel;
            _isOverLock = false;
            _viewModel.ActiveServerChanged += ShellViewModelOnActiveServerChanged;
            NewCommand = new DelegateCommand<ResourceType?>(_viewModel.NewResource, type => CanCreateNewService);
            DeployCommand = new DelegateCommand(() => _viewModel.DeployService(null), () => CanDeploy);
            SaveCommand = new DelegateCommand(_viewModel.SaveService, () => CanSave);
            OpenSchedulerCommand = new DelegateCommand(_viewModel.OpenScheduler, () => CanSetSchedules);
            OpenSettingsCommand = new DelegateCommand(_viewModel.OpenSettings, () => CanSetSettings);
            ExecuteServiceCommand = new DelegateCommand(_viewModel.ExecuteService, () => CanExecuteService);
            CheckForNewVersion(_viewModel);
            CheckForNewVersionCommand = new DelegateCommand(_viewModel.DisplayDialogForNewVersion);

            LockCommand = new DelegateCommand(Lock);
            SlideOpenCommand = new DelegateCommand(() =>
            {
                if (!_isOverLock)
                {
                    SlideOpen(_viewModel);

                }
            });
            SlideClosedCommand = new DelegateCommand(() =>
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (_viewModel.MenuPanelWidth >= 80 && !_isOverLock)
                {
                    SlideClosed(_viewModel);
                }
            });
            IsOverLockCommand = new DelegateCommand(() => _isOverLock = true);
            IsNotOverLockCommand = new DelegateCommand(() => _isOverLock = false);
            ButtonWidth = 115;
            IsPanelLockedOpen = true;
            IsPanelOpen = true;



        }



        public ICommand DeployCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenSchedulerCommand { get; set; }
        public ICommand ExecuteServiceCommand { get; set; }

        public ICommand LockCommand { get; set; }
        public ICommand SlideOpenCommand { get; set; }
        public ICommand SlideClosedCommand { get; set; }
        public ICommand CheckForNewVersionCommand { get; set; }


        public string LockImage
        {
            get
            {
                if (IsPanelLockedOpen)
                    return "UnlockAlt";
                return "Lock";
            }
        }


        void UpdateProperties()
        {
            OnPropertyChanged(() => NewLabel);
            OnPropertyChanged(() => SaveLabel);
            OnPropertyChanged(() => DeployLabel);
            OnPropertyChanged(() => DatabaseLabel);
            OnPropertyChanged(() => DLLLabel);
            OnPropertyChanged(() => WebLabel);
            OnPropertyChanged(() => TaskLabel);
            OnPropertyChanged(() => DebugLabel);
            OnPropertyChanged(() => SettingsLabel);
            OnPropertyChanged(() => SupportLabel);
            OnPropertyChanged(() => ForumsLabel);
            OnPropertyChanged(() => ToursLabel);
            OnPropertyChanged(() => NewVersionLabel);
            OnPropertyChanged(() => LockLabel);

            OnPropertyChanged(() => ButtonWidth);
        }

        public ICommand IsOverLockCommand { get; private set; }
        public ICommand IsNotOverLockCommand { get; private set; }

        public void Lock()
        {

            if (!IsPanelLockedOpen) 
            {
                IsPanelLockedOpen = true;
            }
            else 
            {
                if(!IsPanelOpen && ButtonWidth == 115)
                    ButtonWidth = 35;
                if (IsPanelOpen && ButtonWidth == 35)
                    ButtonWidth = 115;

                IsPanelLockedOpen = false;
            }

            UpdateProperties();

        }


        public void SlideOpen(IShellViewModel shellViewModel)
        {
            if (IsPanelLockedOpen)
            {
                IsPanelOpen = true;
                shellViewModel.MenuExpanded = IsPanelOpen;
                ButtonWidth = 115;
                UpdateProperties();
            }
        }

        public void SlideClosed(IShellViewModel shellViewModel)
        {
            if (IsPanelLockedOpen && !IsPanelOpen)
            {
                shellViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 35;
                IsPanelOpen = !IsPanelOpen;

            }
            else if (IsPanelLockedOpen && IsPanelOpen)
            {
                shellViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 115;
                IsPanelOpen = !IsPanelOpen;
            }

            
            UpdateProperties();
        }


        async void CheckForNewVersion(IShellViewModel shellViewModel)
        {
            HasNewVersion = await shellViewModel.CheckForNewVersion();
        }



        public int ButtonWidth
        {
            get
            {
                return _buttonWidth;
            }
            set
            {
                _buttonWidth = value;
            }
        }


        public bool HasNewVersion
        {
            get
            {
                return _hasNewVersion;
            }
            set
            {
                _hasNewVersion = value;
                OnPropertyChanged(() => HasNewVersion);
            }
        }

        public bool IsPanelOpen
        {
            get
            {
                return _panelOpen;
            }
            set
            {
                _panelOpen = value;
            }

        }


        public bool IsPanelLockedOpen
        {
            get
            {
                return _panelLockedOpen;
            }
            set
            {
                _panelLockedOpen = value;
                OnPropertyChanged(() => LockLabel);
                OnPropertyChanged(() => LockImage);



            }
        }

        void ShellViewModelOnActiveServerChanged()
        {
            UpdateCommandExecutionBaseOnPermissions();
        }

        void UpdateCommandExecutionBaseOnPermissions()
        {
            CanCreateNewService = true;
            CanDeploy = true;
            CanSave = true;
            CanSetSchedules = true;
            CanSetSettings = true;
            CanExecuteService = true;
        }

        public bool CanExecuteService { get; set; }
        public bool CanSetSettings { get; set; }
        public bool CanSetSchedules { get; set; }
        public bool CanSave { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanCreateNewService { get; set; }


        public string NewLabel
        {
            get
            {
                if ( ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogNewLabel;
                return String.Empty;
            }
        }
        public string SaveLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSaveLabel;
                return String.Empty;
            }
        }
        public string DeployLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDeployLabel;
                return String.Empty;
            }
        }
        public string DatabaseLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDatabaseLabel;
                return String.Empty;
            }

        }
        public string DLLLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDLLLabel;
                return String.Empty;
            }
        }
        public string WebLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogWebLabel;
                return String.Empty;
            }
        }
        public string TaskLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogTaskLabel;
                return String.Empty;
            }
        }
        public string DebugLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDebugLabel;
                return String.Empty;
            }
        }
        public string SettingsLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSettingsLabel;
                return String.Empty;
            }
        }
        public string SupportLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSupportLabel;
                return String.Empty;
            }
        }
        public string ForumsLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogForumsLabel;
                return String.Empty;
            }
        }
        public string ToursLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogToursLabel;
                return String.Empty;
            }
        }
        public string NewVersionLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogNewVersionLabel;
                return String.Empty;
            }
        }
        public string LockLabel
        {
            get
            {
                if (IsPanelLockedOpen)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogLockLabel;
                return Resources.Languages.Core.MenuDialogUnLockLabel;

            }
        }


        public object DataContext { get; set; }

        public string NewServiceToolTip
        {
            get { return Resources.Languages.Core.MenuNewServiceToolTip; }
        }
        public string SaveToolTip
        {
            get { return Resources.Languages.Core.MenuSaveToolTip; }
        }
        public string DeployToolTip
        {
            get { return Resources.Languages.Core.MenuDeployToolTip; }
        }
        public string DatabaseToolTip
        {
            get { return Resources.Languages.Core.MenuDatabaseToolTip; }
        }
        public string PluginToolTip
        {
            get { return Resources.Languages.Core.MenuPluginToolTip; }
        }
        public string WebServiceToolTip
        {
            get { return Resources.Languages.Core.MenuWebServiceToolTip; }
        }
        public string SchedulerToolTip
        {
            get { return Resources.Languages.Core.MenuSchedulerToolTip; }
        }
        public string DebugToolTip
        {
            get { return Resources.Languages.Core.DebugToolTip; }
        }
        public string SettingsToolTip
        {
            get { return Resources.Languages.Core.MenuSettingsToolTip; }
        }
        public string HelpToolTip
        {
            get { return Resources.Languages.Core.MenuHelpToolTip; }
        }

        public string DownloadToolTip
        {
            get { return Resources.Languages.Core.MenuDownloadToolTip; }
        }

        public string LockToolTip
        {
            get { return Resources.Languages.Core.MenuLockToolTip; }
        }
    }
}