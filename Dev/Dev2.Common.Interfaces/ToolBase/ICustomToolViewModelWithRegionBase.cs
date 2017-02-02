using System.Collections.Generic;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface ICustomToolViewModelWithRegionBase
    {
        IList<IToolRegion> Regions { get; }
    }
}
