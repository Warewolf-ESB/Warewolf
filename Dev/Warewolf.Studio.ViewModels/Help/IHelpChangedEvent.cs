using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.Models.Help
{
    public class HelpChangedEvent:PubSubEvent<IHelpDescriptor>
    {
    }
}
