using System.Network;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class NetworkStateChangedEventArgs:INetworkStateChangedEventArgs
    {
        NetworkStateChangedState _state;

        public NetworkStateChangedEventArgs(NetworkStateEventArgs state)
        {
            switch(state.ToState)
            {
                case NetworkState.Connecting: _state = NetworkStateChangedState.Connecting; break;
                case NetworkState.Offline: _state = NetworkStateChangedState.Disconnected; break;
                case NetworkState.Online: _state = NetworkStateChangedState.Connected; break;
            }
            
        }

        #region Implementation of INetworkStateChangedEventArgs

        public NetworkStateChangedState State
        {
            get
            {
                return _state;
            }
        }

        #endregion
    }
}
