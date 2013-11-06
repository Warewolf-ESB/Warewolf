using System.Windows.Forms;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Workflow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Dialogs
{
    public class ResourcePickerDialog : IResourcePickerDialog
    {
        public IResourceModel SelectedResource { get; set; }

        public DialogResult ShowDialog()
        {
            var type = typeof(DsfWorkflowActivity);
            DsfActivityDropViewModel dropViewModel;
            SelectedResource = DsfActivityDropUtils.TryPickResource(type.FullName, out dropViewModel) ? dropViewModel.SelectedResourceModel : null;

            return SelectedResource == null ? DialogResult.Cancel : DialogResult.OK;
        }
    }
}