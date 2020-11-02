/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;
using HangfireServer;
using Warewolf.Execution;
using static HangfireServer.Program;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Warewolf.Common.NetStandard20.ExecutionLogger;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class HangfireServerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Program_ConnectionString_Exists()
        {
            var args = new Args
            {
                ShowConsole = true,
            };

            var implConfig = SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
                out Mock<IExecutionLoggerFactory> mockExecutionLoggerFactory, out Mock<IPauseHelper> mockPauseHelper,
                out Mock<IExitHelper> mockExitHelper);

            mockWriter.Setup(o => o.WriteLine("Starting Hangfire server...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Hangfire dashboard started...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Hangfire server started...")).Verifiable();

            var item = new Implementation(args, implConfig);
            var result = item.Run();

            Assert.AreEqual(0, result);
            mockWriter.Verify(o => o.WriteLine("Starting Hangfire server..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Hangfire dashboard started..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Hangfire server started..."), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Never);
            mockExitHelper.Verify(o => o.Exit(), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Program_ConnectionString_Payload_Empty()
        {
            var args = new Args
            {
                ShowConsole = false,
            };

            var implConfig = SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
                out Mock<IExecutionLoggerFactory> mockExecutionLoggerFactory, out Mock<IPauseHelper> mockPauseHelper,
                out Mock<IExitHelper> mockExitHelper);

            mockWriter.Setup(o => o.WriteLine("Starting Hangfire server...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine(
                        "Fatal Error: Could not find persistence config file. Hangfire server is unable to start.")).Verifiable();
            mockWriter.Setup(o => o.Write("Press any key to exit...")).Verifiable();

            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var persistenceSettings = new PersistenceSettings("", mockFile.Object, mockDirectory.Object);

            var item = new Implementation(args, implConfig, persistenceSettings);
            var result = item.Run();

            Assert.AreEqual(0, result);
            mockWriter.Verify(o => o.WriteLine("Starting Hangfire server..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine(
                    "Fatal Error: Could not find persistence config file. Hangfire server is unable to start."), Times.Once);
            mockWriter.Verify(o => o.Write("Press any key to exit..."), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Once);
            mockExitHelper.Verify(o => o.Exit(), Times.Once);
        }

        private Implementation.ConfigImpl SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
            out Mock<IExecutionLoggerFactory> mockExecutionLoggerFactory, out Mock<IPauseHelper> mockPauseHelper,
            out Mock<IExitHelper> mockExitHelper)
        {
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();

            mockWriter = new Mock<IWriter>();
            mockExecutionLoggerFactory = new Mock<IExecutionLoggerFactory>();
            mockExecutionLoggerFactory.Setup(o => o.New(It.IsAny<ISerializer>(), It.IsAny<IWebSocketPool>()))
                .Returns(mockExecutionLogPublisher.Object);

            mockPauseHelper = new Mock<IPauseHelper>();
            mockPauseHelper.Setup(o => o.Pause()).Verifiable();

            mockExitHelper = new Mock<IExitHelper>();
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