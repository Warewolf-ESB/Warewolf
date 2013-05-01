using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Factory
{
    public static class DebugStateFactory
    {
        public static IDebugState Create(string message, IContextualResourceModel resourceModel)
        {
            var state = new DebugState
                {
                    Message = message,
                };

            if (resourceModel != null)
            {
                state.ServerID = resourceModel.ServerID;
                state.ResourceID = resourceModel.ID;
            }
            state.StateType = String.IsNullOrWhiteSpace(message) ? StateType.Clear : StateType.Message;

            return state;
        }

        public static IDebugState Create(Guid serverID, Guid resourceID, StateType stateType, string message)
        {
            return new DebugState
                {
                    ServerID = serverID,
                    ResourceID = resourceID,
                    StateType = stateType,
                    Message = message
                };
        }
    }
}
