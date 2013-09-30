using Dev2.Common;
using Dev2.Providers.Logs;

namespace Dev2.Studio.Core.AppResources.Administration {
    public class BDSAdminChannel : IFrameworkDuplexCallbackChannel, IApplicationMessage {
        #region IFrameworkDuplexCallbackChannel Members

        public void CallbackNotification(string message) {
            SendMessage(message);
           Logger.TraceInfo(message);
        }

        #endregion

        #region IApplicationMessage Members

        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message) {
            if (MessageReceived != null) {
                MessageReceived(message);
            }
        }

        #endregion


    }
}
