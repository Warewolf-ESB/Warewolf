/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Moq;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Warewolf.Auditing.Drivers;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;
using Warewolf.Storage;
using Warewolf.UnitTestAttributes;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class AuditQueryableElasticTests
    {
        private IAuditQueryable GetAuditQueryable(string sink)
        {
            if (sink == "AuditingSettingsData")
            {
                var dependency = new Depends(Depends.ContainerType.Elasticsearch);
                var hostName = "http://" + dependency.Container.IP;
                return new AuditQueryableElastic(hostName, "warewolftestlogs",Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous,"","");
            }
            else
            {
                return new AuditQueryableSqlite();
            }
        }

        private IAuditQueryable GetAuditQueryableBasicAuthentication()
        {
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            return new AuditQueryableElastic(hostName, "warewolftestlogs", Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password, "user", "password");
        }

        private IAuditQueryable GetAuditQueryable()
        {
            return new AuditQueryableElastic();
        }

        private void LoadLogsintoElastic(Guid executionId, Guid resourceId, string auditType, string detail, string eventLevel)
        {
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var port = dependency.Container.Port;
            var loggerSource = new SerilogElasticsearchSource
            {
                Port = port,
                HostName = hostName,
                SearchIndex = "warewolftestlogs"
            };
            var uri = new Uri(hostName + ":" + port);
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(new ElasticsearchSink(new ElasticsearchSinkOptions(uri)
                {
                    AutoRegisterTemplate = true,
                    IndexDecider = (e, o) => loggerSource.SearchIndex,
                }))
                .CreateLogger();

            var mockSeriLogConfig = new Mock<ISeriLogConfig>();
            mockSeriLogConfig.SetupGet(o => o.Logger).Returns(logger);

            using (var loggerConnection = loggerSource.NewConnection(mockSeriLogConfig.Object))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                var mockDataObject = SetupDataObjectWithAssignedInputs(executionId, resourceId);
                var auditLog = new Audit(mockDataObject.Object, auditType, detail, null, null);
                //-------------------------Act----------------------------------
                switch (eventLevel)
                {
                    case "Debug":
                        loggerPublisher.Debug(GlobalConstants.WarewolfLogsTemplate, auditLog);
                        break;
                    case "Warning":
                        loggerPublisher.Warn(GlobalConstants.WarewolfLogsTemplate, auditLog);
                        break;
                    case "Fatal":
                        loggerPublisher.Fatal(GlobalConstants.WarewolfLogsTemplate, auditLog);
                        break;
                    default:
                        loggerPublisher.Info(GlobalConstants.WarewolfLogsTemplate, auditLog);
                        break;
                }
            }
            Task.Delay(225).Wait();
        }

        private void LoadExecutionHistoryintoElastic(Guid executionId, Guid resourceId, string auditType, string detail, string eventLevel)
        {
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var port = dependency.Container.Port;
            var loggerSource = new SerilogElasticsearchSource
            {
                Port = port,
                HostName = hostName,
                SearchIndex = "warewolftestlogs"
            };
            var uri = new Uri(hostName + ":" + port);
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(new ElasticsearchSink(new ElasticsearchSinkOptions(uri)
                {
                    AutoRegisterTemplate = true,
                    IndexDecider = (e, o) => loggerSource.SearchIndex,
                }))
                .CreateLogger();

            var mockSeriLogConfig = new Mock<ISeriLogConfig>();
            mockSeriLogConfig.SetupGet(o => o.Logger).Returns(logger);

            using (var loggerConnection = loggerSource.NewConnection(mockSeriLogConfig.Object))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                var executionInfo = new ExecutionInfo(DateTime.Now, DateTime.Now-DateTime.UtcNow, DateTime.Today, Triggers.QueueRunStatus.Success, executionId,executionId.ToString());
                var executionHistory = new ExecutionHistory(resourceId, "", executionInfo, "username");
                //-------------------------Act----------------------------------
                if (eventLevel == "Debug")
                {
                    loggerPublisher.Debug(GlobalConstants.WarewolfLogsTemplate, executionHistory);
                }
                else
                {
                    loggerPublisher.Info(GlobalConstants.WarewolfLogsTemplate, executionHistory);
                }
            }

            Task.Delay(225).Wait();
        }
        Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid executionId, Guid resourceId)
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Test-Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        [ExpectedException(typeof(Exception))]
        public void AuditQueryableElastic_Default_Constructor_Failed_InvalidSource()
        {
            var auditQueryable = GetAuditQueryable();
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_BasicAuthentication()
        {
            var auditQueryable = GetAuditQueryableBasicAuthentication();
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            var match_all = new JObject
            {
                ["match_all"] = new JObject()
            };

            Assert.AreEqual(match_all.ToString(), auditQueryable.Query);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryTriggerData_FilterBy_ResourceId()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadExecutionHistoryintoElastic(executionId, resourceId, "QueryTriggerData", "details", "Info");
            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var query = new Dictionary<string, StringBuilder>
            {
                {"ResourceId", resourceId.ToString().ToStringBuilder()}
            };
            var result = auditQueryable.QueryTriggerData(query);
            var jsonQuery = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.ResourceId"] = resourceId
                }
            };
            Assert.AreEqual(jsonQuery.ToString(), auditQueryable.Query);
            Assert.AreEqual(1,result.Count());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_NoParameters()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Info");
            //
            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            var match_all = new JObject
            {
                ["match_all"] = new JObject()
            };

            Assert.AreEqual(match_all.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_ExecutionId_EventLevel_Debug()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Debug");
            //

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionId.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()}
            };
            var jsonQueryexecutionId = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.ExecutionID"] = executionId.ToString()
                }
            };
            var jsonLevel = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Debug"
                }
            };
            var jArray = new JArray();
            jArray.Add(jsonQueryexecutionId);
            jArray.Add(jsonLevel);

            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);

            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_IncorrectLevel()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Wrong");
            //
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Wrong".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var levelObject = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Wrong"
                }
            };
            jArray.Add(levelObject);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_ExecutionId()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Info");
            //

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionId.ToString().ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.ExecutionID"] = executionId.ToString()
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Debug()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Debug");
            //

            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Debug".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json =
                new JObject
                {
                    ["match"] = new JObject
                    {
                        ["level"] = "Debug"
                    }
                };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Information()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Information");
            //
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Information".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Information"
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Warning()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Warning");
            //
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Warning".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Warning"
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Error()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Error");
            //
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Error".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query).ToList();
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Error"
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsFalse(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Fatal()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Fatal");
            //
            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Fatal".ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Fatal"
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Info");
            //
            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now.AddDays(-1);
            var CompletedDateTime = DateTime.Now.AddMinutes(10);

            var query = new Dictionary<string, StringBuilder>
            {
                {"StartDateTime", StartDateTime.ToString(dtFormat).ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString(dtFormat).ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var dateObj = new JObject()
            {
                ["gt"] = StartDateTime.ToString(dtFormat),
                ["lt"] = CompletedDateTime.ToString(dtFormat)
            };
            var jsonQueryDateRangeFilter = new JObject
            {
                ["range"] = new JObject
                {
                    ["@timestamp"] = dateObj
                }
            };
            jArray.Add(jsonQueryDateRangeFilter);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_DateTime_EventLevel()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Debug");
            //
            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now.AddDays(-1);
            var CompletedDateTime = DateTime.Now.AddMinutes(10);

            var query = new Dictionary<string, StringBuilder>
            {
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToString(dtFormat).ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString(dtFormat).ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var dateObj = new JObject()
            {
                ["gt"] = StartDateTime.ToString(dtFormat),
                ["lt"] = CompletedDateTime.ToString(dtFormat)
            };
            var json = new JObject
            {
                ["range"] = new JObject
                {
                    ["@timestamp"] = dateObj
                }
            };
            jArray.Add(json);
            var jsonMatch = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Debug"
                }
            };
            jArray.Add(jsonMatch);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime_EventLevel_executionID()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Debug");

            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now.AddDays(-1);
            var CompletedDateTime = DateTime.Now.AddMinutes(10);

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionId.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToString(dtFormat).ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToString(dtFormat).ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var dateObj = new JObject()
            {
                ["gt"] = StartDateTime.ToString(dtFormat),
                ["lt"] = CompletedDateTime.ToString(dtFormat)
            };
            var jsonExecutionId = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.ExecutionID"] = executionId.ToString()
                }
            };

            var jsonLevel = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Debug"
                }
            };

            var jsonDate = new JObject
            {
                ["range"] = new JObject
                {
                    ["@timestamp"] = dateObj
                }
            };
            jArray.Add(jsonDate);
            jArray.Add(jsonExecutionId);
            jArray.Add(jsonLevel);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);

            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime_On_UrlEncoded_DateTime_EventLevel_and_executionID_Should_Not_Break()
        {
            //setup
            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            LoadLogsintoElastic(executionId, resourceId, "LogAdditionalDetail", "details", "Debug");
            //
            var StartDateTime = "2020%2F01%2F01+01%3A40%3A18";
            var CompletedDateTime = "2028%2F10%2F03+01%3A40%3A18";

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionId.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToStringBuilder()}
            };

            var audit = GetAuditQueryable("AuditingSettingsData");
            var results = audit.QueryLogData(query);
            var jArray = new JArray();
            var dateObj = new JObject()
            {
                ["gt"] = "2020-01-01T01:40:18",
                ["lt"] = "2028-10-03T01:40:18"
            };
            var jsonExecutionId = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.ExecutionID"] = executionId.ToString()
                }
            };

            var jsonLevel = new JObject
            {
                ["match"] = new JObject
                {
                    ["level"] = "Debug"
                }
            };

            var jsonDate = new JObject
            {
                ["range"] = new JObject
                {
                    ["@timestamp"] = dateObj
                }
            };
            jArray.Add(jsonDate);
            jArray.Add(jsonExecutionId);
            jArray.Add(jsonLevel);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), audit.Query);
            Assert.IsTrue(results.Any());
        }
    }
}