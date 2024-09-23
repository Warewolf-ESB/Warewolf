/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Newtonsoft.Json.Linq;
using Serilog.Sinks.Elasticsearch;
using Warewolf.Auditing;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;
using Audit = Warewolf.Auditing.Audit;
using File = System.IO.File;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using System.Text.Json;
using Serilog.Debugging;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogPublishTests
    {
        const string DefaultPort = "9200";

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewPublisher_Sqlite_WriteToSink_UsingAny_ILogEventSink_IPML_Success()
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

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogPublisher))]
        public void SeriLogPublisher_NewPublisher_Elasticsearch_WriteToSink_UsingAny_ILogEventSink_IPML_Success()
        {
            //-------------------------Arrange------------------------------
            var testEventSink = new TestLogEventSink();
            var seriConfig = new TestSeriLogSinkConfig(testEventSink);
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var loggerSource = new SerilogElasticsearchSource
            {
                Port = DefaultPort,
                HostName = hostName,
            };

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SeriLogPublisher_NewPublisher_Reading_LogData_From_Elasticsearch_Success()
        {
            //-------------------------Arrange------------------------------
            Config.Server.IncludeEnvironmentVariable = false;
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var loggerSource = new SerilogElasticsearchSource
            {
                Port = dependency.Container.Port,
                HostName = hostName,
                SearchIndex = "warewolftestlogs",
                Username = "test",
                Password = "test123"
            };
            var uri = new Uri(hostName + ":" + dependency.Container.Port);
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(uri)
                {
                    AutoRegisterTemplate = true,
                    IndexDecider = (e, o) => loggerSource.SearchIndex,
                    ModifyConnectionSettings = connectionConfiguration =>
                    {
                        var settings = connectionConfiguration.BasicAuthentication(loggerSource.Username, loggerSource.Password).EnableDebugMode(response =>
                        {
                            Console.WriteLine(response.DebugInformation);
                        });
                        return settings;
                    }
                })
                .CreateLogger();

            var mockSeriLogConfig = new Mock<ISeriLogConfig>();
            mockSeriLogConfig.SetupGet(o => o.Logger).Returns(logger);

            var executionID = Guid.NewGuid();
            using (var loggerConnection = loggerSource.NewConnection(mockSeriLogConfig.Object))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                var mockDataObject = SetupDataObjectWithAssignedInputs(executionID);
                var auditLog = new Audit(mockDataObject.Object, "LogAdditionalDetail", "Test", null, null);
                Assert.AreEqual("",auditLog.Environment);
                mockDataObject.Verify(o => o.Environment.ToJson(), Times.Never);
                var logEntryCommand = new AuditCommand
                {
                    Audit = auditLog,
                    Type = "LogEntry"
                };
                //-------------------------Act----------------------------------
                loggerPublisher.Info(GlobalConstants.WarewolfLogsTemplate, logEntryCommand);
            }
            Task.Delay(500).Wait();
            //-------------------------Assert------------------------------------
            var dataFromDb = new TestElasticsearchDatabase();

             var dataList = dataFromDb.GetPublishedData(loggerSource, executionID.ToString()).ToList();

            Assert.AreEqual(1,dataList.Count);

            foreach (var jsonElement in dataList)
            {
                var fields = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());
                var level = fields.Where(pair => pair.Key.Contains("level")).Select(pair => pair.Value).FirstOrDefault();
                Assert.AreEqual("Information", level.ToString());

                var messageTemplate = fields.Where(pair => pair.Key.Contains("messageTemplate")).Select(pair => pair.Value).FirstOrDefault();
                Assert.AreEqual("{@Data}", messageTemplate.ToString());

                var message = fields.Where(pair => pair.Key.Contains("message"));
                Assert.IsNotNull(message);
            }
        }

        public static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        dict[property.Name] = JsonElementToDictionary(property.Value);
                        break;
                    case JsonValueKind.Array:
                        dict[property.Name] = JsonElementToList(property.Value);
                        break;
                    case JsonValueKind.String:
                        dict[property.Name] = property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        dict[property.Name] = property.Value.GetDecimal();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        dict[property.Name] = property.Value.GetBoolean();
                        break;
                    case JsonValueKind.Null:
                        dict[property.Name] = null;
                        break;
                    default:
                        dict[property.Name] = property.Value.ToString();
                        break;
                }
            }

            return dict;
        }

        public static List<object> JsonElementToList(JsonElement element)
        {
            var list = new List<object>();

            foreach (JsonElement item in element.EnumerateArray())
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                        list.Add(JsonElementToDictionary(item));
                        break;
                    case JsonValueKind.Array:
                        list.Add(JsonElementToList(item));
                        break;
                    case JsonValueKind.String:
                        list.Add(item.GetString());
                        break;
                    case JsonValueKind.Number:
                        list.Add(item.GetDecimal());
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        list.Add(item.GetBoolean());
                        break;
                    case JsonValueKind.Null:
                        list.Add(null);
                        break;
                    default:
                        list.Add(item.ToString());
                        break;
                }
            }

            return list;
        }

        Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid executionId)
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            var mock = new Mock<IExecutionEnvironment>();
            mock.Setup(o => o.ToJson()).Returns("Not an empty string");
            mockedDataObject.Setup(o => o.Environment).Returns(mock.Object);
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Test-Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => Guid.NewGuid());
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId );
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }

        class TestElasticsearchDatabase
        {
            public IEnumerable<object> GetPublishedData(SerilogElasticsearchSource source, string executionID)
            {
                var uri = new Uri(source.HostName + ":" + source.Port);
                var settings = new ElasticsearchClientSettings(uri)
                    .RequestTimeout(TimeSpan.FromMinutes(2))
                    .DefaultIndex(source.SearchIndex)
                    .DisableDirectStreaming()
                    .Authentication(new BasicAuthentication(source.Username, source.Password))
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);

                var client = new ElasticsearchClient(settings);
                var result = client.Ping();
                var isValid = result.IsValidResponse;
                if (!isValid)
                {
                    throw new Exception("Invalid Data Source");
                }
                else
                {
                    var search = new SearchRequestDescriptor<object>()
                        .Query(q =>
                            q.Bool(b => b
                                .Must(m => m
                                    .Match(mt => mt
                                        .Field("fields.Data.Audit.ExecutionID")
                                        .Query(executionID)
                                    )
                                )
                            )
                        );

                    var logEvents = client.Search<object>(search);
                    var sources = logEvents.HitsMetadata.Hits.Select(h => h.Source);
                    return sources;
                }
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
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}