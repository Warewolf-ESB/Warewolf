using Dev2.Common;
using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Administration
{
    public class BdsAdminChannel : IFrameworkDuplexCallbackChannel, IApplicationMessage
    {
        #region IFrameworkDuplexCallbackChannel Members

        public void CallbackNotification(string message)
        {
            SendMessage(message);
            Dev2Logger.Log.Info(message);
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
