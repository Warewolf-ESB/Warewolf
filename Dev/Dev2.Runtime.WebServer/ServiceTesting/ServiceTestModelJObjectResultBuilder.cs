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
using Dev2.Common;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestModelJObjectResultBuilder
    {
        public static void SetupResultSummaryJSON(this List<IServiceTestModelTO> serviceTestModelTOs, JsonTextWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("TestsTotalCount");
            writer.WriteValueAsync(serviceTestModelTOs.Count);

            writer.WritePropertyName("TestsFailed");
            writer.WriteValueAsync(serviceTestModelTOs.Count(o => o.TestFailing));
            writer.WritePropertyName("TestsPassed");
            writer.WriteValueAsync(serviceTestModelTOs.Count(o => o.TestPassed));
            writer.WritePropertyName("TestsInvalid");
            writer.WriteValueAsync(serviceTestModelTOs.Count(o => o.TestInvalid));

            writer.WriteEndObject();
        }


        public static void SetupCoverageNodesSummaryJSON(this List<IServiceTestModelTO> serviceTestModelTOs, JsonTextWriter writer)
        {
            writer.WriteStartObject();        


            writer.WritePropertyName("Total Nodes");
            writer.WriteValueAsync(serviceTestModelTOs.Count);
            writer.WritePropertyName("Covered Nodes");
            writer.WriteValueAsync("2");
            writer.WritePropertyName("Not Covered Nodes");
            writer.WriteValueAsync("1");
            writer.WritePropertyName("Coverage (%)");
            writer.WriteValueAsync("50%");

            writer.WriteEndObject();
        }

        public static JObject BuildTestResultJSONForWebRequest(this IServiceTestModelTO result)
        {
            var resObj = new JObject { { "Test Name", result.TestName } };
            if (result.Result.RunTestResult == RunResult.TestPassed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PassedResult);
            }
            else if (result.Result.RunTestResult == RunResult.TestFailed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_FailureResult);
                var message = result.Result.Message ?? result.FailureMessage;
                resObj.Add("Message", message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestInvalid)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_InvalidResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourceDeleted)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourceDeleteResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourcePathUpdated)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourcpathUpdatedResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else
            {
                if (result.Result.RunTestResult == RunResult.TestPending)
                {
                    resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PendingResult);
                    resObj.Add("Message", result.Result.Message);
                }
            }
            return resObj;
        }


        public static JObject BuildTestResultJSONForWebRequest(this IServiceTestCoverageModelTo report)
        {
            var totalWorkflowNodesCount = report.AllWorkflowNodes;
            int notCoveredNodesCount = report.NotCoveredNodesCount;
            var nodesCoveredCount = report.AllTestNodesCovered.SelectMany(o => o.TestNodesCovered).Distinct().Flatten(o => o.ChildNodes);

            var assertedNodes = nodesCoveredCount.Where(oo => oo.MockSelected == false);
            var mockedNodes = nodesCoveredCount.Where(oo => oo.MockSelected == true);
            var resObj = new JObject
            {
                { "Report Name", report.ReportName},
                { "OldReportName", report.OldReportName},
                { "WorkflowId", report.WorkflowId},
                { "CoveragePercentage", Math.Round(report.TotalCoverage * 100)},
                { "NodesSummary", new JObject
                    {
                        { "TotalNodesCount", totalWorkflowNodesCount.Count() },
                        { "NotCoveredNodes", notCoveredNodesCount },
                        { "CoveredNodes",  nodesCoveredCount.Count() },
                        { "CoveredNodesDetails", new JArray
                        (
                            new JObject 
                            {
                                { "Assert", assertedNodes.Count() },
                                { "Mocked", mockedNodes.Count() }
                            }) 
                        } 
                    } 
                },
                {
                    "AllTestNodesCovered",
                    new JArray(report.AllTestNodesCovered.Select(node => node.BuildTestResultJSONForWebRequest()))
                }
            };

            return resObj;
        }

        private static JObject BuildTestResultJSONForWebRequest(this ISingleTestNodesCovered report)
        {
            var resObj = new JObject { { "TestNodesCovered", new JArray(report.TestNodesCovered.Select(o => o.BuildTestResultJSONForWebRequest())) } };

            return resObj;
        }

        private static JObject BuildTestResultJSONForWebRequest(this IWorkflowNode report)
        {
            var resObj = new JObject
            {
                {"Node Name", report.StepDescription},
                {"ActivityID", report.ActivityID},
                {"UniqueID", report.UniqueID},
                {"MockSelected", report.MockSelected},
                {"ChildNodes", new JArray{  report.ChildNodes.Select(o => o.BuildTestResultJSONForWebRequest()) } } 
            };

            return resObj;
        }
    }
}
