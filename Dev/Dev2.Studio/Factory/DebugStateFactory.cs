using System;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Factory
{
    public static class DebugStateFactory
    {
        public static IDebugState Create(string message, IContextualResourceModel resourceModel)
        {
            var state = new DebugState
                {
                    Message = message,
                };

            if(resourceModel != null)
            {
                state.ServerID = resourceModel.ServerID;
                state.OriginatingResourceID = resourceModel.ID;
            }
            state.StateType = String.IsNullOrWhiteSpace(message) ? StateType.Clear : StateType.Message;

            return state;
        }

        public static IDebugState Create(Guid serverID, Guid resourceID, StateType stateType, string message)
        {
            return new DebugState
                {
                    ServerID = serverID,
                    OriginatingResourceID = resourceID,
                    StateType = stateType,
                    Message = message
                };
        }
    }
}
