#pragma warning disable
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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Warewolf.Storage;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Services;
using Enum = System.Enum;
using System.IO;
using System.Web.UI;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.WebServer
{
    public static class DataObjectExtensions
    {
        public static string SetEmissionType(this IDSFDataObject dataObject, Uri uri, string serviceName, NameValueCollection headers)
        {
            var startLocation = serviceName.LastIndexOf(".", StringComparison.Ordinal);
            if (!string.IsNullOrEmpty(serviceName) && startLocation > 0)
            {
                dataObject.ReturnType = EmitionTypes.XML;

                var extension = serviceName.Substring(startLocation + 1);
                return SetReturnTypeForExtension(dataObject, startLocation, extension);
            }

            if (serviceName == "*")
            {
                var path = uri.AbsolutePath;
                if (path.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TEST;
                }

                if (path.EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.Cover;
                }

                if (path.EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.CoverJson;
                }

                if (path.EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TRX;
                }
            }

            dataObject.SetContentType(headers);
            return serviceName;
        }

        private static string SetReturnTypeForExtension(IDSFDataObject dataObject, int loc, string typeOf)
        {
            if (Enum.TryParse(typeOf.ToUpper(), out EmitionTypes myType))
            {
                dataObject.ReturnType = myType;
            }

            var originalServiceName = dataObject.OriginalServiceName;

            var serviceName = originalServiceName.Substring(0, loc);

            var isApi = typeOf.Equals("api", StringComparison.OrdinalIgnoreCase);
            var isTests = typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase);
            var isTrx = typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase);
            if (isTests || isTrx)
            {
                dataObject.IsServiceTestExecution = true;
                dataObject.TestName = "*";
                var idx = originalServiceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                if (idx > loc)
                {
                    var testName = originalServiceName.Substring(idx + 1).ToUpper();
                    if (!string.IsNullOrEmpty(testName))
                    {
                        dataObject.TestName = testName;
                    }
                }

                if (isTests)
                {
                    dataObject.ReturnType = EmitionTypes.TEST;
                }

                if (isTrx)
                {
                    dataObject.ReturnType = EmitionTypes.TRX;
                    serviceName = originalServiceName.Substring(0, originalServiceName.LastIndexOf(".", StringComparison.Ordinal));
                }
            }

            var isCover = typeOf.StartsWith("coverage", StringComparison.InvariantCultureIgnoreCase);
            var isCoverJson = typeOf.StartsWith("json", StringComparison.InvariantCultureIgnoreCase) && originalServiceName.EndsWith("coverage.json", StringComparison.InvariantCultureIgnoreCase);
            if (isCoverJson || isCover)
            {
                if (isCover)
                {
                    dataObject.ReturnType = EmitionTypes.Cover;
                }

                if (isCoverJson)
                {
                    dataObject.ReturnType = EmitionTypes.CoverJson;
                    serviceName = originalServiceName.Substring(0, originalServiceName.LastIndexOf(".coverage.json", StringComparison.Ordinal));
                }
            }

            if (isApi)
            {
                dataObject.ReturnType = EmitionTypes.SWAGGER;
            }

            dataObject.ServiceName = serviceName;
            return serviceName;
        }

        public static void SetContentType(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            if (headers != null)
            {
                var contentType = headers.Get("Content-Type");
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = headers.Get("ContentType");
                }

                if (!string.IsNullOrEmpty(contentType) && !dataObject.IsServiceTestExecution)
                {
                    if (contentType.ToLowerInvariant().Contains("json"))
                    {
                        dataObject.ReturnType = EmitionTypes.JSON;
                    }

                    if (contentType.ToLowerInvariant().Contains("xml"))
                    {
                        dataObject.ReturnType = EmitionTypes.XML;
                    }
                }
            }
            else
            {
                dataObject.ReturnType = EmitionTypes.XML;
            }
        }

        public static void SetHeaders(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            var customTransactionId = headers?.Get("Warewolf-Custom-Transaction-Id");
            if (!string.IsNullOrEmpty(customTransactionId))
            {
                dataObject.CustomTransactionID = customTransactionId;
            }

            var executionId = headers?.Get("Warewolf-Execution-Id");
            if (!string.IsNullOrEmpty(executionId))
            {
                dataObject.ExecutionID = Guid.Parse(executionId);
                return;
            }

            dataObject.ExecutionID = Guid.NewGuid();
        }

        public static void SetupForWebDebug(this IDSFDataObject dataObject, WebRequestTO webRequest)
        {
            var contains = webRequest?.Variables?.AllKeys.Contains("IsDebug");
            if (contains != null && contains.Value)
            {
                dataObject.IsDebug = true;
                dataObject.IsDebugFromWeb = true;
                dataObject.ClientID = Guid.NewGuid();
                dataObject.DebugSessionID = Guid.NewGuid();
            }
        }

        public static void SetupForRemoteInvoke(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            var isRemote = headers?.Get(HttpRequestHeader.Cookie.ToString());
            var remoteId = headers?.Get(HttpRequestHeader.From.ToString());

            if (isRemote == null || remoteId == null)
            {
                return;
            }

            if (isRemote.Equals(GlobalConstants.RemoteServerInvoke))
            {
                // we have a remote invoke ;)
                dataObject.RemoteInvoke = true;
            }

            if (isRemote.Equals(GlobalConstants.RemoteDebugServerInvoke))
            {
                // we have a remote invoke ;)
                dataObject.RemoteNonDebugInvoke = true;
            }

            dataObject.RemoteInvokerID = remoteId;
        }

        public static void SetTestResourceIds(this IDSFDataObject dataObject, IContextualResourceCatalog catalog, WebRequestTO webRequest, string serviceName, IWarewolfResource resource)
        {
            if (IsRunAllTestsRequest(dataObject.ReturnType, serviceName))
            {
                var pathOfAllResources = webRequest.GetPathForAllResources();
                dataObject.ResourceID = Guid.Empty;
                var path = pathOfAllResources;
                if (string.IsNullOrEmpty(pathOfAllResources))
                {
                    path = "/";
                }

                var resources = catalog.GetExecutableResources(path);
                dataObject.TestsResourceIds = resources.Select(p => p.ResourceID).ToArray();
            }
            else if (resource != null)
            {
                dataObject.TestsResourceIds = new[] { resource.ResourceID };
            }
        }


        public static void SetTestCoverageResourceIds(this ICoverageDataObject coverageData, IContextualResourceCatalog catalog, WebRequestTO webRequest, string serviceName, IWarewolfResource resource)
        {
            if (IsRunAllCoverageRequest(coverageData.ReturnType, serviceName))
            {
                var pathOfAllResources = webRequest.GetPathForAllResources();
                //dataObject.ResourceID = Guid.Empty;
                var path = pathOfAllResources;
                if (string.IsNullOrEmpty(pathOfAllResources))
                {
                    path = "/";
                }

                var resources = catalog.GetExecutableResources(path);
                coverageData.CoverageReportResourceIds = resources.Select(p => p.ResourceID).ToArray();
            }
            else if (resource != null)
            {
                coverageData.CoverageReportResourceIds = new[] { resource.ResourceID }; 
            }
        }

        public static void SetupForTestExecution(this IDSFDataObject dataObject, string serviceName, NameValueCollection headers)
        {
            if (IsRunAllTestsRequest(dataObject.ReturnType, serviceName) || IsRunAllCoverageRequest(dataObject.ReturnType, serviceName))
            {
                dataObject.IsServiceTestExecution = true;
                dataObject.TestName = "*";
            }
            else
            {
                dataObject.SetContentType(headers);
            }
        }

        private static bool IsRunAllTestsRequest(EmitionTypes returnType, string serviceName)
        {
            var isRunAllTests = !string.IsNullOrEmpty(serviceName);
            isRunAllTests &= serviceName == "*" || serviceName == ".tests" || serviceName == ".tests.trx";
            isRunAllTests &= returnType == EmitionTypes.TEST || returnType == EmitionTypes.TRX;
            return isRunAllTests;
        }

        private static bool IsRunAllCoverageRequest(EmitionTypes returnType, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return false;
            }
            var isRunAllCoverage = serviceName == "*" || serviceName == ".coverage" || serviceName == ".coverage.json";
            isRunAllCoverage |= serviceName.EndsWith("/.coverage");
            isRunAllCoverage &= returnType == EmitionTypes.Cover || returnType == EmitionTypes.CoverJson;
            return isRunAllCoverage;
        }

        public static void SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IWarewolfResource resource)
        {
            IWarewolfResource localResource = null;

            if (Guid.TryParse(serviceName, out Guid resourceID))
            {
                localResource = catalog.GetResource(dataObject.WorkspaceID, resourceID);
                if (localResource != null)
                {
                    MapServiceToDataObjects(dataObject, localResource);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dataObject.ServiceName))
                {
                    localResource = catalog.GetResource(dataObject.WorkspaceID, dataObject.ServiceName);
                    if (localResource != null)
                    {
                        dataObject.ResourceID = localResource.ResourceID;
                        dataObject.SourceResourceID = localResource.ResourceID;
                    }
                }
            }

            if (localResource == null)
            {
                var stringDynaResourceId = serviceName.Replace(".xml", "").Replace(".bite", "").Replace(".json", "");
                if (Guid.TryParse(stringDynaResourceId, out resourceID))
                {
                    localResource = catalog.GetResource(dataObject.WorkspaceID, resourceID);
                    if (localResource != null)
                    {
                        MapServiceToDataObjects(dataObject, localResource);
                    }
                }

                if (localResource == null)
                {
                    dataObject.Environment.AddError($"Service {serviceName} not found.");
                }
            }

            resource = localResource;
        }

        static void MapServiceToDataObjects(IDSFDataObject dataObject, IWarewolfResource localResource)
        {
            dataObject.ServiceName = localResource.ResourceName;
            dataObject.ResourceID = localResource.ResourceID;
            dataObject.SourceResourceID = localResource.ResourceID;
        }

        public static bool CanExecuteCurrentResource(this IDSFDataObject dataObject, IWarewolfResource resource, IAuthorizationService service)
        {
            if (resource is null)
            {
                return false;
            }

            var canExecute = true;
            if (service != null && dataObject.ReturnType != EmitionTypes.TRX)
            {
                var hasView = service.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.View, dataObject.ResourceID.ToString());
                var hasExecute = service.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, dataObject.ResourceID.ToString());
                canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource.ResourceType == "ReservedService");
            }

            return canExecute;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnJSON(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
            Dev2JsonSerializer serializer,
            IResourceCatalog catalog, ITestCatalog testCatalog,
            out string executePayload, ITestCoverageCatalog testCoverageCatalog)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog, testCoverageCatalog);
            var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");

            var objArray = testResults.Results
                .Where(o => o.HasTestResults)
                .Select(o =>
                {
                    var name = o.Resource.ResourceName;
                    if (o.Resource is IFilePathResource filePath)
                    {
                        name = filePath.Path;
                    }

                    return new JObject
                    {
                        {"ResourceID", o.Resource.ResourceID},
                        {"Name", name},
                        {"Tests", new JArray(o.Results.Select(o1 => o1.BuildTestResultJSONForWebRequest()))}
                    };
                });

            var obj = new JObject
            {
                {"StartTime", testResults.StartTime},
                {"EndTime", testResults.EndTime},
                {"Results", new JArray(objArray)},
            };

            executePayload = serializer.Serialize(obj);
            return formatter;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnTRX(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
            Dev2JsonSerializer serializer,
            IResourceCatalog catalog, ITestCatalog testCatalog,
            out string executePayload, ITestCoverageCatalog testCoverageCatalog)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog, testCoverageCatalog);
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(dataObject.ServiceName, testResults.Results.SelectMany(o => o.Results).ToList());
            return formatter;
        }

        static TestResults RunListOfTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog)
        {
            var result = new TestResults();

            testCoverageCatalog.ReloadAllReports();

            var selectedResources = catalog.GetResources(workspaceGuid)
                .Where(resource => dataObject.TestsResourceIds.Contains(resource.ResourceID)).ToArray();

            var workflowTaskList = new List<Task<WorkflowTestResults>>();
            foreach (var testsResourceId in dataObject.TestsResourceIds)
            {
                var workflowTask = Task<WorkflowTestResults>.Factory.StartNew(() =>
                {
                    var workflowTestTaskList = new List<Task<IServiceTestModelTO>>();
                    var res = selectedResources.First(o => o.ResourceID == testsResourceId);
                    var resourcePath = res.GetResourcePath(workspaceGuid).Replace("\\", "/");
                    var workflowTestResults = new WorkflowTestResults(res);

                    var allTests = testCatalog.Fetch(testsResourceId);
                    foreach (var test in allTests)
                    {
                        var dataObjectClone = dataObject.Clone();
                        dataObjectClone.Environment = new ExecutionEnvironment();
                        dataObjectClone.TestName = test.TestName;
                        dataObjectClone.ServiceName = res.ResourceName;
                        dataObjectClone.ResourceID = res.ResourceID;
                        var lastTask = ServiceTestExecutor.ExecuteTestAsync(resourcePath, userPrinciple, workspaceGuid,
                            serializer, dataObjectClone);
                        workflowTestTaskList.Add(lastTask);

                        var report = testCoverageCatalog.FetchReport(res.ResourceID, test.TestName);
                        var lastTestCoverageRun = report?.LastRunDate;
                        if (report is null || test.LastRunDate > lastTestCoverageRun)
                        {
                            testCoverageCatalog.GenerateSingleTestCoverage(res.ResourceID, lastTask.Result);
                        }
                    }

                    Task.WaitAll(workflowTestTaskList.Cast<Task>().ToArray());
                    foreach (var task in workflowTestTaskList)
                    {
                        workflowTestResults.Add(task.Result);
                    }
                    testCoverageCatalog.GenerateAllTestsCoverage(res.ResourceName, res.ResourceID, workflowTestResults.Results);

                    return workflowTestResults;
                });

                workflowTaskList.Add(workflowTask);
            }

            Task.WaitAll(workflowTaskList.Cast<Task>().ToArray());

            foreach (var task in workflowTaskList)
            {
                result.Add(task.Result);
            }

            result.EndTime = DateTime.Now;

            return result;
        }

        public static DataListFormat RunCoverageAndReturnJSON(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog catalog, Guid workspaceGuid, Dev2JsonSerializer serializer, out string executePayload)
        {
            var allCoverageReports = RunListOfCoverage(coverageData, testCoverageCatalog, workspaceGuid, catalog);

            var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");

            var objArray = allCoverageReports.AllCoverageReportsSummary
                .Where(o => o.HasTestReports)
                .Select(o =>
                {
                    var name = o.Resource.ResourceName;
                    if (o.Resource is IFilePathResource filePath)
                    {
                        name = filePath.Path;
                    }

                    return new JObject
                    {
                        {"ResourceID", o.Resource.ResourceID},
                        {"Name", name},
                        {"Reports", new JArray(o.Reports.Select(o1 => o1.BuildTestResultJSONForWebRequest()))}
                    };
                });

            var obj = new JObject
            {
                {"StartTime", allCoverageReports.StartTime},
                {"EndTime", allCoverageReports.EndTime},
                {"Results", new JArray(objArray)},
            };
            executePayload = serializer.Serialize(obj);
            return formatter;
        }

        public static DataListFormat RunCoverageAndReturnHTML(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog catalog, Guid workspaceGuid, out string executePayload)
        {
            var allCoverageReports = RunListOfCoverage(coverageData, testCoverageCatalog, workspaceGuid, catalog);
            var allTests = TestCatalog.Instance.FetchAllTests();

            var formatter = DataListFormat.CreateFormat("HTML", EmitionTypes.Cover, "text/html; charset=utf-8");

            var stringWriter = new StringWriter();

            using (var writer = new HtmlTextWriter(stringWriter))
            {
                writer.SetupNavBarHtml("nav-bar-row", "Coverage Summary");

                allTests.SetupCountSummaryHtml(writer, "count-summary row");

                allCoverageReports.AllCoverageReportsSummary
                    .Where(o => o.HasTestReports)
                    .ToList()
                    .ForEach(oo =>
                    {
                        var resourcePath = string.Empty;
                        if (oo.Resource is IFilePathResource filePath)
                        {
                            resourcePath = filePath.Path;
                        }

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#333");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20%");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "8px 16px 16px 8px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.Write(resourcePath);
                        writer.RenderEndTag();

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml-link");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, "_new");
                        var hostname = coverageData.GetTestUrl(resourcePath);
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, hostname);
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                        writer.Write("Run Tests");
                        writer.RenderEndTag();
                        writer.RenderEndTag();

                        var (totalCoverage, workflowNodes, coveredNodes) = oo.GetTotalCoverage();

                        writer.SetupWorkflowReportsHtml(totalCoverage, "SetupWorkflowReportsHtml");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0 0 0 35px");
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "workflow-nodes-row");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);



                        workflowNodes.ForEach(node => node.SetupWorkflowNodeHtml(writer, "workflow-nodes", coveredNodes));

                        writer.RenderEndTag();
                    });
            }

            executePayload = stringWriter.ToString();
            return formatter;
        }

        private static AllCoverageReports RunListOfCoverage(ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, Guid workspaceGuid, IResourceCatalog catalog)
        {
            var allCoverageReports = new AllCoverageReports
            {
                StartTime = DateTime.Now
            };

            var resources = catalog.GetResources<IWarewolfWorkflow>(workspaceGuid);
            var selectedResources = resources.Where(resource => coverageData.CoverageReportResourceIds.Contains(resource.ResourceID)).ToArray();

            testCoverageCatalog.ReloadAllReports();
            var coverageReportsTemp = new List<WorkflowCoverageReports>();
            foreach (var coverageResourceId in coverageData.CoverageReportResourceIds)
            {
                var res = selectedResources.First(o => o.ResourceID == coverageResourceId);
                var coverageReports = new WorkflowCoverageReports(res);

                var allWorkflowReports = testCoverageCatalog.Fetch(coverageResourceId);
                if (allWorkflowReports?.Count > 0)
                {
                    foreach (var workflowReport in allWorkflowReports)
                    {
                        coverageReports.Add(workflowReport);
                    }

                    coverageReportsTemp.Add(coverageReports);
                }
            }

            foreach (var item in coverageReportsTemp)
            {
                allCoverageReports.Add(item);
            }

            allCoverageReports.EndTime = DateTime.Now;

            return allCoverageReports;
        }
    }

    internal class AllCoverageReports
    {
        public List<WorkflowCoverageReports> AllCoverageReportsSummary { get; } = new List<WorkflowCoverageReports>();
        public JToken StartTime { get; internal set; }
        public JToken EndTime { get; internal set; }

        internal void Add(WorkflowCoverageReports item)
        {
            AllCoverageReportsSummary.Add(item);
        }
    }

    internal class WorkflowCoverageReports
    {
        public WorkflowCoverageReports(IWarewolfWorkflow resource)
        {
            Resource = resource;
        }

        public List<IServiceTestCoverageModelTo> Reports { get; } = new List<IServiceTestCoverageModelTo>();
        public bool HasTestReports => Reports.Count > 0;
        public IWarewolfWorkflow Resource { get; }

        internal void Add(IServiceTestCoverageModelTo coverage)
        {
            Reports.Add(coverage);
        }

        public (double TotalCoverage, List<IWorkflowNode> AllWorkflowNodes, IWorkflowNode[] CoveredNodes) GetTotalCoverage()
        {
            var coveredNodes = Reports
                .SelectMany(o => o.AllTestNodesCovered)
                .SelectMany(o => o.TestNodesCovered)
                .GroupBy(n => n.ActivityID)
                .Select(o => o.First()).ToArray();

            var accum = coveredNodes
                .Select(o => o.ActivityID)
                .Distinct().ToList();
            var allWorkflowNodes = Resource.WorkflowNodesForHtml;
            var accum2 = allWorkflowNodes.Select(o=> o.UniqueID).ToList();
            var activitiesExistingInTests = accum2.Intersect(accum).ToList();
            var total = Math.Round(activitiesExistingInTests.Count / (double)accum2.Count, 2);
            return (total, allWorkflowNodes, coveredNodes);
        }
    }

    public class TestResults
    {
        public TestResults()
        {
            StartTime = DateTime.Now;
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; set; }
        public List<WorkflowTestResults> Results { get; } = new List<WorkflowTestResults>();

        public void Add(WorkflowTestResults taskResult)
        {
            Results.Add(taskResult);
        }
    }

    public class WorkflowTestResults
    {
        public WorkflowTestResults(IWarewolfResource res)
        {
            Resource = res;
        }

        public IWarewolfResource Resource { get; }
        public List<IServiceTestModelTO> Results { get; } = new List<IServiceTestModelTO>();
        public bool HasTestResults => Results.Count > 0;

        public void Add(IServiceTestModelTO result)
        {
            Results.Add(result);
        }
    }
}