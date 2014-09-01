using Dev2.Common.Interfaces.Core;

namespace Dev2.Common.Interfaces.Activity
{
    public interface IApplicationMessage
    {
        void SendMessage(string message);
        event MessageEventHandler MessageReceived;
    }
}
