using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class RemoveResourceAndCloseTabMessage : IMessage
    {
        #region Properties

        public IContextualResourceModel ResourceToRemove { get; set; }
        public bool RemoveFromWorkspace { get; set; }

        #endregion

        #region Ctor

        public RemoveResourceAndCloseTabMessage(IContextualResourceModel resourceToRemove, bool removeFromWorkspace)
        {
            ResourceToRemove = resourceToRemove;
            RemoveFromWorkspace = removeFromWorkspace;
        }

        #endregion
    }
}
