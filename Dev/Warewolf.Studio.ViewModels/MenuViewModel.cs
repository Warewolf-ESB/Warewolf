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

            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource);
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(null));
            SaveCommand = new DelegateCommand(shellViewModel.SaveService);
            OpenSchedulerCommand = new DelegateCommand(shellViewModel.OpenScheduler);
            OpenSettingsCommand = new DelegateCommand(shellViewModel.OpenSettings);
            ExecuteServiceCommand = new DelegateCommand(shellViewModel.ExecuteService);
        }

        public ICommand DeployCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenSchedulerCommand { get; set; }
        public ICommand ExecuteServiceCommand { get; set; }
    }
}