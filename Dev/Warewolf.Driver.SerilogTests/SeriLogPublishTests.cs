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
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogPublishTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewPublisher_WriteToSink_UsingAny_ILogEventSink_IPML_Success()
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewPublisher_WriteToSink_UsingAny_ILogEventSink_IPML_WithOutputTemplateFormat_Test_Success()
        {
            //-------------------------Arrange------------------------------
            var testEventSink = new TestLogEventSink();

            var seriConfig = new TestSeriLogSinkConfig(testEventSink);

            var loggerSource = new SeriLoggerSource();

            var loggerConnection = loggerSource.NewConnection(seriConfig);
            var loggerPublisher = loggerConnection.NewPublisher();

            var error = new { ServerName = "testServer", Error = "testError" };
            var fatal = new { ServerName = "testServer", Error = "testFatalError" };

            var expectedTestInfoMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}";
            var expectedTestErrorMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            var expectedTestFatalMsg = "test infomation {testFatalKey}";

            var expectedTestErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";
            //-------------------------Act----------------------------------

            loggerPublisher.Info(expectedTestInfoMsgTemplate, DateTime.Now, LogEventLevel.Information, "test message");
            loggerPublisher.Error(expectedTestErrorMsgTemplate, DateTime.Now, LogEventLevel.Error, expectedTestErrorMsg, Environment.NewLine, "test exception");
            loggerPublisher.Fatal(expectedTestFatalMsg, @fatal);

            var actualLogEventList = testEventSink.LogData;

            var item1 = actualLogEventList[0].Properties.FirstOrDefault(o => o.Key == "Message");
            var item2 = actualLogEventList[1].Properties.First(o => o.Key == "Message");

            //-------------------------Assert-------------------------------
            Assert.IsTrue(actualLogEventList.Count == 3);

            Assert.AreEqual(expected: LogEventLevel.Information, actual: actualLogEventList[0].Level);
            Assert.AreEqual(expected: expectedTestInfoMsgTemplate, actual: actualLogEventList[0].MessageTemplate.Text);

            Assert.AreEqual(expected: "Message", actual: item1.Key);
            Assert.AreEqual(expected: "\"test message\"".ToString(), actual: item1.Value.ToString());

            Assert.AreEqual(expected: LogEventLevel.Error, actual: actualLogEventList[1].Level);
            Assert.AreEqual(expected: expectedTestErrorMsgTemplate, actual: actualLogEventList[1].MessageTemplate.Text);
            Assert.AreEqual(expected: "\"Error From: testServer : testError \"", actual: item2.Value.ToString());

            Assert.AreEqual(expected: LogEventLevel.Fatal, actual: actualLogEventList[2].Level);
            Assert.AreEqual(expected: 1, actual: actualLogEventList[2].Properties.Count);
            Assert.IsTrue(actualLogEventList[2].Properties.ContainsKey("testFatalKey"));

            Assert.AreEqual(1, actualLogEventList[2].Properties.Values.Count());
            Assert.AreEqual(expected: expectedTestFatalMsg, actual: actualLogEventList[2].MessageTemplate.Text);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewConsumer_Reading_LogData_From_SQLite_NoParamCTOR_Success()
        {
            var defaultSqlitePath = @"C:\ProgramData\Warewolf\Audits\auditDB.db";
            File.Delete(defaultSqlitePath);
            //-------------------------Arrange------------------------------
            var seriConfig = new SeriLogSQLiteConfig();
            var loggerSource = new SeriLoggerSource();

            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();

                var error = new { ServerName = "testServer", Error = "testError" };
                var fatal = new { ServerName = "testServer", Error = "testFatalError" };

                var expectedTestErrorMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
                var expectedTestFatalMsg = "test infomation {testFatalKey}";

                var testErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";

                //-------------------------Act----------------------------------

                loggerPublisher.Info("test message");
                loggerPublisher.Error(expectedTestErrorMsgTemplate, DateTime.Now, LogEventLevel.Error, testErrorMsg, Environment.NewLine, "test exception");
                loggerPublisher.Fatal(expectedTestFatalMsg, @fatal);
            };

            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {

                var loggerConsumer = loggerConnection.NewConsumer();
                var settings = new SeriLogSQLiteConfig.Settings();
                var dataList = loggerConsumer.GetData(connectionString: settings.ConnectionString, settings.TableName);

                var actItem1 = JsonConvert.DeserializeObject<SeriLogData>(dataList[0][0]);
                var actItem2 = JsonConvert.DeserializeObject<SeriLogData>(dataList[1][0]);
                var actItem3 = JsonConvert.DeserializeObject<SeriLogData>(dataList[2][0]);

                Assert.AreEqual(expected: 3, actual: dataList.Count);

                Assert.AreEqual(expected: null, actual: actItem1);
                Assert.AreEqual(expected: "Error From: testServer : testError ", actual: actItem2.Message);
                Assert.AreEqual(expected: null, actual: actItem3.Message);
            }

        }
       
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewConsumer_Reading_LogData_From_SQLite_WithParamCTOR_Success()
        {
            //-------------------------Arrange------------------------------
            var settings = new SeriLogSQLiteConfig.Settings
            {
                TableName = "testLogs",
                Path = @"C:\Test\Warewolf\db\",
                Database = "testAudits.db"
            };

            File.Delete(settings.ConnectionString);

            var seriConfig = new SeriLogSQLiteConfig(settings);
            var loggerSource = new SeriLoggerSource();

            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();

                var error = new { ServerName = "testServer", Error = "testError" };
                var fatal = new { ServerName = "testServer", Error = "testFatalError" };

                var expectedTestErrorMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
                var expectedTestFatalMsg = "test infomation {testFatalKey}";

                var testErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";

                //-------------------------Act----------------------------------

                loggerPublisher.Info("test message");
                loggerPublisher.Error(expectedTestErrorMsgTemplate, DateTime.Now, LogEventLevel.Error, testErrorMsg, Environment.NewLine, "test exception");
                loggerPublisher.Fatal(expectedTestFatalMsg, @fatal);
            };

            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerConsumer = loggerConnection.NewConsumer();
                var dataList = loggerConsumer.GetData(connectionString: settings.ConnectionString, settings.TableName);

                var actItem1 = JsonConvert.DeserializeObject<SeriLogData>(dataList[0][0]);
                var actItem2 = JsonConvert.DeserializeObject<SeriLogData>(dataList[1][0]);
                var actItem3 = JsonConvert.DeserializeObject<SeriLogData>(dataList[2][0]);

                Assert.AreEqual(expected: 3, actual: dataList.Count);

                Assert.AreEqual(expected: null, actual: actItem1);
                Assert.AreEqual(expected: "Error From: testServer : testError ", actual: actItem2.Message);
                Assert.AreEqual(expected: null, actual: actItem3.Message);
            }
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

            public ILogger Logger { get => CreateLogger(); }

            public string ConnectionString => throw new NotImplementedException();

            public TestSeriLogSinkConfig(ILogEventSink logEventSink)
            {
                _logEventSink = logEventSink;
            }

            private ILogger CreateLogger()
            {
                return new LoggerConfiguration().WriteTo.Sink(logEventSink: _logEventSink).CreateLogger();
            }
        }
    }
}
