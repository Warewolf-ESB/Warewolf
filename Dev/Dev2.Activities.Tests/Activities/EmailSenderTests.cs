/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Mail;

namespace Dev2.Tests.Activities.Activities
{
    [TestClass]
    public class EmailSenderTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailSender))]
        public void EmailSender_Validate_Send()
        {
            var mailMessage = new MailMessage();

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(email => email.Send(mailMessage));

            var emailSender = new EmailSender();
            emailSender.Send(mockEmailSource.Object, mailMessage);

            mockEmailSource.Verify(sender => sender.Send(mailMessage), Times.Once());
        }
    }
}
