
using System;

namespace Dev2.Studio.Core.Network
{
    public enum ServerState
    {
        Offline,
        Online
    }

    public class ServerStateEventArgs : EventArgs
    {
        public ServerStateEventArgs(ServerState state)
        {
            State = state;
        }

        public ServerState State { get; private set; }
    }
}
