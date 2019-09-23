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
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Program.Implementation))]
        public void Program_Implementation_Run_And_Pause_Success()
        {
            //--------------------------------Arrange-------------------------------
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            var mockConsoleWindowFactory = new Mock<IConsoleWindowFactory>();
            var mockLogServerFactory = new Mock<ILogServerFactory>();
            var mockWriter = new Mock<IWriter>();
            var mockLogServer = new Mock<ILogServer>();

            mockLogServerFactory.Setup(o => o.New(mockWebSocketServerFactory.Object, mockWriter.Object, mockLoggerContext.Object)).Returns(mockLogServer.Object);

            var programImpl = new Program.Implementation(mockLoggerContext.Object, mockWebSocketServerFactory.Object, mockConsoleWindowFactory.Object, mockLogServerFactory.Object, mockWriter.Object);
            //--------------------------------Act-----------------------------------
            programImpl.Run();
            programImpl.Pause();
            //--------------------------------Assert--------------------------------
            mockConsoleWindowFactory.Verify(o => o.New(), Times.Once);
            mockLogServerFactory.Verify(o => o.New(It.IsAny<IWebSocketServerFactory>(), It.IsAny<IWriter>(), It.IsAny<ILoggerContext>()), Times.Once);
            mockWriter.Verify(o => o.ReadLine(), Times.Once);
        }
    }
}
