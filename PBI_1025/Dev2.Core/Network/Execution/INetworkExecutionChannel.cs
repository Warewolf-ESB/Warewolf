using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network;

namespace Dev2.Network.Execution
{
    public interface INetworkExecutionChannel : INetworkChannel
    {
        bool AddExecutionStatusCallback(Guid callbackID, Action<ExecutionStatusCallbackMessage> callback);
        bool RemoveExecutionStatusCallback(Guid callbackID);
    }
}
