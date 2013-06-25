using System.Data;
using System.Net.Mail;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Activities
{
    public class EmailSender : IEmailSender
    {
        #region Implementation of IEmailSender

        public void Send(EmailSource emailSource, MailMessage mailMessage)
        {
            EmailSource = emailSource;
            MailMessage = mailMessage;
            emailSource.Send(mailMessage);
        }

        public void Send()
        {
            if (EmailSource == null) throw new NoNullAllowedException("Please set SmtpClient");
            if (MailMessage == null) throw new NoNullAllowedException("Please set MailMessage");
            EmailSource.Send(MailMessage);
        }

        public EmailSource EmailSource { get; set; }
        public MailMessage MailMessage { get; set; }

        #endregion
    }
}