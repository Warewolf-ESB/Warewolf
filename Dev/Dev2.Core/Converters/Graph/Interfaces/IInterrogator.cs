using System;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IInterrogator
    {
        IMapper CreateMapper(object data);
        INavigator CreateNavigator(object data, Type pathType);
    }
}
