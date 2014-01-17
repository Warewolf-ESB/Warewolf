using Dev2.Studio.Core.Interfaces;
// ReSharper disable once CheckNamespace


namespace Dev2.Studio.Core.Messages
{
    public class ResourceCheckedMessage : IDeployMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public bool? PreCheckedState { get; set; }
        public bool? PostCheckedState { get; set; }
    }
}
