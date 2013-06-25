using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IMapper
    {
        IEnumerable<IPath> Map(object data);
    }
}
