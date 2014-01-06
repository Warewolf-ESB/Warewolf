using System;

namespace Dev2.Network.Execution
{
    public interface INetworkExecutionChannel : INetworkChannel
    {
        bool AddExecutionStatusCallback(Guid callbackID, Action<ExecutionStatusCallbackMessage> callback);
        bool RemoveExecutionStatusCallback(Guid callbackID);
    }
}
