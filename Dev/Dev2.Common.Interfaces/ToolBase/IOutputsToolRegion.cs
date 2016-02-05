using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IOutputsToolRegion : IToolRegion
    {

        ICollection<IServiceOutputMapping> Outputs { get; set; }
        bool OutputMappingEnabled { get; set; }
        bool IsOutputsEmptyRows { get; }
        string RecordsetName { get; set; }
        IOutputDescription Description { get; set; }
    }


    public interface IWebServiceGetViewModel
    {
        IOutputsToolRegion Outputs { get; set; }
        IWebGetInputArea InputArea { get; set; }
        ISourceToolRegion<IWebServiceSource> Source { get; set; }
    }
}