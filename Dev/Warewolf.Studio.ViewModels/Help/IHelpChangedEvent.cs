using Dev2.Common.Interfaces.Help;
using Prism.Events;

namespace Warewolf.Studio.Models.Help
{
    public class HelpChangedEvent:PubSubEvent<IHelpDescriptor>
    {
    }
}
