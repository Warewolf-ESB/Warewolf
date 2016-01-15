using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.Core
{
    public class ServerAddedEvent:PubSubEvent<IServer>
    {
    }
    public class ItemAddedEvent : PubSubEvent<IExplorerItem>
    {
    }
}
