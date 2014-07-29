using System;

namespace Dev2.ConnectionHelpers
{
    public class ConnectedServerChangedEvent
    {
        public Guid EnvironmentId { get; private set; }

        public ConnectedServerChangedEvent(Guid environmentId)
        {
            EnvironmentId = environmentId;
        }
    }
}
