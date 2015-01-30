using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase, IMenuViewModel
    {
        bool _hasNewVersion;
        bool _isPanelLocked;
        int _buttonWidth;
        

        public MenuViewModel(IShellViewModel shellViewModel)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            shellViewModel.ActiveServerChanged += ShellViewModelOnActiveServerChanged;
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource, type => CanCreateNewService);
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(null), () => CanDeploy);
            SaveCommand = new DelegateCommand(shellViewModel.SaveService, () => CanSave);
            OpenSchedulerCommand = new DelegateCommand(shellViewModel.OpenScheduler, () => CanSetSchedules);
            OpenSettingsCommand = new DelegateCommand(shellViewModel.OpenSettings, () => CanSetSettings);
            ExecuteServiceCommand = new DelegateCommand(shellViewModel.ExecuteService, () => CanExecuteService);
            CheckForNewVersion(shellViewModel);
            CheckForNewVersionCommand = new DelegateCommand(shellViewModel.DisplayDialogForNewVersion);
            LockCommand = new DelegateCommand(()=>Lock(shellViewModel));

        }



        public void Lock(IShellViewModel shellViewModel)
        {
            IsPanelLocked = !IsPanelLocked;
            shellViewModel.MenuExpanded = IsPanelLocked;
        }

        public ICommand LockCommand { get; set; }


        public ICommand CheckForNewVersionCommand { get; set; }

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

        public bool IsPanelLocked
        {
            get
            {
                return _isPanelLocked;
            }
            set
            {
                _isPanelLocked = value;
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
                OnPropertyChanged(() => UnLockLabel);

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

        public ICommand DeployCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenSchedulerCommand { get; set; }
        public ICommand ExecuteServiceCommand { get; set; }


        public string NewLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogNewLabel;
                return String.Empty;
            }
        }
        public string SaveLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogSaveLabel;
                return String.Empty;
            }
        }
        public string DeployLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogDeployLabel;
                return String.Empty;
            }
        }
        public string DatabaseLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogDatabaseLabel;
                return String.Empty;
            }

        }
        public string DLLLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogDLLLabel;
                return String.Empty;
            }
        }
        public string WebLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogWebLabel;
                return String.Empty;
            }
        }
        public string TaskLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogTaskLabel;
                return String.Empty;
            }
        }
        public string DebugLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogDebugLabel;
                return String.Empty;
            }
        }
        public string SettingsLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogSettingsLabel;
                return String.Empty;
            }
        }
        public string SupportLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogSupportLabel;
                return String.Empty;
            }
        }
        public string ForumsLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogForumsLabel;
                return String.Empty;
            }
        }
        public string ToursLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogToursLabel;
                return String.Empty;
            }
        }
        public string NewVersionLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogNewVersionLabel;
                return String.Empty;
            }
        }
        public string LockLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogLockLabel;
                return String.Empty;
            }
        }

        public string UnLockLabel
        {
            get
            {
                if (!_isPanelLocked)
                    return Resources.Languages.Core.MenuDialogUnLockLabel;
                return String.Empty;
            }
        }
    }
}