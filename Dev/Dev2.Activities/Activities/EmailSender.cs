
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
