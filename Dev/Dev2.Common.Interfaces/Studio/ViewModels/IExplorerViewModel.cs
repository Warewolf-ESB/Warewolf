using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
	public interface IExplorerViewModel
	{
		ICollection<IEnvironmentViewModel> Environments {get;set;}
	}

    public interface IEnvironmentViewModel
    {
        ICollection<IExplorerItemViewModel> ExplorerItemViewModels { get; set; }
        string DisplayName { get; set; }
        bool IsConnected { get; }
        IServer Server { get; set; }
    }

    public interface IExplorerItemViewModel
    {
        string ResourceName { get; set; }
        ICollection<IExplorerItemViewModel> Children { get; set; } 
    }
}