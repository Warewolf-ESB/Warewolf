using System;
using System.Network;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;

namespace Dev2.Studio.Core.Interfaces {
    public interface IEnvironmentConnection {
        Uri Address { get; set; }
        bool IsConnected { get; }
        bool IsAuxiliry { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }
        IFrameworkDataChannel DataChannel { get; set; }
        INetworkExecutionChannel ExecutionChannel { get; set; }
        INetworkDataListChannel DataListChannel { get; set; }
        IFrameworkSecurityContext SecurityContext { get; set; }
        void Connect();
        void Disconnect();
        event EventHandler<LoginStateEventArgs> LoginStateChanged;
    }
}
