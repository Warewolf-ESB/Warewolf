using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IGenerateOutputArea : IToolRegion
    {
        ICollection<IServiceOutputMapping> Outputs { get; set; }
    }
}
