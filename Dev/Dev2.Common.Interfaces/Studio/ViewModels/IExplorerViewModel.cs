using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
	public interface IExplorerViewModel
	{
		ICollection<IExplorerItemViewModel> ExplorerItems {get;set;}
	}

    public interface IExplorerItemViewModel
    {
        string ResourceName { get; set; }
        string IconPath { get; set; }
    }
}