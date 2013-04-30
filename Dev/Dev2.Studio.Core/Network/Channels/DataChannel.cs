using System;
using Dev2.Diagnostics;
using Dev2.Network.Messaging;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Core.Network.Channels
{
    public class DataChannel : IStudioClientContext
    {
        readonly IEnvironmentConnection _connection;

        public DataChannel(IEnvironmentConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            _connection = connection;
        }

        #region Implementation of IStudioEsbChannel

        public string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID)
        {
            Connect();

            return _connection.ExecuteCommand(xmlRequest, workspaceID, dataListID);
        }

        public INetworkMessage SendMessage<T>(T message) where T : INetworkMessage, new()
        {
            return null;
        }

        #endregion

        #region Implementation of IStudioClientContext

        public Guid WorkspaceID { get { return _connection.WorkspaceID; } }

        public Guid ServerID { get { return _connection.ServerID; } }

        #endregion

        void Connect()
        {
            _connection.Connect();
        }

    }
}
