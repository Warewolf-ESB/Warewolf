using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces
{
    public interface IManagePluginServiceInputViewModel : IManageServiceInputViewModel<IPluginService>
    {
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        List<IServiceOutputMapping> OutputMappings { get; set; }
        IOutputDescription Description { get; set; }
    }

    public interface IManageWebServiceInputViewModel : IManageServiceInputViewModel<IWebService>
    {
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        List<IServiceOutputMapping> OutputMappings { get; set; }
        IOutputDescription Description { get; set; }
    }
}