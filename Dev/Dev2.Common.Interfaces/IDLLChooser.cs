using System.Collections.Generic;
using System.Windows.Input;
// ReSharper disable InconsistentNaming

namespace Dev2.Common.Interfaces
{
    public interface IDLLChooser
    {
        ICommand CancelCommand { get; set; }
        ICommand SelectCommand { get; set; }
        IDllListingModel SelectedDll { get; set; }
        string AssemblyName { get; set; }
        List<IDllListingModel> AllDllListingModels { get; set; }
        List<IDllListingModel> DllListingModels { get; set; }
        bool IsLoading { get; set; }
        string SearchTerm { get; set; }
        string FilterTooltip { get; set; }
        string FilesTooltip { get; set; }
        string SelectTooltip { get; set; }
        ICommand ClearSearchTextCommand { get; set; }
        IDllListingModel GetGacDLL();
        IDllListingModel GetFileSystemDLL();
    }
}
