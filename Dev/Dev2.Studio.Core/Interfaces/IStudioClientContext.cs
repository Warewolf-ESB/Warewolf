using System;
using Dev2.Diagnostics;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IStudioClientContext : IStudioEsbChannel
    {
        Guid AccountID { get; }
        Guid ServerID { get; }

        TCPDispatchedClient AcquireAuxiliaryConnection();
        void AddDebugWriter(IDebugWriter writer);
        void RemoveDebugWriter(IDebugWriter writer);
        void RemoveDebugWriter(Guid writerID);
    }
}
