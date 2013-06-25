using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

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
