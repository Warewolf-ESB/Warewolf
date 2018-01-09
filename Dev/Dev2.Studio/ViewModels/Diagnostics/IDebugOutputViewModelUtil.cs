using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.Diagnostics;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    interface IDebugOutputViewModelUtil
    {
        bool ContenIsNotValid(IDebugState content);
        bool QueuePending(IDebugState item, List<IDebugState> pendingItems, bool isProcessing);
    
        bool IsValidLineItem(IDebugLineItem item);
        bool IsItemMoreLinkValid(IDebugLineItem item);
    }
}
