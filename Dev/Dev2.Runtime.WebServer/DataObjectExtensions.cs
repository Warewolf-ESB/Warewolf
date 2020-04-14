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
using Dev2.Runtime;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Dev2.Runtime.WebServer
{
    public static class DataObjectExtensions
    {
        private static string _originalServiceName;
        private static string _originalWebServerUrl;

        public static string SetEmitionType(this IDSFDataObject dataObject, WebRequestTO webRequest, string serviceName, NameValueCollection headers)
        {
            _originalServiceName = serviceName;
            _originalWebServerUrl = webRequest.WebServerUrl;
            var startLocation = serviceName.LastIndexOf(".", StringComparison.Ordinal);
            if (!string.IsNullOrEmpty(serviceName) && startLocation > 0)
            {
                dataObject.ReturnType = EmitionTypes.XML;

                var extension = serviceName.Substring(startLocation + 1);
                return SetReturnTypeForExtension(dataObject, startLocation, extension);
            }

            if (serviceName == "*")
            {
                if (webRequest.WebServerUrl.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TEST;
                }

                if (webRequest.WebServerUrl.EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.Cover;
                }

                if (webRequest.WebServerUrl.EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.CoverJson;
                }

                if (webRequest.WebServerUrl.EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
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

            var serviceName = _originalServiceName.Substring(0, loc);

            var isApi = typeOf.Equals("api", StringComparison.OrdinalIgnoreCase);
            var isTests = typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase);
            var isTrx = typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase);
            if (isTests || isTrx)
            {
                dataObject.IsServiceTestExecution = true;
                dataObject.TestName = "*";
                var idx = _originalServiceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                if (idx > loc)
                {
                    var testName = _originalServiceName.Substring(idx + 1).ToUpper();
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
                    serviceName = _originalServiceName.Substring(0, _originalServiceName.LastIndexOf(".", StringComparison.Ordinal));
                }
            }

            var isCover = typeOf.StartsWith("coverage", StringComparison.InvariantCultureIgnoreCase);
            var isCoverJson = typeOf.StartsWith("json", StringComparison.InvariantCultureIgnoreCase);
            if (isCoverJson || isCover)
            {
                if (isCover)
                {
                    dataObject.ReturnType = EmitionTypes.Cover;
                }

                if (isCoverJson)
                {
                    dataObject.ReturnType = EmitionTypes.CoverJson;
                    serviceName = _originalServiceName.Substring(0, _originalServiceName.LastIndexOf(".coverage.json", StringComparison.Ordinal));
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
            if (IsRunAllTestsRequest(dataObject.ReturnType, serviceName) || IsRunAllCoverageRequest(dataObject.ReturnType, serviceName))
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
            if (IsRunAllTestsRequest(coverageData.ReturnType, serviceName) || IsRunAllCoverageRequest(coverageData.ReturnType, serviceName))
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
            var isRunAllCoverage = !string.IsNullOrEmpty(serviceName);
            isRunAllCoverage &= serviceName == "*" || serviceName == ".coverage" || serviceName == ".coverage.json";
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
                canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource != null && resource.ResourceType == "ReservedService");
            }

            return canExecute;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnJSON(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
            Dev2JsonSerializer serializer,
            IResourceCatalog catalog, ITestCatalog testCatalog,
            out string executePayload)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
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
            out string executePayload)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(dataObject.ServiceName, testResults.Results.SelectMany(o => o.Results).ToList());
            return formatter;
        }

        static TestResults RunListOfTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog)
        {
            var result = new TestResults();

            var _testCoverageCatalog = TestCoverageCatalog.Instance;
            _testCoverageCatalog.ReloadAllReports();

            var selectedResources = catalog.GetResources(workspaceGuid)
                .Where(resource => dataObject.TestsResourceIds.Contains(resource.ResourceID)).ToArray();

            var workflowTaskList = new List<Task<WorkflowTestResults>>();
            foreach (var testsResourceId in dataObject.TestsResourceIds)
            {
                var workflowTask = Task<WorkflowTestResults>.Factory.StartNew(() =>
                {
                    var workflowTestTaskList = new List<Task<IServiceTestModelTO>>();
                    var res = selectedResources.FirstOrDefault(o => o.ResourceID == testsResourceId);
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

                        var report = _testCoverageCatalog.FetchReport(res.ResourceID, test.TestName);
                        var lastTestCoverageRun = report?.LastRunDate;
                        if (report is null || test.LastRunDate > lastTestCoverageRun)
                        {
                            _testCoverageCatalog.GenerateSingleTestCoverage(res.ResourceID, lastTask.Result);
                        }
                    }

                    Task.WaitAll(workflowTestTaskList.ToArray());
                    foreach (var task in workflowTestTaskList)
                    {
                        workflowTestResults.Add(task.Result);
                    }

                    /*var workflowCoverageReport = _testCoverageCatalog.FetchReport(res.ResourceID, res.ResourceName);
                    var lastWorkflowCoverageRun = workflowCoverageReport?.LastRunDate;
                    var lastModifiedDate = System.IO.File.GetLastWriteTime(res.FilePath); //TODO: can we add LastRunDate to workflow set on save()? 
                    if (workflowCoverageReport is null || lastModifiedDate > lastWorkflowCoverageRun)*/
                    {
                        _testCoverageCatalog.GenerateAllTestsCoverage(res.ResourceID, workflowTestResults.Results);
                    }

                    return workflowTestResults;
                });

                workflowTaskList.Add(workflowTask);
            }

            Task.WaitAll(workflowTaskList.ToArray());

            foreach (var task in workflowTaskList)
            {
                result.Add(task.Result);
            }

            result.EndTime = DateTime.Now;

            return result;
        }

        public static DataListFormat RunCoverageAndReturnJSON(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog catalog, Guid workspaceGuid, Dev2JsonSerializer serializer, out string executePayload)
        {
            var allCoverageReports = RunListOfCoverage(coverageData, testCoverageCatalog, workspaceGuid, serializer, catalog);

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
                        {"Reports", new JArray(o.ReportsPerWorkflow.Select(o1 => o1.BuildTestResultJSONForWebRequest()))}
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

        public static DataListFormat RunCoverageAndReturnHTML(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog catalog, Guid workspaceGuid, Dev2JsonSerializer serializer, out string executePayload)
        {
            var allCoverageReports = RunListOfCoverage(coverageData, testCoverageCatalog, workspaceGuid, serializer, catalog);
            var allTests = TestCatalog.Instance.FetchAllTests();

            var formatter = DataListFormat.CreateFormat("HTML", EmitionTypes.Cover, "text/html; charset=utf-8");

            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.SetupNavBarHtml("nav-bar-row", "Coverage Summary");

                allTests.SetupCountSummaryHtml(writer, "count-summary row");

                allCoverageReports.AllCoverageReportsSummary
                    .Where(o => o.HasTestReports)
                    .Select(o =>
                    {
                        var name = o.Resource.ResourceName;
                        var resourcePath = string.Empty;
                        if (o.Resource is IFilePathResource filePath)
                        {
                            resourcePath = filePath.Path;
                        }

                        return new ReportTempHolder
                        {
                            ResourceID = o.Resource.ResourceID,
                            Name = name,
                            ResourcePath = resourcePath,
                            Reports = o.ReportsPerWorkflow
                        };
                    })
                    .ToList()
                    .ForEach(oo =>
                    {
                        var workflowReport = oo.Reports.Where(r => oo.ResourcePath.Contains(r.ReportName)).FirstOrDefault();
                        if (workflowReport != null)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#333");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20%");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "8px 16px 16px 8px");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            writer.Write(oo.ResourcePath);
                            writer.RenderEndTag();
                            
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml-link");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100px");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_new");
                            var hostname = GetTestUrl(oo);
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, hostname);
                            writer.RenderBeginTag(HtmlTextWriterTag.A);
                            writer.Write("Run Tests");
                            writer.RenderEndTag();
                            writer.RenderEndTag();
                            
                            workflowReport.SetupWorkflowReportsHtml(writer, "SetupWorkflowReportsHtml");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0 0 0 35px");
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "workflow-nodes-row");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            var nodes = new List<IWorkflowNode>();
                            workflowReport.AllTestNodesCovered.ForEach(c => c.ForEach(o => o.TestNodesCovered.ForEach(node => nodes.Add(node))));

                            var coveredNodes = nodes.GroupBy(n => n.ActivityID).Select(o => o.FirstOrDefault()).ToList();

                            var workflow = new WorkflowWrapper(oo.ResourceID);

                            var allWorkflowNodes = workflow.GetHTMLWorkflowNodes();
                           
                            allWorkflowNodes.ForEach(node => node.SetupWorkflowNodeHtml(writer, "workflow-nodes", coveredNodes));
                            
                            writer.RenderEndTag();
                        }
                    });
            }

            executePayload = stringWriter.ToString();
            return formatter;
        }

        private static string GetTestUrl(ReportTempHolder oo)
        {
            Uri myUri = new Uri(_originalWebServerUrl);
            var security = "";
            foreach (var segment in myUri.Segments)
            {
                if (segment.Contains("public"))
                    security = segment;
                if (segment.Contains("secure"))
                    security = segment;
            }

            var filepath = oo.ResourcePath.Replace("\\", "/");
            var hostname = myUri.Scheme + "://" + myUri.Authority + "/" + security + filepath + ".tests";
            return hostname;
        }

        private static AllCoverageReports RunListOfCoverage(ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog)
        {
            var allCoverageReports = new AllCoverageReports
            {
                StartTime = DateTime.Now
            };

            var selectedResources = catalog.GetResources(workspaceGuid)
                .Where(resource => coverageData.CoverageReportResourceIds.Contains(resource.ResourceID)).ToArray();

            testCoverageCatalog.ReloadAllReports();
            var coverageReportsTemp = new List<WorkflowCoverageReports>();
            foreach (var coverageResourceId in coverageData.CoverageReportResourceIds)
            {
                var res = selectedResources.FirstOrDefault(o => o.ResourceID == coverageResourceId);
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

    internal class ReportTempHolder
    {
        public Guid ResourceID { get; internal set; }
        public string Name { get; internal set; }

        public List<IServiceTestCoverageModelTo> Reports { get; set; }
        public string ResourcePath { get; internal set; }
    }

    internal class AllCoverageReports
    {
        public AllCoverageReports()
        {
        }

        public List<WorkflowCoverageReports> AllCoverageReportsSummary { get; set; } = new List<WorkflowCoverageReports>();
        public JToken StartTime { get; internal set; }
        public JToken EndTime { get; internal set; }

        internal void Add(WorkflowCoverageReports item)
        {
            AllCoverageReportsSummary.Add(item);
        }
    }

    internal class WorkflowCoverageReports
    {
        public WorkflowCoverageReports(IWarewolfResource resource)
        {
            Resource = resource;
        }

        public List<IServiceTestCoverageModelTo> ReportsPerWorkflow { get; set; } = new List<IServiceTestCoverageModelTo>();
        public Guid WorkflowId { get; set; }
        public bool HasTestReports => ReportsPerWorkflow.Count > 0;
        public IWarewolfResource Resource { get; set; }

        internal void Add(IServiceTestCoverageModelTo lii)
        {
            ReportsPerWorkflow.Add(lii);
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