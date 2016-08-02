﻿using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IManageComPluginSourceViewModel
    {
        ObservableCollection<IDllListingModel> DllListings { get; set; }
        IDllListingModel SelectedDll { get; set; }
        string ResourceName { get; set; }
        ICommand OkCommand { get; set; }
        bool IsLoading { get; set; }
        string SearchTerm { get; set; }
        ICommand ClearSearchTextCommand { get; }
        ICommand RefreshCommand { get; set; }
        string AssemblyName { get; set; }
    }
}