using System.Network;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class NetworkStateChangedEventArgs:INetworkStateChangedEventArgs
    {
        ConnectionNetworkState _state;

        public NetworkStateChangedEventArgs(NetworkStateEventArgs state)
        {
            switch(state.ToState)
            {
                case NetworkState.Connecting: _state = ConnectionNetworkState.Connecting; break;
                case NetworkState.Offline: _state = ConnectionNetworkState.Disconnected; break;
                case NetworkState.Online: _state = ConnectionNetworkState.Connected; break;
            }
            
        }

        #region Implementation of INetworkStateChangedEventArgs

        public ConnectionNetworkState State
        {
            get
            {
                return _state;
            }
        }

        #endregion
    }
}
