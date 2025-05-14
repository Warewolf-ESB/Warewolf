/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text.Json;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;
using LogLevel = Warewolf.Logging.LogLevel;


namespace Warewolf.Auditing.Drivers
{
    public class AuditQueryableElastic : AuditQueryable, IDisposable
    {
        private readonly IElasticsearchSource _elasticsearchSource;
        private readonly ElasticsearchClient _elasticClient;

        public AuditQueryableElastic(IElasticsearchSource elasticsearchSource, ElasticsearchClient elasticClient)
        {
            _elasticsearchSource = elasticsearchSource;
            _elasticClient = elasticClient;
        }

        public AuditQueryableElastic(string hostname, string port, string searchIndex,
            AuthenticationType authenticationType, string username, string password)
            : this(new ElasticsearchSource(), null)
        {
            _elasticsearchSource.HostName = hostname;
            _elasticsearchSource.Port = port;
            _elasticsearchSource.SearchIndex = searchIndex;
            _elasticsearchSource.Username = username;
            _elasticsearchSource.Password = password;
            _elasticsearchSource.AuthenticationType = authenticationType;
        }

        public override IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            try
            {
                var resourceId = GetValue<string>("ResourceId", values);
                var result = new List<ExecutionHistory>();

                if (resourceId != null)
                {
                    var search = new SearchRequestDescriptor<object>()
                    .Size(20)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Term(t => t
                                    .Field("fields.Data.WorkflowID.keyword")
                                    .Value(resourceId)
                                )
                            )
                        )
                    );

                    var results = ExecuteDatabase(search)?.ToList();
                    if (results?.Count > 0)
                    {
                        var queryTriggerData = ExecutionHistories(results, result);
                        if (queryTriggerData != null)
                        {
                            return queryTriggerData;
                        }
                    }
                }

                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in querying trigger data: "+ ex.ToString());
                return new List<ExecutionHistory>();
            }
        }

        private static IEnumerable<IExecutionHistory> ExecutionHistories(IEnumerable<object> results, ICollection<ExecutionHistory> result)
        {
            foreach (JsonElement jsonElement in results)
            {
                if (jsonElement.TryGetProperty("fields", out JsonElement fieldsElement) &&
                    fieldsElement.TryGetProperty("Data", out JsonElement dataElement))
                {
                    var executionHistory = new ExecutionHistory();

                    foreach (JsonProperty property in dataElement.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Null)
                            continue;

                        switch (property.Name)
                        {
                            case "WorkflowID":
                                executionHistory.ResourceId = Guid.Parse(property.Value.GetString());
                                break;
                            case "ExecutionID":
                                var jObject = fieldsElement.GetProperty("Data");

                                var hasStart = DateTime.TryParse(jObject.GetProperty("StartDateTime").ToString(), out var start);
                                var hasEnd = DateTime.TryParse(jObject.GetProperty("CompletedDateTime").ToString(), out var end);

                                var execInfo = new ExecutionInfo
                                {
                                    CustomTransactionID = jObject.GetProperty("CustomTransactionID").ToString(),
                                    StartDate = hasStart ? start : default,
                                    EndDate = hasEnd ? end : default,
                                    Duration = hasStart && hasEnd ? end - start : TimeSpan.Zero,
                                    ExecutionId = Guid.TryParse(jObject.GetProperty("ExecutionID").ToString(), out var eid) ? eid : Guid.Empty,
                                    FailureReason = jObject.TryGetProperty("Exception", out var exProp) && exProp.ValueKind != JsonValueKind.Null ? exProp.ToString() : null,
                                    Success = jObject.TryGetProperty("Exception", out var exProp1) && exProp1.ValueKind != JsonValueKind.Null
                                        ? QueueRunStatus.Success
                                        : QueueRunStatus.Error
                                };

                                executionHistory.ExecutionInfo = execInfo;
                                break;
                            case "ExecutionInfo":
                                var executionInfo = ExecutionInfo(property); // Ensure ExecutionInfo() accepts JsonProperty or JsonElement
                                executionHistory.ExecutionInfo = executionInfo;
                                break;
                            case "User":
                                executionHistory.UserName = property.Value.GetString();
                                break;
                            case "Exception":
                                // Assuming it's a string or null — update if needed
                                executionHistory.Exception = null; // Deserialize appropriately if needed
                                break;
                            case "LogLevel":
                                Enum.TryParse(property.Value.GetString(), true, out LogLevel logLevel);
                                executionHistory.LogLevel = logLevel;
                                break;
                            case "AuditType":
                                executionHistory.AuditType = property.Value.GetString();
                                break;
                        }
                    }

                    result.Add(executionHistory);
                }
            }

            return result;
        }

        private static ExecutionInfo ExecutionInfo(JsonProperty item)
        {
            var executionInfo = new ExecutionInfo();

            if (item.Value.ValueKind != JsonValueKind.Object)
                return executionInfo;

            foreach (var prop in item.Value.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Null)
                    continue;

                switch (prop.Name)
                {
                    case "ExecutionId":
                        executionInfo.ExecutionId = Guid.Parse(prop.Value.GetString());
                        break;

                    case "CustomTransactionID":
                        executionInfo.CustomTransactionID = prop.Value.GetString();
                        break;

                    case "Success":
                        // You'll likely need custom logic here if you're deserializing an enum
                        if (Enum.TryParse<QueueRunStatus>(prop.Value.GetString(), true, out var status))
                        {
                            executionInfo.Success = status;
                        }
                        else
                        {
                            executionInfo.Success = QueueRunStatus.Success;
                        }
                        break;

                    case "EndDate":
                        executionInfo.EndDate = prop.Value.GetDateTime();
                        break;

                    case "Duration":
                        executionInfo.Duration = TimeSpan.Parse(prop.Value.GetString());
                        break;

                    case "StartDate":
                        executionInfo.StartDate = prop.Value.GetDateTime();
                        break;

                    case "FailureReason":
                        executionInfo.FailureReason = prop.Value.GetString();
                        break;
                }
            }

            return executionInfo;
        }


        public override IEnumerable<IAudit> QueryLogData(Dictionary<string, StringBuilder> values)
        {
            var result = new List<Audit>();
            var searchDescriptor = BuildDescriptorQuery(values);
            var results = ExecuteDatabase(searchDescriptor)?.ToList();
            if (!(results?.Count > 0)) return result;
            var queryLogData = AuditLogs(results, result);
            return queryLogData ?? result;
        }

        private SearchRequestDescriptor<object> BuildDescriptorQuery(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionId = GetValue<string>("ExecutionID", values);

            const string dtFormat = "yyyy-MM-ddTHH:mm:ss";

            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
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

            var search = new SearchRequestDescriptor<object>().Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Range(r => r.DateRange(dr => dr
                            .Field("@timestamp")
                            .Gt(startTime)
                            .Lt(endTime)
                        )),
                        m => m.Term(t => t.Field("fields.Data.ExecutionID.keyword").Value(executionId)),
                        m => m.Term(t => t.Field("fields.Data.LogLevel.keyword").Value(eventLevel))
                    )
                )
            );
            Query = search.SerializeToJsonString(new KnownTypesBinder
            {
                KnownTypes = new List<Type> { typeof(object) }
            });
            return search;
        }

        private static IEnumerable<IAudit> AuditLogs(IEnumerable<object> results, ICollection<Audit> result)
        {
            try
            {
                foreach (Dictionary<string, object> item in results)
                {
                    foreach (var entry in item)
                    {
                        if (entry.Key != "fields")
                        {
                            continue;
                        }
                        foreach (var fields in (Dictionary<string, object>) entry.Value)
                        {
                            var auditHistory = new Audit();
                            var keyValuePairs = ((Dictionary<string, object>) fields.Value)
                                .Where(items => items.Value != null);

                            foreach (var items in keyValuePairs)
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
                                        auditHistory.Environment = string.Empty;
                                        break;
                                    case "AuditDate":
                                        auditHistory.AuditDate = DateTime.Parse(items.Value.ToString());
                                        break;
                                    case "Exception":
                                        auditHistory.Exception = items.Value as SerializableException;
                                        break;
                                    case "AuditType":
                                        auditHistory.AuditType = items.Value.ToString();
                                        break;
                                    case "LogLevel":
                                        Enum.TryParse((string) items.Value, true, out LogLevel logLevel);
                                        auditHistory.LogLevel = logLevel;
                                        break;
                                    case "IsSubExecution":
                                        auditHistory.IsSubExecution = bool.Parse(items.Value.ToString());
                                        break;
                                    case "IsRemoteWorkflow":
                                        auditHistory.IsRemoteWorkflow = bool.Parse(items.Value.ToString());
                                        break;
                                    case "ServerID":
                                        auditHistory.ServerID = items.Value.ToString();
                                        break;
                                    case "ParentID":
                                        auditHistory.ParentID = items.Value.ToString();
                                        break;
                                }
                            }

                            result.Add(auditHistory);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Invalid Data Returned");
            }

            return result;
        }

        private IEnumerable<object> ExecuteDatabase(SearchRequestDescriptor<object> search)
        {
            var client = _elasticClient ?? Client();
            var logEvents = client.Search(search);
            if (!logEvents.IsValidResponse)
            {
                Console.WriteLine("Elasticsearch query failed");
                return Enumerable.Empty<object>();
            }

            var sources = logEvents.HitsMetadata?.Hits?.Select(h => h.Source);
            return sources ?? Enumerable.Empty<object>();
        }

        private ElasticsearchClient Client()
        {
            var uri = new Uri(_elasticsearchSource.HostName + ":" + _elasticsearchSource.Port);
            var settings = new ElasticsearchClientSettings(uri)
                .RequestTimeout(TimeSpan.FromMinutes(2))
                .DefaultIndex(_elasticsearchSource.SearchIndex)
                .DisableDirectStreaming();
            if (_elasticsearchSource.AuthenticationType == AuthenticationType.Password)
            {
                var basicAuth = new BasicAuthentication(_elasticsearchSource.Username, _elasticsearchSource.Password);
                settings.Authentication(basicAuth);
            }

            var elasticClient = new ElasticsearchClient(settings);
            var result = elasticClient.Ping();
            var isValid = result.IsValidResponse;
            if (!isValid && result.TryGetOriginalException(out Exception e))
            {
                throw e;
            }
            return elasticClient;
        }

        public void Dispose()
        {
            _elasticsearchSource.Dispose();
        }
    }
}