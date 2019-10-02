﻿/*
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
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

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
        public void SeriLogPublisher_NewPublisher_Reading_LogData_From_SQLite_Success()
        {
            //-------------------------Arrange------------------------------
            var testDBPath = @"C:\ProgramData\Warewolf\Audits\AuditTestDB.db";
            if (File.Exists(testDBPath))
                File.Delete(testDBPath);

            var testTableName = "Logs";
            var logger = new LoggerConfiguration().WriteTo.SQLite(testDBPath, testTableName).CreateLogger();

            var mockSeriLogConfig = new Mock<ISeriLogConfig>();
            mockSeriLogConfig.SetupGet(o => o.ConnectionString).Returns(testDBPath);
            mockSeriLogConfig.SetupGet(o => o.Logger).Returns(logger);

            var loggerSource = new SeriLoggerSource
            {
                ConnectionString = testDBPath,
                TableName = testTableName
            };

            using (var loggerConnection = loggerSource.NewConnection(mockSeriLogConfig.Object))
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

            //-------------------------Assert------------------------------------
            var dataFromDB = new TestDatabase(loggerSource.ConnectionString, loggerSource.TableName);
            var dataList = dataFromDB.GetPublishedData().ToList();

            Assert.IsNull(dataList[0]);

            Assert.AreEqual(expected: LogEventLevel.Error, actual: dataList[1].Level);
            Assert.AreEqual(expected: "\r\n", actual: dataList[1].NewLine);
            Assert.AreEqual(expected: "test exception", actual: dataList[1].Exception);
            Assert.AreEqual(expected: "Error From: testServer : testError ", actual: dataList[1].Message);

            Assert.AreEqual(expected: LogEventLevel.Verbose, actual: dataList[2].Level);
            Assert.AreEqual(expected: null, actual: dataList[2].NewLine);
            Assert.AreEqual(expected: null, actual: dataList[2].Exception);
            Assert.AreEqual(expected: null, actual: dataList[2].Message);

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
            public string ServerLoggingAddress { get; set; }

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
                using (var sqlConn = new SQLiteConnection(connectionString: "Data Source=" + _connectionString + ";"))
                {
                    var sql = new StringBuilder($"SELECT * FROM {_tableName} ");

                    using (var command = new SQLiteCommand(sql.ToString(), sqlConn))
                    {
                        sqlConn.Open();
                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var results = reader.GetValues();
                                var value = results.GetValues("Properties");

                                var serilogData = JsonConvert.DeserializeObject<SeriLogData>(value[0]);

                                yield return serilogData;
                            }
                        }
                    };

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

        public ILogger Logger { get => CreateLogger(); }
        public string ServerLoggingAddress { get; set; }
        public string ConnectionString { get; }

        private ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo
                .SQLite(sqliteDbPath: _config.Path, tableName: _config.TableName, restrictedToMinimumLevel: _config.RestrictedToMinimumLevel, formatProvider: _config.FormatProvider, storeTimestampInUtc: _config.StoreTimestampInUtc, retentionPeriod: _config.RetentionPeriod)
                .CreateLogger();
        }
    }

}
