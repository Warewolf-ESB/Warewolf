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
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Dev2.Common;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogPublishTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
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
            var fatal = new {ServerName = "testServer", Error = "testFatalError"};
            var info = new {Message = "test message"};

            //-------------------------Act----------------------------------

            loggerPublisher.Info(GlobalConstants.WarewolfLogsTemplate, info);
            loggerPublisher.Error(GlobalConstants.WarewolfLogsTemplate, error);
            loggerPublisher.Fatal(GlobalConstants.WarewolfLogsTemplate, fatal);

            var actualLogEventList = testEventSink.LogData;
            //-------------------------Assert-------------------------------
            Assert.AreEqual(3, actualLogEventList.Count);

            Assert.AreEqual(expected: LogEventLevel.Information, actual: actualLogEventList[0].Level);
            Assert.AreEqual(expected: GlobalConstants.WarewolfLogsTemplate, actual: actualLogEventList[0].MessageTemplate.Text);
            var o1 = JObject.Parse(actualLogEventList[0].Properties["Data"].ToString());
            Assert.AreEqual(expected: "test message", actual: o1["Message"].ToString());

            Assert.AreEqual(expected: LogEventLevel.Error, actual: actualLogEventList[1].Level);
            Assert.AreEqual(expected: GlobalConstants.WarewolfLogsTemplate, actual: actualLogEventList[1].MessageTemplate.Text);
            var o2 = JObject.Parse(actualLogEventList[1].Properties["Data"].ToString());
            Assert.AreEqual(expected: "testServer", o2["ServerName"].ToString());
            Assert.AreEqual(expected: "testError", o2["Error"].ToString());

            Assert.AreEqual(expected: LogEventLevel.Fatal, actual: actualLogEventList[2].Level);
            Assert.AreEqual(expected: GlobalConstants.WarewolfLogsTemplate, actual: actualLogEventList[2].MessageTemplate.Text);
            var o3 = JObject.Parse(actualLogEventList[2].Properties["Data"].ToString());
            Assert.AreEqual(expected: "testServer", o3["ServerName"].ToString());
            Assert.AreEqual(expected: "testFatalError", o3["Error"].ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
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
            var dataFromDb = new TestSqliteDatabase(loggerSource.ConnectionString, loggerSource.TableName);
            var dataList = dataFromDb.GetPublishedData().ToList();

            var o1 = JObject.Parse(dataList[0].Properties);
            Assert.IsNotNull(dataList[0].Timestamp);
            Assert.AreEqual(null, dataList[0].Exception);
            Assert.AreEqual(LogEventLevel.Information, dataList[0].Level);
            Assert.AreEqual(expected: "test message", actual: o1["Data"].ToString());

            var o2 = JObject.Parse(dataList[1].Properties);
            Assert.IsNotNull(dataList[1].Timestamp);
            Assert.AreEqual(null, dataList[1].Exception);
            Assert.AreEqual(LogEventLevel.Error, dataList[1].Level);
            Assert.AreEqual(expected: "Error From: testServer : testError ", actual: o2["Data"].ToString());

            var o3 = JObject.Parse(dataList[2].Properties);
            Assert.IsNotNull(dataList[2].Timestamp);
            Assert.AreEqual(null, dataList[2].Exception);
            Assert.AreEqual(LogEventLevel.Fatal, dataList[2].Level);
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

        class TestSqliteDatabase
        {
            private string _connectionString;
            private string _tableName;

            public TestSqliteDatabase(string connectionString, string tableName)
            {
                _connectionString = connectionString;
                _tableName = tableName;
            }

            public IEnumerable<SeriLogLogEvent> GetPublishedData()
            {
                try
                {
                    using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
                    {
                        var sql = new StringBuilder($"SELECT * FROM {_tableName} ");
                        using (var command = new SQLiteCommand(sql.ToString(), sqlConn))
                        {
                            sqlConn.Open();
                            var list = new List<SeriLogLogEvent>();
                            using (var reader = command.ExecuteReader())
                            {
                                foreach (DbDataRecord s in reader)
                                {
                                    Enum.TryParse(s["Level"].ToString(), out LogEventLevel level);
                                    var log = new SeriLogLogEvent
                                    {
                                        Timestamp = DateTimeOffset.Parse(s["Timestamp"].ToString()),
                                        Level = level,
                                        Exception = s["Exception"] as Exception,
                                        Properties = s["Properties"].ToString()
                                    };
                                    list.Add(log);
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
}