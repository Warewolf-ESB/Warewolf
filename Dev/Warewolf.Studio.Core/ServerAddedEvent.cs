using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.Core
{
    public class ServerAddedEvent:PubSubEvent<IServer>
    {
    }
}
