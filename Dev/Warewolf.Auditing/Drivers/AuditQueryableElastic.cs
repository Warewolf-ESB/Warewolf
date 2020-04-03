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
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Warewolf.Auditing.Drivers
{
    public class AuditQueryableElastic : AuditQueryable
    {
        public override IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values)
        {
            var resourceId = GetValue<string>("ResourceId", values);
            var result = new List<ExecutionHistory>();
            var query = @"{""match"":{""fields.Data.ResourceId"":""9e556ceb-ce2d-4aaa-a556-5d6e80261c96""}}";
            if (resourceId != null)
            {
                var results = ExecuteDatabase(query).ToList();
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
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var eventLevel = GetValue<string>("EventLevel", values);
            var executionID = GetValue<string>("ExecutionID", values);

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

            var query = @"{""match"":{""fields.Data.Audit.ExecutionID"":""920df540-5c48-4600-9a17-87a8214cde8c""}}";
            var results = ExecuteDatabase(query);
            if (results.Count() > 0)
            {
                /*  foreach (var result in results)
                  {
                      var audit = JsonConvert.DeserializeObject<Audit>(result);
                      if (audit.ExecutionID != null)
                      {
                          yield return audit;
                      }
                  }*/
                yield return null;
            }
            else
            {
                yield return null;
            }
        }

        private IEnumerable<object> ExecuteDatabase(string query)
        {
            var src = new ElasticsearchSource();
            var uri = new Uri(src.HostName + ":" + src.Port);
            var settings = new ConnectionSettings(uri)
                .RequestTimeout(TimeSpan.FromMinutes(2))
                .DefaultIndex(src.SearchIndex);
            if (src.AuthenticationType == AuthenticationType.Password)
            {
                settings.BasicAuthentication(src.Username, src.Password);
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
                        q.Raw(query));
                var logEvents = client.Search<object>(search);
                var sources = logEvents.HitsMetadata.Hits.Select(h => h.Source);
                return sources;
            }
        }
    }
}