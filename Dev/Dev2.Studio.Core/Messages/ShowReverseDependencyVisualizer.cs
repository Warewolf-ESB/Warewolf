using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class ShowReverseDependencyVisualizer : IMessage
    {
        public IContextualResourceModel Model { get; set; }

        public ShowReverseDependencyVisualizer(IContextualResourceModel model)
        {
            Model = model;
        }
    }
}
