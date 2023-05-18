/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class ExchangeEmailSenderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExchangeEmailSender))]
        public void ExchangeEmailSender_InValid_Send_AutoDiscoverUrl_IsNullOrEmpty_ExpectServiceValidationException()
        {
            //---------------------------Arrange---------------------------
            var mockExchange = new Mock<IExchange>();

            var exchangeEmailSender = new ExchangeEmailSender(mockExchange.Object);
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<ServiceValidationException>(()=> exchangeEmailSender.Send(new ExchangeServiceFactory().Create(), new EmailMessage(new ExchangeServiceFactory().Create())));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExchangeEmailSender))]
        public void ExchangeEmailSender_InValid_Send_AutoDiscoverUrl_IsNotNullOrEmpty_ExpectUriFormatException()
        {
            //---------------------------Arrange---------------------------
            var mockExchange = new Mock<IExchange>();

            mockExchange.Setup(o => o.AutoDiscoverUrl).Returns("testAutoDiscoverUrl");

            var exchangeEmailSender = new ExchangeEmailSender(mockExchange.Object);
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<UriFormatException>(()=> exchangeEmailSender.Send(new ExchangeServiceFactory().Create(), new EmailMessage(new ExchangeServiceFactory().Create())));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [Ignore]
        [TestCategory(nameof(ExchangeEmailSender))]
        public void ExchangeEmailSender_InValid_Send_AutoDiscoverUrl_IsNotNullOrEmpty_ExpectServiceLocalException()
        {
            //---------------------------Arrange---------------------------
            var mockExchange = new Mock<IExchange>();

            mockExchange.Setup(o => o.AutoDiscoverUrl).Returns("https://testAutoDiscoverUrl");

            var exchangeEmailSender = new ExchangeEmailSender(mockExchange.Object);
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<ServiceLocalException>(() => exchangeEmailSender.Send(new ExchangeServiceFactory().Create(), new EmailMessage(new ExchangeServiceFactory().Create())));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExchangeEmailSender))]
        public void ExchangeEmailSender_InValid_Send_AutoDiscoverUrl_IsNotNullOrEmpty_ExpectFormatException()
        {
            //---------------------------Arrange---------------------------
            var mockExchange = new Mock<IExchange>();

            mockExchange.Setup(o => o.UserName).Returns("TestUsername");
            var exchangeEmailSender = new ExchangeEmailSender(mockExchange.Object);
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<FormatException>(()=> exchangeEmailSender.Send(new ExchangeServiceFactory().Create(), new EmailMessage(new ExchangeServiceFactory().Create())));
        }
    }
}
