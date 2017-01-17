using System.Windows.Input;
// ReSharper disable InconsistentNaming

namespace Dev2.Common.Interfaces
{
    public interface IManagePluginSourceViewModel
    {
        IDllListingModel SelectedDll { get; set; }
        string ResourceName { get; set; }
        ICommand OkCommand { get; set; }
        string FileSystemAssemblyName { get; set; }
        string ConfigFilePath { get; set; }
        string GACAssemblyName { get; set; }
        ICommand ChooseGACDLLCommand { get; set; }
        ICommand ChooseFileSystemDLLCommand { get; set; }
        ICommand ChooseConfigFileCommand { get; set; }
        bool CanSelectConfigFiles { get; set; }
    }
}
