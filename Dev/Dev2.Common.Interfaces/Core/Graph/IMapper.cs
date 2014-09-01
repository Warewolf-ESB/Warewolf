using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IMapper
    {
        IEnumerable<IPath> Map(object data);
    }
}
