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
using Dev2.Data;
using Dev2.Common.Interfaces.Runtime.Services;

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

            if (serviceName == "*" || serviceName == ".coverage")
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
                dataObject.TestsResourceIds = resources.Where(o => o is IWarewolfWorkflow).Select(p => p.ResourceID).GroupBy(o => o).Select(o => o.Key).ToArray();
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
                var path = pathOfAllResources;
                if (string.IsNullOrEmpty(pathOfAllResources))
                {
                    path = "/";
                }

                var resources = catalog.GetExecutableResources(path);
                coverageData.CoverageReportResourceIds = resources.Where(o => o is IWarewolfWorkflow).Select(p => p.ResourceID).GroupBy(o => o).Select(o => o.Key).ToArray();
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

            if (Guid.TryParse(serviceName, out var resourceId))
            {
                localResource = catalog.GetResource(dataObject.WorkspaceID, resourceId);
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
                if (Guid.TryParse(stringDynaResourceId, out resourceId))
                {
                    localResource = catalog.GetResource(dataObject.WorkspaceID, resourceId);
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
            out string executePayload, ITestCoverageCatalog testCoverageCatalog,
            IServiceTestExecutorWrapper serviceTestExecutorWrapper)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog, testCoverageCatalog, serviceTestExecutorWrapper);
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
            out string executePayload, ITestCoverageCatalog testCoverageCatalog,
            IServiceTestExecutorWrapper serviceTestExecutorWrapper)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog, testCoverageCatalog, serviceTestExecutorWrapper);
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(dataObject.ServiceName, testResults.Results.SelectMany(o => o.Results).ToList());
            return formatter;
        }

        static TestResults RunListOfTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IServiceTestExecutorWrapper serviceTestExecutorWrapper)
        {
            var result = new TestResults();

            var selectedResources = catalog.GetResources(workspaceGuid)
                ?.Where(resource => dataObject.TestsResourceIds.Contains(resource.ResourceID)).ToArray();

            if (selectedResources != null)
            {
                var workflowTaskList = new List<Task<WorkflowTestResults>>();
                foreach (var testsResourceId in dataObject.TestsResourceIds)
                {
                    var workflowTask = Task<WorkflowTestResults>.Factory.StartNew(() =>
                    {
                        var workflowTestTaskList = new List<Task<IServiceTestModelTO>>();
                        var res = selectedResources.FirstOrDefault(o => o.ResourceID == testsResourceId);
                        if (res is null)
                        {
                            return null;
                        }
                        else
                        {
                            var resourcePath = res.GetResourcePath(workspaceGuid).Replace("\\", "/");
                            var workflowTestResults = new WorkflowTestResults(res);

                            var allTests = testCatalog.Fetch(testsResourceId);
                            foreach (var (test, dataObjectClone) in from test in allTests
                                                                    let dataObjectClone = dataObject.Clone()
                                                                    select (test, dataObjectClone))
                            {
                                dataObjectClone.Environment = new ExecutionEnvironment();
                                dataObjectClone.TestName = test.TestName;
                                dataObjectClone.ServiceName = res.ResourceName;
                                dataObjectClone.ResourceID = res.ResourceID;
                                var lastTask = serviceTestExecutorWrapper.ExecuteTestAsync(resourcePath, userPrinciple, workspaceGuid,
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

                            var testResults = workflowTestResults.Results;
                            if (testResults.Count > 0)
                            {
                                testCoverageCatalog.GenerateAllTestsCoverage(res.ResourceName, res.ResourceID, testResults);
                            }

                            return workflowTestResults;
                        }
                    });

                    if (workflowTask != null)
                    {
                        workflowTaskList.Add(workflowTask);
                    }
                }

                Task.WaitAll(workflowTaskList.Cast<Task>().ToArray());

                foreach (var task in workflowTaskList)
                {
                    if (task.Result != null)
                    {
                        result.Add(task.Result);
                    }
                }
            }
            
            result.EndTime = DateTime.Now;

            return result;
        }


        public static DataListFormat RunCoverageAndReturnJSON(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, ITestCatalog testCatalog, IResourceCatalog catalog, Guid workspaceGuid, Dev2JsonSerializer serializer, out string executePayload)
        {
            var (allCoverageReports, _) = RunListOfCoverage(coverageData, testCoverageCatalog, testCatalog, workspaceGuid, catalog);

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

        public static DataListFormat RunCoverageAndReturnHTML(this ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, ITestCatalog testCatalog, IResourceCatalog catalog, Guid workspaceGuid, out string executePayload)
        {
            var (allCoverageReports, allTestResults) = RunListOfCoverage(coverageData, testCoverageCatalog, testCatalog, workspaceGuid, catalog);

            var formatter = DataListFormat.CreateFormat("HTML", EmitionTypes.Cover, "text/html; charset=utf-8");

            var stringWriter = new StringWriter();

            using (var writer = new HtmlTextWriter(stringWriter))
            {
                writer.SetupNavBarHtml();
                
                allTestResults.Results
                    .SelectMany(o => o.Results)
                    .ToList()
                    .SetupCountSummaryHtml(writer, coverageData);

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

                        writer.SetupWorkflowRowHtml(resourcePath, coverageData, oo);
                    });
            }

            executePayload = stringWriter.ToString();
            return formatter;
        }

        private static (AllCoverageReports AllCoverageReports, TestResults AllTestResults) RunListOfCoverage(ICoverageDataObject coverageData, ITestCoverageCatalog testCoverageCatalog, ITestCatalog testCatalog, Guid workspaceGuid, IResourceCatalog catalog)
        {
            var allTestResults = new TestResults();

            var allCoverageReports = new AllCoverageReports
            {
                StartTime = DateTime.Now
            };

            var resources = catalog.GetResources<IWarewolfWorkflow>(workspaceGuid);
            var selectedResources = resources.Where(resource => coverageData.CoverageReportResourceIds.Contains(resource.ResourceID)).ToArray();

            var testResultsTemp = new List<WorkflowTestResults>();
            var coverageReportsTemp = new List<WorkflowCoverageReports>();

            foreach (var coverageResourceId in coverageData.CoverageReportResourceIds)
            {
                var res = selectedResources.FirstOrDefault(o => o.ResourceID == coverageResourceId);
                if (res is null)
                {
                    continue;
                }

                var workflowTestResults = new WorkflowTestResults();
                testCatalog.Fetch(coverageResourceId)
                    ?.ForEach(o => workflowTestResults.Add(o));

                testResultsTemp.Add(workflowTestResults);

                var coverageReports = new WorkflowCoverageReports(res);
                testCoverageCatalog.Fetch(coverageResourceId)
                    ?.ForEach(o => coverageReports.Add(o));

                coverageReportsTemp.Add(coverageReports);
            }

            testResultsTemp.ForEach(o => allTestResults.Add(o));

            coverageReportsTemp.ForEach(o => allCoverageReports.Add(o));

            allTestResults.EndTime = DateTime.Now;
            allCoverageReports.EndTime = DateTime.Now;

            return (allCoverageReports, allTestResults);
        }

        public interface IServiceTestExecutorWrapper
        {
            Task<IServiceTestModelTO> ExecuteTestAsync(string resourcePath, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObjectClone);
        }
        public class ServiceTestExecutorWrapper : IServiceTestExecutorWrapper
        {
            public Task<IServiceTestModelTO> ExecuteTestAsync(string resourcePath, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObjectClone)
            {
                return ServiceTestExecutor.ExecuteTestAsync(resourcePath, userPrinciple, workspaceGuid, serializer, dataObjectClone);
            }
        }
    }
}