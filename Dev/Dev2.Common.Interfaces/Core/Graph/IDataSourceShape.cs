using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IDataSourceShape
    {
        List<IPath> Paths { get; set; }
    }
}
