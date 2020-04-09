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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Warewolf.Auditing.Drivers
{
    public class AuditQueryableElastic : AuditQueryable
    {
        private string _query;
        private readonly ElasticsearchSource _elasticsearchSource;

        public override string Query
        {
            get => _query;
            set => _query = value;
        }

        public AuditQueryableElastic()
        {
            _elasticsearchSource = new ElasticsearchSource();
        }
        public AuditQueryableElastic(string hostname)
        {
            _elasticsearchSource = new ElasticsearchSource();
            _elasticsearchSource.HostName = hostname;
        }
        public override IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);
            var result = new List<ExecutionHistory>();
            if (resourceId != null)
            {
                var jsonQuery = new JObject
                {
                    ["match"] = new JObject
                    {
                        ["fields.Data.ResourceId"] = resourceId
                    }
                };

                _query = jsonQuery.ToString();
                var results = ExecuteDatabase().ToList();
                if (results.Count > 0)
                {
                    var queryTriggerData = ExecutionHistories(results, result);
                    if (queryTriggerData != null) return queryTriggerData;
                }
            }

            return result;
        }

        private static IEnumerable<IExecutionHistory> ExecutionHistories(List<object> results, List<ExecutionHistory> result)
        {
            foreach (Dictionary<string, object> item in results)
            {
                foreach (var entry in item)
                {
                    if (entry.Key == "fields")
                    {
                        foreach (var fields in (Dictionary<string, object>) entry.Value)
                        {
                            var executionHistory = new ExecutionHistory();
                            foreach (var items in (Dictionary<string, object>) fields.Value)
                            {
                                if (items.Value != null)
                                {
                                    switch (items.Key)
                                    {
                                        case "ResourceId":
                                            executionHistory.ResourceId = Guid.Parse(items.Value.ToString());
                                            break;
                                        case "ExecutionInfo":
                                            var executionInfo = ExecutionInfo(items);
                                            executionHistory.ExecutionInfo = executionInfo;
                                            break;
                                        case "UserName":
                                            executionHistory.UserName = items.Value.ToString();
                                            break;
                                        case "Exception":
                                            executionHistory.Exception = items.Value as Exception;
                                            break;
                                        case "AuditType":
                                            executionHistory.AuditType = items.Value.ToString();
                                            break;
                                    }
                                }
                            }

                            result.Add(executionHistory);
                        }
                    }
                }
            }

            return result;
        }

        private static ExecutionInfo ExecutionInfo(KeyValuePair<string, object> items)
        {
            var executionInfo = new ExecutionInfo();
            foreach (var infoItem in (Dictionary<string, object>) items.Value)
            {
                if (infoItem.Value != null)
                {
                    switch (infoItem.Key)
                    {
                        case "ExecutionId":
                            executionInfo.ExecutionId = Guid.Parse(infoItem.Value.ToString());
                            break;
                        case "CustomTransactionID":
                            executionInfo.CustomTransactionID = infoItem.Value.ToString();
                            break;
                        case "Success":
                            executionInfo.Success = items.Value is QueueRunStatus ? (QueueRunStatus) items.Value : QueueRunStatus.Success;
                            break;
                        case "EndDate":
                            executionInfo.EndDate = DateTime.Parse(infoItem.Value.ToString());
                            break;
                        case "Duration":
                            executionInfo.Duration = TimeSpan.Parse(infoItem.Value.ToString());
                            break;
                        case "StartDate":
                            executionInfo.StartDate = DateTime.Parse(infoItem.Value.ToString());
                            break;
                        case "FailureReason":
                            executionInfo.FailureReason = infoItem.Value.ToString();
                            break;
                    }
                }
            }

            return executionInfo;
        }

        public override IEnumerable<IAudit> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var result = new List<Audit>();
            BuildJsonQuery(values);
            var results = ExecuteDatabase().ToList();
            if (results.Count > 0)
            {
                var queryLogData = AuditLogs(results, result);
                if (queryLogData != null) return queryLogData;
            }

            return result;
        }

        private void BuildJsonQuery(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionId = GetValue<string>("ExecutionID", values);

            var dtFormat = "yyyy-MM-ddTHH:mm:ss";

            if (!string.IsNullOrEmpty(startTime) & !string.IsNullOrEmpty(endTime))
            {
                if (IsUrlEncoded(startTime) & IsUrlEncoded(endTime))
                {
                    var decodedStartTime = Convert.ToDateTime(GetDecoded(startTime));
                    var decodedEndTime = Convert.ToDateTime(GetDecoded(endTime));

                    startTime = decodedStartTime.ToString(dtFormat);
                    endTime = decodedEndTime.ToString(dtFormat);
                }
                else
                {
                    startTime = Convert.ToDateTime(startTime).ToString(dtFormat);
                    endTime = Convert.ToDateTime(endTime).ToString(dtFormat);
                }
            }

            var jArray = new JArray();
            var jsonQueryexecutionId = new JObject();
            var jsonQueryexecutionLevel = new JObject();
            var jsonQueryDateRangeFilter = new JObject();

            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                var dateObj = new JObject()
                {
                    ["gt"] = startTime,
                    ["lt"] = endTime
                };
                jsonQueryDateRangeFilter = new JObject
                {
                    ["range"] = new JObject
                    {
                        ["@timestamp"] = dateObj
                    }
                };
                jArray.Add(jsonQueryDateRangeFilter);
            }

            if (!string.IsNullOrEmpty(executionId))
            {
                jsonQueryexecutionId = new JObject
                {
                    ["match"] = new JObject
                    {
                        ["fields.Data.Audit.ExecutionID"] = executionId
                    }
                };
                jArray.Add(jsonQueryexecutionId);
            }

            if (!string.IsNullOrEmpty(eventLevel))
            {
                jsonQueryexecutionLevel = new JObject
                {
                    ["match"] = new JObject
                    {
                        ["level"] = eventLevel
                    }
                };
                jArray.Add(jsonQueryexecutionLevel);
            }

            if (jArray.Count == 0)
            {
                var match_all = new JObject
                {
                    ["match_all"] = new JObject()
                };
                _query = match_all.ToString();
            }
            else
            {
                var objMust = new JObject();
                objMust.Add("must", jArray);

                var obj = new JObject();
                obj.Add("bool", objMust);
                _query = obj.ToString();
            }
        }

        private static IEnumerable<IAudit> AuditLogs(List<object> results, List<Audit> result)
        {
            try
            {
                foreach (Dictionary<string, object> item in results)
                {
                    foreach (var entry in item)
                    {
                        if (entry.Key == "fields")
                        {
                            foreach (var fields in (Dictionary<string, object>) entry.Value)
                            {
                                var auditHistory = new Audit();
                                foreach (var items in (Dictionary<string, object>) fields.Value)
                                {
                                    if (items.Value != null)
                                    {
                                        switch (items.Key)
                                        {
                                            case "ExecutionID":
                                                auditHistory.ExecutionID = items.Value.ToString();
                                                break;
                                            case "CustomTransactionID":
                                                auditHistory.CustomTransactionID = items.Value.ToString();
                                                break;
                                            case "WorkflowName":
                                                auditHistory.WorkflowName = items.Value.ToString();
                                                break;
                                            case "ExecutingUser":
                                                auditHistory.ExecutingUser = items.Value.ToString();
                                                break;
                                            case "Url":
                                                auditHistory.Url = items.Value.ToString();
                                                break;
                                            case "Environment":
                                                auditHistory.Environment = items.Value.ToString();
                                                break;
                                            case "AuditDate":
                                                auditHistory.AuditDate = DateTime.Parse(items.Value.ToString());
                                                break;
                                            case "Exception":
                                                auditHistory.Exception = items.Value as Exception;
                                                break;
                                            case "AuditType":
                                                auditHistory.AuditType = items.Value.ToString();
                                                break;
                                            case "IsSubExecution":
                                                auditHistory.IsSubExecution = Boolean.Parse(items.Value.ToString());
                                                break;
                                            case "IsRemoteWorkflow":
                                                auditHistory.IsRemoteWorkflow = Boolean.Parse(items.Value.ToString());
                                                break;
                                            case "ServerID":
                                                auditHistory.ServerID = items.Value.ToString();
                                                break;
                                            case "ParentID":
                                                auditHistory.ParentID = items.Value.ToString();
                                                break;
                                        }
                                    }
                                }

                                result.Add(auditHistory);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private IEnumerable<object> ExecuteDatabase()
        {
            var uri = new Uri(_elasticsearchSource.HostName + ":" + _elasticsearchSource.Port);
            var settings = new ConnectionSettings(uri)
                .RequestTimeout(TimeSpan.FromMinutes(2))
                .DefaultIndex(_elasticsearchSource.SearchIndex);
            if (_elasticsearchSource.AuthenticationType == AuthenticationType.Password)
            {
                settings.BasicAuthentication(_elasticsearchSource.Username, _elasticsearchSource.Password);
            }

            var client = new ElasticClient(settings);
            var result = client.Ping();
            var isValid = result.IsValid;
            if (!isValid)
            {
                throw new Exception("Invalid Data Source");
            }
            else
            {
                var search = new SearchDescriptor<object>()
                    .Query(q =>
                        q.Raw(_query));
                var logEvents = client.Search<object>(search);
                var sources = logEvents.HitsMetadata.Hits.Select(h => h.Source);
                return sources;
            }
        }
        
    }
}