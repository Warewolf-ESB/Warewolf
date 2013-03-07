using System.Xml.Linq;
using Dev2.Network.Messages;

namespace Dev2.Network.Messaging.Messages
{
    public interface ISettingsMessage : INetworkMessage
    {
        byte[] Assembly { get; set; }
        byte[] AssemblyHash { get; set; }
        XElement Settings { get; set; }
        NetworkMessageAction Action { get; set; }
        NetworkMessageResult Result { get; set; }
    }
}