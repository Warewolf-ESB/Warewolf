using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IGenerateInputArea : IToolRegion
    {
        ICollection<IServiceInput> Inputs { get; set; }

        double InputsHeight { get; set; }
        double MaxInputsHeight { get; set; }
    }
}
