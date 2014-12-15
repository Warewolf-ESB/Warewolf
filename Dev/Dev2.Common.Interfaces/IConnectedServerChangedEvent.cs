using System;

namespace Dev2.Common.Interfaces
{
    public interface IConnectedServerChangedEvent
    {
        Guid EnvironmentId { get; }
    }
}