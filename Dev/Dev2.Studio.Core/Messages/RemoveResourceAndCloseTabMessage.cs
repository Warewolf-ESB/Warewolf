using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class RemoveResourceAndCloseTabMessage : IMessage
    {
        #region Properties

        public IContextualResourceModel ResourceToRemove { get; set; }

        #endregion

        #region Ctor

        public RemoveResourceAndCloseTabMessage(IContextualResourceModel resourceToRemove)
        {
            ResourceToRemove = resourceToRemove;
        }

        #endregion



    }
}
