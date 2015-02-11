using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.ViewModels.Help;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyHelpViewModel : HelpWindowViewModel
    {
        public DummyHelpViewModel(IHelpDescriptorViewModel currentHelpText, IEventAggregator aggregator):base(currentHelpText,new HelpModel(aggregator))
        {
        }
    }
}
