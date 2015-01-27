using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{

    public delegate void SelectedExplorerEnvironmentChanged(object sender, IEnvironmentViewModel e);

	public interface IExplorerViewModel
	{
		ICollection<IEnvironmentViewModel> Environments {get;set;}
        void Filter(string filter);
        void RemoveItem(IExplorerItemViewModel item);
        event SelectedExplorerEnvironmentChanged SelectedEnvironmentChanged;
        IEnvironmentViewModel SelectedEnvironment { get; set; }
        IServer SelectedServer { get;  }
	    IConnectControlViewModel ConnectControlViewModel { get; }
        string SearchText { get; set; }
	    ICommand RefreshCommand { get; set; }
        bool IsRefreshing { get; set; }

	    IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc);
	}
}