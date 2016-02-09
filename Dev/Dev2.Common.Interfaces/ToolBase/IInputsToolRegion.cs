using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IInputsToolRegion : IToolRegion
    {
        
        ICollection<IServiceInput> Inputs { get; set; }
    }
}