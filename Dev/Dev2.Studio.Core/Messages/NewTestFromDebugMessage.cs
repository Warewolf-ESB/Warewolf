using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class NewTestFromDebugMessage : IMessage
    {
        public Guid ResourceID { get; set; }
        public List<IDebugTreeViewItemViewModel> RootItems { get; set; }
        public IContextualResourceModel ResourceModel { get; set; }
    }
}
