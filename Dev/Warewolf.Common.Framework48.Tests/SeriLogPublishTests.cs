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
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace Warewolf.Common.Framework48.Tests
{
    [TestClass]
    public class SeriLogPublishTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("SeriLogPublish")]
        public void SeriLogPublish_NewPublisher_WriteToSink_UsingAny_ILogEventSink_IPML_Success()
        {
            //-------------------------Arrange------------------------------
            var testEventSink = new TestLogEventSink();

            var seriConfig = new TestSeriLogSinkConfig(testEventSink);

            var loggerSource = new SeriLoggerSource();

            var loggerConnection = loggerSource.NewConnection(seriConfig);
            var loggerPublisher = loggerConnection.NewPublisher();

            var error = new { ServerName = "testServer", Error = "testError" };

            var expectedTestWarnMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            var expectedTestErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";

            //-------------------------Act----------------------------------
            loggerPublisher.Warn(expectedTestWarnMsgTemplate);
            loggerPublisher.Error(expectedTestErrorMsg);

            var actualLogEventList = testEventSink.LogData;
            //-------------------------Assert-------------------------------
            Assert.IsTrue(actualLogEventList.Count == 2);

            Assert.AreEqual(expected: LogEventLevel.Warning, actual: actualLogEventList[0].Level);
            Assert.AreEqual(expected: expectedTestWarnMsgTemplate, actual: actualLogEventList[0].MessageTemplate.Text);

            Assert.AreEqual(expected: LogEventLevel.Error, actual: actualLogEventList[1].Level);
            Assert.AreEqual(expected: expectedTestErrorMsg, actual: actualLogEventList[1].MessageTemplate.Text);

        }

        class TestLogEventSink : ILogEventSink
        {
            public List<LogEvent> LogData { get; private set; } = new List<LogEvent>();
            public void Emit(LogEvent logEvent)
            {
               LogData.Add(logEvent);
            }
        }

        class TestSeriLogSinkConfig : ISeriLogConfig
        {
            readonly ILogEventSink _logEventSink;

            public Logger Logger { get => CreateLogger(); }

            public TestSeriLogSinkConfig(ILogEventSink logEventSink)
            {
                _logEventSink = logEventSink;
            }

            public Logger CreateLogger()
            {
                return new LoggerConfiguration().WriteTo.Sink(logEventSink: _logEventSink).CreateLogger();
            }
        }

    }
}
