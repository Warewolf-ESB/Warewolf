using Dev2.Studio.Interfaces;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.PubSubEvents;
#else
using Prism.Events;
#endif

namespace Warewolf.Studio.Core
{
    public class ServerAddedEvent:PubSubEvent<IServer>
    {
    }
}
