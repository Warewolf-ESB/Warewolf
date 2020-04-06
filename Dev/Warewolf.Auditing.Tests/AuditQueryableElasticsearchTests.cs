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
using System.Text;
using Newtonsoft.Json.Linq;
using Warewolf.Auditing.Drivers;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class AuditQueryableElasticTests
    {
        private IAuditQueryable GetAuditQueryable(string sink)
        {
            if (sink == "AuditingSettingsData")
            {
                return new AuditQueryableElastic();
            }
            else
            {
                return new AuditQueryableSqlite();
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryTriggerData_FilterBy_ResourceId()
        {
            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var resourceId = Guid.Parse("9e556ceb-ce2d-4aaa-a556-5d6e80261c96"); // Guid.NewGuid();
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_NoParameters()
        {
            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var query = new Dictionary<string, StringBuilder>();

            var results = auditQueryable.QueryLogData(query);
            var match_all = new JObject
            {
                ["match_all"] = new JObject()
            };
            var jArray = new JArray();
            jArray.Add(match_all);

            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_ExecutionId_EventLevel()
        {
            var executionID = Guid.NewGuid();

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()}
            };
            var jsonQueryexecutionId = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.Audit.ExecutionID"] = executionID.ToString()
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_IncorrectLevel()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_ExecutionId()
        {
            var executionID = Guid.NewGuid();

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()}
            };

            var auditQueryable = GetAuditQueryable("AuditingSettingsData");
            var results = auditQueryable.QueryLogData(query);
            var jArray = new JArray();
            var json = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.Audit.ExecutionID"] = executionID.ToString()
                }
            };
            jArray.Add(json);
            var objMust = new JObject();
            objMust.Add("must", jArray);

            var obj = new JObject();
            obj.Add("bool", objMust);
            Assert.AreEqual(obj.ToString(), auditQueryable.Query);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Debug()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Information()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Warning()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Error()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_EventLevel_Fatal()
        {
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime()
        {
            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now.AddDays(-5);
            var CompletedDateTime = DateTime.Now;
         
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_DateTime_EventLevel()
        {
            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now;
            var CompletedDateTime = StartDateTime.AddMinutes(30);

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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime_EventLevel_executionID()
        {
            var executionID = Guid.NewGuid();
            var dtFormat = "yyyy-MM-ddTHH:mm:ss";
            var StartDateTime = DateTime.Now.AddDays(-5);
            var CompletedDateTime = StartDateTime.AddMinutes(30);

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
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
                    ["fields.Data.Audit.ExecutionID"] = executionID.ToString()
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData_FilterBy_DateTime_On_UrlEncoded_DateTime_EventLevel_and_executionID_Should_Not_Break()
        {
            var executionID = Guid.NewGuid();
            var StartDateTime = "2019%2F10%2F01+01%3A40%3A18";
            var CompletedDateTime = "2019%2F10%2F03+01%3A40%3A18";

            var query = new Dictionary<string, StringBuilder>
            {
                {"ExecutionID", executionID.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"StartDateTime", StartDateTime.ToStringBuilder()},
                {"CompletedDateTime", CompletedDateTime.ToStringBuilder()}
            };

            var audit = GetAuditQueryable("AuditingSettingsData");
            var results = audit.QueryLogData(query);
            var jArray = new JArray();
            var dateObj = new JObject()
            {
                ["gt"] = "2019-10-01T01:40:18",
                ["lt"] = "2019-10-03T01:40:18"
            };
            var jsonExecutionId = new JObject
            {
                ["match"] = new JObject
                {
                    ["fields.Data.Audit.ExecutionID"] = executionID.ToString()
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
        }
    }
}