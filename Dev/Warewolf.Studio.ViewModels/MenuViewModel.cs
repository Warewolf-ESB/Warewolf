using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase,IMenuViewModel
    {
        bool _hasNewVersion;

        public MenuViewModel(IShellViewModel shellViewModel)
        {
            if(shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            shellViewModel.ActiveServerChanged+=ShellViewModelOnActiveServerChanged;
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource,type => CanCreateNewService);
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(null),()=>CanDeploy);
            SaveCommand = new DelegateCommand(shellViewModel.SaveService,()=>CanSave);
            OpenSchedulerCommand = new DelegateCommand(shellViewModel.OpenScheduler, () => CanSetSchedules);
            OpenSettingsCommand = new DelegateCommand(shellViewModel.OpenSettings, () => CanSetSettings);
            ExecuteServiceCommand = new DelegateCommand(shellViewModel.ExecuteService, () => CanExecuteService);
            CheckForNewVersion(shellViewModel);
            CheckForNewVersionCommand = new DelegateCommand(shellViewModel.DisplayDialogForNewVersion);
        }

        public ICommand CheckForNewVersionCommand { get; set; }

        async void CheckForNewVersion(IShellViewModel shellViewModel)
        {
             HasNewVersion = await shellViewModel.CheckForNewVersion();             
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
    }
}