
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Sources
{
    /// <summary>
    /// Summary description for EmailSourceTest
    /// </summary>
    [TestClass]
    public class EmailSourceTest
    {
        const int SmtpTimeout = 30000;

        [TestMethod]
        [Ignore]
        public void EmailSourcesTest_YAHOO()
        {
            var source = CreateYahooSource().ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        [Ignore]
        public void EmailSourcesTest_GMAIL_SSL()
        {
            var source = CreateGmailSource().ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        [Ignore]
        public void EmailSourcesTest_HOTMAIL_SSL()
        {
            var source = CreateWindowsLiveSource().ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        [Ignore]
        public void EmailSourcesTest_LOCAL()
        {
            var source = CreateLocalSource().ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

        #region CreateSources

        static EmailSource CreateYahooSource()
        {
            return new EmailSource
            {
                Host = "smtp.mail.yahoo.com",
                Port = 25,
                EnableSsl = false,
                Timeout = SmtpTimeout,
                UserName = "dev2developer@yahoo.com",
                Password = "Q38qrDmsi36ei1R",
                TestFromAddress = "dev2developer@yahoo.com",
                TestToAddress = "dev2warewolf@gmail.com",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestYahoo",
                ResourcePath = "Testing",
            };
        }


        static EmailSource CreateLocalSource()
        {
            return new EmailSource
            {
                Host = "smtp.afrihost.co.za",
                Port = 25,
                EnableSsl = false,
                Timeout = SmtpTimeout,
                UserName = "dev2test",
                Password = "Password",
                TestFromAddress = "ThorLocal@norsegods.com",
                TestToAddress = "dev2warewolf@gmail.com",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestLocal",
                ResourcePath = "Testing",
            };
        }

        static EmailSource CreateGmailSource()
        {
            return new EmailSource
            {
                Host = "smtp.gmail.com",
                Port = 25,
                EnableSsl = true,
                Timeout = SmtpTimeout,
                UserName = "dev2warewolf@gmail.com",
                Password = "Q38qrDmsi36ei1R",
                TestFromAddress = "ThorGmail@norsegods.com",
                TestToAddress = "dev2warewolf@gmail.com",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestGmail",
                ResourcePath = "Testing",
            };
        }

        static EmailSource CreateWindowsLiveSource()
        {
            return new EmailSource
            {
                Host = "smtp.live.com",
                Port = 25,
                EnableSsl = true,
                Timeout = SmtpTimeout,
                UserName = "dev2warewolf@hotmail.com",
                Password = "Q38qrDmsi36ei1R",
                TestFromAddress = "dev2warewolf@hotmail.com",
                TestToAddress = "dev2warewolf@gmail.com",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestWindowsLive",
                ResourcePath = "Testing",
            };
        }

        #endregion
    }
}
