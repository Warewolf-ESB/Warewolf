using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio
{
    public interface IMenuViewModel
    {
        ICommand NewCommand { get; set; }
        ICommand DeployCommand { get; set; }
        ICommand SaveCommand { get; set; }
        ICommand OpenSettingsCommand { get; set; }
        ICommand OpenSchedulerCommand { get; set; }
        ICommand ExecuteServiceCommand { get; set; }
    }
}
