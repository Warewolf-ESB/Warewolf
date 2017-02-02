namespace Dev2.Common.Interfaces
{
    public interface INetworkStateChangedEventArgs
    {
        ConnectionNetworkState State{get;}
    }

    public enum ConnectionNetworkState
    {
        Connecting,
        Disconnected,
        Connected
    }
}