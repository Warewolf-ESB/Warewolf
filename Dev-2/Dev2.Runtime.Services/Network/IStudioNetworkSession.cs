using System;
using System.Network;

namespace Dev2.DynamicServices
{
    public interface IStudioNetworkSession : INetworkOperator
    {       
        void Kill();
        Guid AccountID { get; }
        Guid SessionID { get; }
        Version Version { get; }
        PlatformID Platform { get; }
        string ServicePack { get; }
        uint Fingerprint { get; }
        bool Attached { get; }
    }
}