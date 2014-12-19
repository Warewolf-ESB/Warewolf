using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
	public interface IExplorerViewModel
	{
		ICollection<IEnvironmentViewModel> Environments {get;set;}
        void Filter(string filter);
	}
}