using System;

namespace Dev2.ConnectionHelpers
{
    public class ConnectionStatusChangedEventArg : EventArgs
    {
        public ConnectionEnumerations.ConnectedState ConnectedStatus { get; private set; }
        public Guid EnvironmentId { get; private set; }
        public bool DoCallback { get; private set; }

        public ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState connectedStatus, Guid environmentId, bool doCallback)
        {
            ConnectedStatus = connectedStatus;
            EnvironmentId = environmentId;
            DoCallback = doCallback;
        }
    }
}
    