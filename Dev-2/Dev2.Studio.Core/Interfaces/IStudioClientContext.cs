using System;
using Dev2.Diagnostics;
using Dev2.Studio.Core.Network;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IStudioClientContext : IStudioEsbChannel
    {
        Guid WorkspaceID { get; }
        Guid ServerID { get; }
    }
}
