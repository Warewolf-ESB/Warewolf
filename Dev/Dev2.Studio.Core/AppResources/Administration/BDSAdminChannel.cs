using Dev2.Providers.Logs;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Administration
{
    public class BdsAdminChannel : IFrameworkDuplexCallbackChannel, IApplicationMessage
    {
        #region IFrameworkDuplexCallbackChannel Members

        public void CallbackNotification(string message)
        {
            SendMessage(message);
            this.TraceInfo(message);
        }

        #endregion

        #region IApplicationMessage Members

        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message)
        {
            if(MessageReceived != null)
            {
                MessageReceived(message);
            }
        }

        #endregion
    }
}
