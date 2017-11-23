using System.Network;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Core
{
    public class NetworkStateChangedEventArgs : INetworkStateChangedEventArgs
    {
        readonly ConnectionNetworkState _state;

        public NetworkStateChangedEventArgs(NetworkStateEventArgs state)
        {
            switch (state.ToState)
            {
                case NetworkState.Connecting:
                    _state = ConnectionNetworkState.Connecting;
                    break;
                case NetworkState.Online:
                    _state = ConnectionNetworkState.Connected;
                    break;
                default:
                    _state = ConnectionNetworkState.Disconnected;
                    break;
            }

        }

        #region Implementation of INetworkStateChangedEventArgs

        public ConnectionNetworkState State => _state;

        #endregion
    }

}
