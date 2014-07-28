
namespace Dev2.ConnectionHelpers
{
    public class ConnectEventHandlers
    {
        public delegate void ConnectedStatusHandler(object sender, ConnectionStatusChangedEventArg e);
        public delegate void ConnectedServerChangedHandler(object sender, ConnectedServerChangedEvent e);
    }
}
