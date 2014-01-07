using System.Collections.Generic;
using Dev2.DynamicServices;

namespace Dev2.Runtime.Network
{
    public interface IContextManager<T>
        where T : IStudioNetworkSession
    {
        IList<T> CurrentContexts { get; }
    }
}