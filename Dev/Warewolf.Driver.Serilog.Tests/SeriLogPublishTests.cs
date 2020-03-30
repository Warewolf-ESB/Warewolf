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
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Newtonsoft.Json.Linq;

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

            var error = new {ServerName = "testServer", Error = "testError"};

            var expectedTestWarnMsgTemplate = GlobalConstants.WarewolfLogsTemplate;
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

            var error = new {ServerName = "testServer", Error = "testError"};
            var fatal = new {ServerName = "testServer", Error = "testFatalError"};

            var expectedTestInfoMsgTemplate = GlobalConstants.WarewolfLogsTemplate;
            var expectedTestErrorMsgTemplate = GlobalConstants.WarewolfLogsTemplate;
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
        public void SeriLogPublisher_NewPublisher_Reading_LogData_From_SQLite_Success()
        {
            //-------------------------Arrange------------------------------
            var testDBPath = @"C:\ProgramData\Warewolf\Audits\AuditTestDB.db";
            if (File.Exists(testDBPath))
                File.Delete(testDBPath);

            var testTableName = "Logs";
            var logger = new LoggerConfiguration().WriteTo.SQLite(testDBPath, testTableName).CreateLogger();

            var mockSeriLogConfig = new Mock<ISeriLogConfig>();
            mockSeriLogConfig.SetupGet(o => o.Logger).Returns(logger);

            var loggerSource = new SeriLogSQLiteSource
            {
                ConnectionString = testDBPath,
                TableName = testTableName
            };

            using (var loggerConnection = loggerSource.NewConnection(mockSeriLogConfig.Object))
            {
                var loggerPublisher = loggerConnection.NewPublisher();

                var error = new {ServerName = "testServer", Error = "testError"};
                var fatal = new {ServerName = "testServer", Error = "testFatalError"};

                var expectedTestErrorMsgTemplate = GlobalConstants.WarewolfLogsTemplate;
                var expectedTestFatalMsg = "test infomation {testFatalKey}";
                var testErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";

                //-------------------------Act----------------------------------

                loggerPublisher.Info(GlobalConstants.WarewolfLogsTemplate, "test message");
                loggerPublisher.Error(expectedTestErrorMsgTemplate, testErrorMsg);
                loggerPublisher.Fatal(expectedTestFatalMsg, @fatal);
            }

            //-------------------------Assert------------------------------------
            var dataFromDb = new TestDatabase(loggerSource.ConnectionString, loggerSource.TableName);
            var dataList = dataFromDb.GetPublishedData().ToList();

            var o1 = JObject.Parse(dataList[0].Message);
            Assert.IsNotNull(dataList[0].Timestamp);
            Assert.AreEqual("", dataList[0].Exception);
            Assert.AreEqual(expected: "test message", actual: o1["Data"].ToString());

            var o2 = JObject.Parse(dataList[1].Message);
            Assert.IsNotNull(dataList[1].Timestamp);
            Assert.AreEqual("", dataList[1].Exception);
            Assert.AreEqual(expected: "Error From: testServer : testError ", actual: o2["Data"].ToString());

            var o3 = JObject.Parse(dataList[2].Message);
            Assert.IsNotNull(dataList[2].Timestamp);
            Assert.AreEqual("", dataList[2].Exception);
            Assert.AreEqual(expected: "{ ServerName = testServer, Error = testFatalError }", o3["testFatalKey"].ToString());
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

            public ILogger Logger => CreateLogger();

            public TestSeriLogSinkConfig(ILogEventSink logEventSink)
            {
                _logEventSink = logEventSink;
            }

            private ILogger CreateLogger()
            {
                return new LoggerConfiguration().WriteTo.Sink(logEventSink: _logEventSink).CreateLogger();
            }

            public string Endpoint { get; set; }
        }

        class TestDatabase
        {
            private string _connectionString;
            private string _tableName;

            public TestDatabase(string connectionString, string tableName)
            {
                _connectionString = connectionString;
                _tableName = tableName;
            }

            public IEnumerable<SeriLogData> GetPublishedData()
            {
                try
                {
                    using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
                    {
                        var sql = new StringBuilder($"SELECT * FROM {_tableName} ");

                        using (var command = new SQLiteCommand(sql.ToString(), sqlConn))
                        {
                            sqlConn.Open();
                            var list = new List<SeriLogData>();
                            using (var reader = command.ExecuteReader())
                            {
                                foreach (DbDataRecord s in reader)
                                {
                                    var seriLogData =
                                        new SeriLogData
                                        {
                                            Timestamp = s["Timestamp"].ToString(),
                                            Message = s["Properties"].ToString(),
                                            Level = s["Level"].ToString(),
                                            Exception = s["Exception"].ToString()
                                        };
                                    list.Add(seriLogData);
                                }
                            }
                            return list;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }

    class TestSeriLogSQLiteConfig : ISeriLogConfig
    {
        readonly SeriLogSQLiteConfig.Settings _config;

        public TestSeriLogSQLiteConfig()
        {
            _config = new SeriLogSQLiteConfig.Settings();
        }

        public TestSeriLogSQLiteConfig(SeriLogSQLiteConfig.Settings sqlConfig)
        {
            _config = sqlConfig;
        }

        public ILogger Logger
        {
            get => CreateLogger();
        }

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo
                .SQLite(sqliteDbPath: _config.Path, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, formatProvider: _config.FormatProvider, storeTimestampInUtc: _config.StoreTimestampInUtc, retentionPeriod: _config.RetentionPeriod)
                .CreateLogger();
        }

        public string Endpoint { get; set; }
    }
}