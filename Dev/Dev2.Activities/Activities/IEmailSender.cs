
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
