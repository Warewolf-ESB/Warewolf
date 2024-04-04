using Dev2.Common.Interfaces.Help;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.PubSubEvents;
#else
using Prism.Events;
#endif

namespace Warewolf.Studio.Models.Help
{
    public class HelpChangedEvent:PubSubEvent<IHelpDescriptor>
    {
    }
}
