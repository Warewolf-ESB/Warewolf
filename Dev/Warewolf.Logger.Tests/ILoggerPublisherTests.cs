/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Logging;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class ILoggerPublisherTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ILoggerPublisher))]
        public void ILoggerPublisher_NewConnection_IsSuccessfull()
        {
            var mockConfig = new Mock<ILoggerConfig>();
            var mockloggerSource = new Mock<ILoggerSource>();
            var mockConnection = new Mock<ILoggerConnection>();

            mockConnection.Setup(o => o.NewPublisher()).Returns(new Mock<ILoggerPublisher>().Object);

            mockloggerSource.Setup(o => o.NewConnection(mockConfig.Object)).Returns(mockConnection.Object);

            var loggerSource = mockloggerSource.Object;

            var connection = loggerSource.NewConnection(mockConfig.Object);

            //------------------------------------Assert--------------------------------------
            mockloggerSource.Verify(o => o.NewConnection(It.IsAny<ILoggerConfig>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ILoggerPublisher))]
        public void ILoggerPublisher_NewPublisher_PublishLogger_IsSuccessfull()
        {
            var mockConfig = new Mock<ILoggerConfig>();
            var mockloggerSource = new Mock<ILoggerSource>();
            var mockConnection = new Mock<ILoggerConnection>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();

            mockConnection.Setup(o => o.NewPublisher()).Returns(mockLoggerPublisher.Object);

            mockloggerSource.Setup(o => o.NewConnection(mockConfig.Object)).Returns(mockConnection.Object);

            var loggerSource = mockloggerSource.Object;

            var connection = loggerSource.NewConnection(mockConfig.Object);
            var logger = connection.NewPublisher();

            logger.Info(outputTemplate: GlobalConstants.WarewolfLogsTemplate);
            logger.Warn(outputTemplate: GlobalConstants.WarewolfLogsTemplate);
            logger.Error(outputTemplate: GlobalConstants.WarewolfLogsTemplate);
            logger.Fatal(outputTemplate:GlobalConstants.WarewolfLogsTemplate);

            //------------------------------------Assert--------------------------------------
            mockloggerSource.Verify(o => o.NewConnection(It.IsAny<ILoggerConfig>()), Times.Once);
            mockConnection.Verify(o => o.NewPublisher(), Times.Once);
            mockLoggerPublisher.Verify(o => o.Info(It.IsAny<string>()), Times.Once);
        }
    }
}
