using System.Collections.Generic;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface ICustomToolViewModelWithRegionBase
    {
        IList<IToolRegion> Regions { get; }

        double DesignHeight { get; set; }
        double DesignMinHeight { get; set; }
        double DesignMaxHeight { get; set; }
    }
}
