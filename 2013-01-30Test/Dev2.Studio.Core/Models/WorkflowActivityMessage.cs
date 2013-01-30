using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unlimited.Framework;

namespace Dev2.Studio.Core {
    public class ActivityMessage : IApplicationMessage {

        public ActivityMessage() {
            
            
        }
        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message) {
            if (MessageReceived != null) {
                MessageReceived(message);
            }
        }
    }
}
