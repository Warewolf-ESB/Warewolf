using System;

namespace Dev2.Common.Interfaces
{
    public interface IConnectionStatusChangedEventArg
    {
        ConnectionEnumerations.ConnectedState ConnectedStatus { get; }
        Guid EnvironmentId { get; }
        bool DoCallback { get; }
    }
}