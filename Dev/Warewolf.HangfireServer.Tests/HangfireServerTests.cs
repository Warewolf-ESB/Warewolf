/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;
using HangfireServer;
using static HangfireServer.ExecutionLogger;
using static HangfireServer.Program;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    public class HangfireServerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Program_ConnectionString_False()
        {
            var args = new Args
            {
                ShowConsole = true,
            };

            var implConfig = SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
                out Mock<IExecutionLoggerFactory> mockExecutionLoggerFactory);

            var item = new Implementation(args, implConfig);
            var result = item.Run();

            Assert.AreEqual(0, result);
            mockWriter.Verify(o => o.Write("Starting Hangfire server..."), Times.Once);
        }

        private Implementation.ConfigImpl SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
            out Mock<IExecutionLoggerFactory> mockExecutionLoggerFactory)
        {
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();

            mockWriter = new Mock<IWriter>();
            mockExecutionLoggerFactory = new Mock<IExecutionLoggerFactory>();
            mockExecutionLoggerFactory.Setup(o => o.New(It.IsAny<ISerializer>(), It.IsAny<IWebSocketPool>())).Returns(mockExecutionLogPublisher.Object);

            var mockPauseHelper = new Mock<IPauseHelper>();
            mockPauseHelper.Setup(o => o.Pause()).Verifiable();

            var mockExitHelper = new Mock<IExitHelper>();
            mockExitHelper.Setup(o => o.Exit()).Verifiable();

            var implConfig = new Implementation.ConfigImpl
            {
                Writer = mockWriter.Object,
                ExecutionLoggerFactory = mockExecutionLoggerFactory.Object,
                PauseHelper = mockPauseHelper.Object,
                ExitHelper = mockExitHelper.Object,
            };

            return implConfig;
        }
    }
}