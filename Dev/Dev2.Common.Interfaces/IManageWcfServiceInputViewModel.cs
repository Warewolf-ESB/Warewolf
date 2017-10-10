using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces
{
    public interface IManageWcfServiceInputViewModel : IToolRegion, IManageServiceInputViewModel<IWcfService>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        IGenerateOutputArea OutputArea { get; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; }
        bool PasteResponseVisible { get; set; }
        bool PasteResponseAvailable { get; }
        bool OutputCountExpandAllowed { get; set; }
        bool InputCountExpandAllowed { get; set; }
        bool IsGenerateInputsEmptyRows { get; set; }
    }
}
