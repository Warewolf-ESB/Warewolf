using System;
using System.Net.Mail;

namespace Tu.Servers
{
    public interface IEmailServer : IDisposable
    {
        void Send(MailMessage message);
    }
}
