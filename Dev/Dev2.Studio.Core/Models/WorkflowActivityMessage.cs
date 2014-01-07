
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
