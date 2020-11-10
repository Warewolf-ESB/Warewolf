/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.AccessControl;
using System.Threading;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Interfaces.Auditing;
using HangfireServer;
using Newtonsoft.Json.Serialization;
using Warewolf.Data;
using Warewolf.Execution;
using static HangfireServer.Program;
using static Warewolf.Common.NetStandard20.ExecutionLogger;
using ISerializer = Warewolf.Streams.ISerializer;

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

            var implConfig = SetupHangfireImplementationConfigs(out var mockWriter, out var mockPauseHelper, out var mockExecutionLogPublisher);

            var dbSource = new DbSource
            {
                ConnectionString = "connectionString",
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            var serializeToJsonString = dbSource.SerializeToJsonString(new DefaultSerializationBinder());
            compressedExecuteMessage.SetMessage(serializeToJsonString);

            var mockSerializer = new Mock<IBuilderSerializer>();

            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var persistenceSettings = new PersistenceSettings("some path", mockFile.Object, mockDirectory.Object)
            {
                Enable = false,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source", Value = Guid.Empty, Payload = serializeToJsonString
                },
            };
            mockSerializer.Setup(o => o.Deserialize<DbSource>(persistenceSettings.PersistenceDataSource.Payload)).Returns(dbSource).Verifiable();
            var mockContext = new HangfireContext(args);
            var waitHandle = new EventWaitHandleTesting(false, EventResetMode.ManualReset);
            var item = new Implementation(mockContext, implConfig, persistenceSettings, mockSerializer.Object, waitHandle);
            item.Run();

            mockWriter.Verify(o => o.WriteLine("Starting Hangfire server..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Starting Hangfire server..."), Times.Once);
            mockSerializer.Verify(o => o.Deserialize<DbSource>(persistenceSettings.PersistenceDataSource.Payload), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Hangfire dashboard started..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire dashboard started..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Hangfire server started..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire server started..."), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Program_ConnectionString_Payload_Empty_Pause()
        {
            var args = new Args
            {
                ShowConsole = true,
            };

            var implConfig = SetupHangfireImplementationConfigs(out var mockWriter, out var mockPauseHelper, out var mockExecutionLogPublisher);

            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var persistenceSettings = new PersistenceSettings("", mockFile.Object, mockDirectory.Object);
            var mockContext = new HangfireContext(args);
            var waitHandle = new EventWaitHandleTesting(false, EventResetMode.ManualReset);
            var item = new Implementation(mockContext, implConfig, persistenceSettings, new Mock<IBuilderSerializer>().Object, waitHandle);
            item.Run();
            item.WaitForExit();

            mockExecutionLogPublisher.Verify(o => o.Info("Starting Hangfire server..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Fatal Error: Could not find persistence config file. Hangfire server is unable to start."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("Fatal Error: Could not find persistence config file. Hangfire server is unable to start."), Times.Once);
            mockWriter.Verify(o => o.Write("Press any key to exit..."), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Hangfire dashboard started..."), Times.Never);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire dashboard started..."), Times.Never);
            mockWriter.Verify(o => o.WriteLine("Hangfire server started..."), Times.Never);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire server started..."), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Program_ConnectionString_Payload_Empty_WaitOne()
        {
            var args = new Args
            {
                ShowConsole = false,
            };

            var implConfig = SetupHangfireImplementationConfigs(out var mockWriter, out var mockPauseHelper, out var mockExecutionLogPublisher);

            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var persistenceSettings = new PersistenceSettings("", mockFile.Object, mockDirectory.Object);
            var mockContext = new HangfireContext(args);
            var waitHandle = new EventWaitHandleTesting(false, EventResetMode.AutoReset);
            var item = new Implementation(mockContext, implConfig, persistenceSettings, new Mock<IBuilderSerializer>().Object, waitHandle);
            item.Run();
            item.WaitForExit();

            mockWriter.Verify(o => o.WriteLine("Starting Hangfire server..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Starting Hangfire server..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Fatal Error: Could not find persistence config file. Hangfire server is unable to start."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("Fatal Error: Could not find persistence config file. Hangfire server is unable to start."), Times.Once);
            mockWriter.Verify(o => o.Write("Press any key to exit..."), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Never);
            mockWriter.Verify(o => o.WriteLine("Hangfire dashboard started..."), Times.Never);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire dashboard started..."), Times.Never);
            mockWriter.Verify(o => o.WriteLine("Hangfire server started..."), Times.Never);
            mockExecutionLogPublisher.Verify(o => o.Info("Hangfire server started..."), Times.Never);
        }

        private static Implementation.ConfigImpl SetupHangfireImplementationConfigs(out Mock<IWriter> mockWriter,
            out Mock<IPauseHelper> mockPauseHelper, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher)
        {
            mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Starting Hangfire server...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Hangfire dashboard started...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Hangfire server started...")).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Fatal Error: Could not find persistence config file. Hangfire server is unable to start.")).Verifiable();
            mockWriter.Setup(o => o.Write("Press any key to exit...")).Verifiable();

            mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            mockExecutionLogPublisher.Setup(o => o.Info("Starting Hangfire server...")).Verifiable();
            mockExecutionLogPublisher.Setup(o => o.Info("Hangfire dashboard started...")).Verifiable();
            mockExecutionLogPublisher.Setup(o => o.Info("Hangfire server started...")).Verifiable();
            mockExecutionLogPublisher.Setup(o => o.Error("Fatal Error: Could not find persistence config file. Hangfire server is unable to start.")).Verifiable();

            var mockExecutionLoggerFactory = new Mock<IExecutionLoggerFactory>();
            mockExecutionLoggerFactory.Setup(o => o.New(It.IsAny<ISerializer>(), It.IsAny<IWebSocketPool>()))
                .Returns(mockExecutionLogPublisher.Object);

            mockPauseHelper = new Mock<IPauseHelper>();
            mockPauseHelper.Setup(o => o.Pause()).Verifiable();

            var implConfig = new Implementation.ConfigImpl
            {
                Writer = mockWriter.Object,
                ExecutionLoggerFactory = mockExecutionLoggerFactory.Object,
                PauseHelper = mockPauseHelper.Object,
            };

            return implConfig;
        }

        private class EventWaitHandleTesting : EventWaitHandle
        {
            public EventWaitHandleTesting(bool initialState, EventResetMode mode) : base(initialState, mode)
            {
                base.Set();
            }

            public EventWaitHandleTesting(bool initialState, EventResetMode mode, string name) : base(initialState, mode, name)
            {
            }

            public EventWaitHandleTesting(bool initialState, EventResetMode mode, string name, out bool createdNew) : base(initialState, mode, name, out createdNew)
            {
            }

            public EventWaitHandleTesting(bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity) : base(initialState, mode, name, out createdNew, eventSecurity)
            {
            }
        }
    }
}