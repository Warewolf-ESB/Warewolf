using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel:IMenuViewModel
    {
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