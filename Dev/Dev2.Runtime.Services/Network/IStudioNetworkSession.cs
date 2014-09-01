using System;
using System.Network;
using Dev2.Common.Interfaces.Hosting;

namespace Dev2.DynamicServices
{
    public interface IStudioNetworkSession : INetworkOperator, IHostContext
    {
        void Kill();
        Version Version { get; }
        PlatformID Platform { get; }
        string ServicePack { get; }
        uint Fingerprint { get; }
        bool Attached { get; }
    }
}