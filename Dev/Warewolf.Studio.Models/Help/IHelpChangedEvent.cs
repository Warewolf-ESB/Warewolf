using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.Models.Help
{
    public class HelpChangedEvent:PubSubEvent<IHelpDescriptor>
    {
    }

    public class ToolDropped : PubSubEvent<IToolDescriptor>
    {
        
    }
}
