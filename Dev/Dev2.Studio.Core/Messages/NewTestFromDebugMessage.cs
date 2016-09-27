using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class NewTestFromDebugMessage : IMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public List<IDebugTreeViewItemViewModel> RootItems { get; set; }
        public List<IDebugState> DebugStates { get; set; }
    }
}
