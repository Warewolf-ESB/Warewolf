﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{

    public delegate void SelectedExplorerEnvironmentChanged(object sender, IEnvironmentViewModel e);
    public delegate void SelectedServerChanged(object sender, Guid environmentId);
    public delegate void SelectedExplorerItemChanged(object sender, IExplorerTreeItem e);
	public interface IExplorerViewModel:INotifyPropertyChanged,IDisposable
	{
		ICollection<IEnvironmentViewModel> Environments {get;set;}
        void Filter(string filter);
        void RemoveItem(IExplorerItemViewModel item);
        event SelectedExplorerEnvironmentChanged SelectedEnvironmentChanged;
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
	    IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc);

	    void SelectItem(string path);

	    void RefreshEnvironment(Guid environmentId);
	}
}