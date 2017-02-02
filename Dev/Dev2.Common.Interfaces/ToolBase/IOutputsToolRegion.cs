using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IOutputsToolRegion : IToolRegion
    {
        ICollection<IServiceOutputMapping> Outputs { get; set; }
        bool OutputMappingEnabled { get; set; }
        bool IsOutputsEmptyRows { get; set; }
        string RecordsetName { get; set; }
        bool IsObject { get; set; }
        string ObjectName { get; set; }
        string ObjectResult { get; set; }
        bool IsObjectOutputUsed { get; set; }
        IOutputDescription Description { get; set; }
        bool OutputCountExpandAllowed { get; set; }
    }
}