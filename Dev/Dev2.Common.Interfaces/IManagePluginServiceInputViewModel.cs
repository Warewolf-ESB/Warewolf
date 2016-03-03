using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces
{
    public interface IManagePluginServiceInputViewModel : IToolRegion, IManageServiceInputViewModel<IPluginService>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        IGenerateOutputArea OutputArea { get; set; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; set; }
        bool PasteResponseVisible { get; set; }
        bool PasteResponseAvailable { get; }

        void SetInitialVisibility();
    }

    public interface IManageWebServiceInputViewModel : IToolRegion, IManageServiceInputViewModel<IWebService>
    {
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        ICommand PasteResponseCommand { get; }
        IGenerateOutputArea OutputArea { get; set; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; set; }
        bool PasteResponseVisible { get; set; }
        bool PasteResponseAvailable { get; }

        void SetInitialVisibility();
    }
}