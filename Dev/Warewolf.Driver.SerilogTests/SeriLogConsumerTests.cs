/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using System.Text;
using Warewolf.Data;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;

namespace Warewolf.Tests
{
    [TestClass]
    public class SeriLogConsumerTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_Success()
        {
            //------------------------------Arrange-----------------------------
            //var expectedTestMessage = "test message";

            //var mockLogger = new Mock<ILogger>();
            //var mockLoggerPublisher = new Mock<ILoggerPublisher>();

            //mockLogger.Setup(o => o.Debug(It.IsAny<string>())).Verifiable();

            ////mockSeriLogConfig.Setup(o => o.Logger).Returns(mockLogger.Object);

            //var seriLogConsumer = new SeriLogConsumer(mockLoggerPublisher.Object);

            ////------------------------------Act---------------------------------
            //var response = seriLogConsumer.Consume(Encoding.Default.GetBytes(expectedTestMessage));
            ////------------------------------Assert------------------------------

            //mockLogger.Verify(o => o.Debug(It.IsAny<string>()), Times.Once);
            //Assert.AreEqual(expected: ConsumerResult.Success, actual: response.Result);
        }
    }
}
