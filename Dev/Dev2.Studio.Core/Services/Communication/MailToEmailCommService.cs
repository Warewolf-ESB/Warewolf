using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.Communication
{
    public class MailToEmailCommService<T> : ICommService<T>
        where T : EmailCommMessage
    {
        public void SendCommunication(T message)
        {
            var mailto = string.Format(
                "mailto:{0}?Subject={1}&Body={2}&Attach={3}",
                message.To, message.Subject, message.Content, message.AttachmentLocation);
            Process.Start(mailto);
        }
    }
}
