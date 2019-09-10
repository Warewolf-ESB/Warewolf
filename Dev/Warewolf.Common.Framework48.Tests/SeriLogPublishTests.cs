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
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Warewolf.Logging.SeriLog;

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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("SeriLogPublish")]
        public void SeriLogPublish_NewPublisher_WriteToSink_UsingAny_ILogEventSink_IPML_WithOutputTemplateFormat_Test_Success()
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
            loggerPublisher.Error(expectedTestErrorMsgTemplate , DateTime.Now, LogEventLevel.Error, expectedTestErrorMsg, Environment.NewLine, "test exception");
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
        [TestCategory("SeriLogPublish")]
        public void SeriLogPublish_NewPublisher_WriteToSink_UsingAny_ILogEventSink_IPML_Success1()
        {
            //-------------------------Arrange------------------------------
            var testSqlitePath = @"C:\Test\Warewolf\db\testAudits.db";
            File.Delete(testSqlitePath);

            Assert.IsFalse(File.Exists(testSqlitePath));

            var seriConfig = new TestSeriLogSQLiteConfig(sqlPath:testSqlitePath);


            var loggerSource = new SeriLoggerSource();

            var loggerConnection = loggerSource.NewConnection(seriConfig);

            Assert.IsTrue(File.Exists(testSqlitePath));

            var loggerPublisher = loggerConnection.NewPublisher();

            var error = new { ServerName = "testServer", Error = "testError" };
            var fatal = new { ServerName = "testServer", Error = "testFatalError" };

            var expectedTestInfoMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}";
            var expectedTestErrorMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            var expectedTestFatalMsg = "test infomation {testFatalKey}";

            var expectedTestErrorMsg = $"Error From: {@error.ServerName} : {error.Error} ";

            //-------------------------Act----------------------------------

            loggerPublisher.Info("test message");
            loggerPublisher.Error(expectedTestErrorMsgTemplate, DateTime.Now, LogEventLevel.Error, expectedTestErrorMsg, Environment.NewLine, "test exception");
            loggerPublisher.Fatal(expectedTestFatalMsg, @fatal);

            //var loggerConsumer = loggerConnection.Consumer();

            //var item1 = actualLogEventList[0].Properties.FirstOrDefault(o => o.Key == "Message");
            //var item2 = actualLogEventList[1].Properties.First(o => o.Key == "Message");

            ////-------------------------Assert-------------------------------
            //Assert.IsTrue(actualLogEventList.Count == 3);

            //Assert.AreEqual(expected: LogEventLevel.Information, actual: actualLogEventList[0].Level);
            //Assert.AreEqual(expected: expectedTestInfoMsgTemplate, actual: actualLogEventList[0].MessageTemplate.Text);

            //Assert.AreEqual(expected: "Message", actual: item1.Key);
            //Assert.AreEqual(expected: "\"test message\"".ToString(), actual: item1.Value.ToString());

            //Assert.AreEqual(expected: LogEventLevel.Error, actual: actualLogEventList[1].Level);
            //Assert.AreEqual(expected: expectedTestErrorMsgTemplate, actual: actualLogEventList[1].MessageTemplate.Text);
            //Assert.AreEqual(expected: "\"Error From: testServer : testError \"", actual: item2.Value.ToString());

            //Assert.AreEqual(expected: LogEventLevel.Fatal, actual: actualLogEventList[2].Level);
            //Assert.AreEqual(expected: 1, actual: actualLogEventList[2].Properties.Count);
            //Assert.IsTrue(actualLogEventList[2].Properties.ContainsKey("testFatalKey"));

            //Assert.AreEqual(1, actualLogEventList[2].Properties.Values.Count());
            //Assert.AreEqual(expected: expectedTestFatalMsg, actual: actualLogEventList[2].MessageTemplate.Text);

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

            private Logger CreateLogger()
            {
               return new LoggerConfiguration().WriteTo.Sink(logEventSink: _logEventSink).CreateLogger();
                
            }
        }

    }

    internal class TestSeriLogSQLiteConfig : ISeriLogConfig
    {
        readonly string _sqlPath;

        public TestSeriLogSQLiteConfig(string sqlPath)
        {
            _sqlPath = sqlPath;
        }

        public Logger Logger { get => CreateLogger(); }

        private Logger CreateLogger()
        {
            var config = new SeriLogSQLiteConfig.Config
            {
                TableName = "testLogs",
                RestrictedToMinimumLevel = LogEventLevel.Information,
            };

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo
                .SQLite(_sqlPath) //, tableName: config.TableName, restrictedToMinimumLevel: config.RestrictedToMinimumLevel, formatProvider: config.FormatProvider, storeTimestampInUtc: config.StoreTimestampInUtc, retentionPeriod: config.RetentionPeriod)
                .CreateLogger();
        }
    }

}
