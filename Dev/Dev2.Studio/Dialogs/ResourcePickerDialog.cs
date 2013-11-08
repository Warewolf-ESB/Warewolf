using System.Windows.Forms;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Utils;

namespace Dev2.Dialogs
{
    public class ResourcePickerDialog : IResourcePickerDialog
    {
        readonly IEnvironmentRepository _environmentRepository;

        public ResourcePickerDialog(IEnvironmentModel source)
        {
            VerifyArgument.IsNotNull("source", source);
            _environmentRepository = EnvironmentRepository.Create(source);
        }

        public IResourceModel SelectedResource { get; set; }

        public DialogResult ShowDialog()
        {
            SelectedResource = null;
            IResourceModel selectedResource;

            if(DsfActivityDropUtils.TryPickAnyResource(_environmentRepository, out selectedResource))
            {
                SelectedResource = selectedResource;
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }
    }
}