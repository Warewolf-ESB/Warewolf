using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Services.Communication.Mapi;

namespace Dev2.Studio.Core.Services.Communication
{
    public class MailToEmailCommService<T> : ICommService<T> 
        where T : EmailCommMessage
    {
        public void SendCommunication(T message)
        {
            var mailto = string.Format(
                "mailto:{0}?Subject={1}&Body={2}&Attach={3}",
                message.To,message.Subject,message.Content,message.AttachmentLocation);
            Process.Start(mailto);
        }
    }
}
