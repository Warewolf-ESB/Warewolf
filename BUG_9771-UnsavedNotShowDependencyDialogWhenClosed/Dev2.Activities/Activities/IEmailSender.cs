using System.Net.Mail;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Activities
{
    public interface IEmailSender
    {
        void Send(EmailSource emailSource, MailMessage mailMessage);
        void Send();
        EmailSource EmailSource { get; set; }
        MailMessage MailMessage { get; set; }
    }
}