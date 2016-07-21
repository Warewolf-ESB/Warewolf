using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Studio.ViewModels.Diagnostics
{

    class DebugOutputViewModelUtil : IDebugOutputViewModelUtil
    {
        private readonly Guid _sessionId;

        public DebugOutputViewModelUtil(Guid sessionId)
        {
            _sessionId = sessionId;
        }

        public bool QueuePending(IDebugState item, List<IDebugState> pendingItems, bool isProcessing)
        {
            if (item.StateType == StateType.Message && isProcessing)
            {
                pendingItems.Add(item);
                return true;
            }
            return false;
        }

        public bool ContenIsNotValid(IDebugState content)
        {
            if (content == null || content.SessionID != _sessionId)
            {
                return true;
            }
            if (content.Name == "EsbServiceInvoker" && content.ExecutionOrigin == ExecutionOrigin.Unknown)
            {
                return true;
            }

            if (content.StateType == StateType.Start && !content.IsFirstStep())
            {
                return true;
            }
            return false;
        }
    }
}
