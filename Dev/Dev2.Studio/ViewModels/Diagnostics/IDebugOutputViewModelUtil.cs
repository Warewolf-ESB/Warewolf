using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    interface IDebugOutputViewModelUtil
    {
        bool ContenIsNotValid(IDebugState content);
        bool QueuePending(IDebugState item, List<IDebugState> pendingItems, bool isProcessing);
    }
}
