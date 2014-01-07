using System.Collections.Generic;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IMapper
    {
        IEnumerable<IPath> Map(object data);
    }
}
