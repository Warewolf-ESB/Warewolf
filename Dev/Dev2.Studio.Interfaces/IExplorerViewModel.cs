using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dev2.Studio.Interfaces
{

    public delegate void SelectedExplorerEnvironmentChanged(object sender, IEnvironmentViewModel e);
    public delegate void SelectedServerChanged(object sender, Guid environmentId);
    public delegate void SelectedExplorerItemChanged(object sender, IExplorerTreeItem e);
	public interface IExplorerViewModel:INotifyPropertyChanged,IDisposable
	{
        ObservableCollection<IEnvironmentViewModel> Environments {get;set;}
        void Filter(string filter);
        event SelectedExplorerItemChanged SelectedItemChanged;
        IEnvironmentViewModel SelectedEnvironment { get; set; }
        IServer SelectedServer { get;  }
	    IConnectControlViewModel ConnectControlViewModel { get; }
        string SearchText { get; set; }
        bool IsFromActivityDrop { get; set; }
	    ICommand RefreshCommand { get; set; }
        bool IsRefreshing { get; set; }
	    bool ShowConnectControl { get; set; }
	    IExplorerTreeItem SelectedItem { get; set; }
	    object[] SelectedDataItems { get; set; }
	    ICommand ClearSearchTextCommand { get; }
	    ICommand CreateFolderCommand { get; }
        bool AllowDrag { get; set; }
	    string SearchToolTip { get; }
	    string ExplorerClearSearchTooltip { get; }
	    string RefreshToolTip { get; }

	    void SelectItem(Guid id);

        Task RefreshEnvironment(Guid environmentId);

	    Task RefreshSelectedEnvironment();
	}
}