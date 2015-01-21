using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{

    public delegate void SelectedExplorerEnvironmentChanged(object sender, IEnvironmentViewModel e);

	public interface IExplorerViewModel
	{
		ICollection<IEnvironmentViewModel> Environments {get;set;}
        void Filter(string filter);
        event SelectedExplorerEnvironmentChanged SelectedEnvironmentChanged;
        IEnvironmentViewModel SelectedEnvironment { get; set; }
        IServer SelectedServer { get;  }
	    IConnectControlViewModel ConnectControlViewModel { get; }

	    IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc);
	}
}