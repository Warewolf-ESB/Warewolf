
// ReSharper disable once CheckNamespace

using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Studio.Core
{
    public class ActivityMessage : IApplicationMessage
    {
        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message)
        {
            if(MessageReceived != null)
            {
                MessageReceived(message);
            }
        }
    }
}
