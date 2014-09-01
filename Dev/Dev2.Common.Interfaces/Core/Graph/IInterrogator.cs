using System;

namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IInterrogator
    {
        IMapper CreateMapper(object data);
        INavigator CreateNavigator(object data, Type pathType);
    }
}
