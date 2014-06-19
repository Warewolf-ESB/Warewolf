using Dev2.Studio.Core.Interfaces;

namespace Dev2.Dialogs
{
    public interface IResourcePickerDialog
    {
        IResourceModel SelectedResource { get; set; }
        bool ShowDialog(IEnvironmentModel environmentModel = null);
    }
}