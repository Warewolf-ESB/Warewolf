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
using Dev2.Common.Interfaces.Data;
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

namespace Dev2.Runtime.WebServer
{
    static class DataObjectExtentions
    {
        public static string SetEmitionType(this IDSFDataObject dataObject, WebRequestTO webRequest, string serviceName, NameValueCollection headers)
        {
            int extensionStartLocation;
            var originalServiceName = serviceName;
            if (!string.IsNullOrEmpty(serviceName) && (extensionStartLocation = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
            {
                dataObject.ReturnType = EmitionTypes.XML;

                if (extensionStartLocation > 0)
                {
                    var extension = serviceName.Substring(extensionStartLocation + 1);
                    serviceName = SetReturnTypeForExtension(dataObject, extensionStartLocation, originalServiceName, extension);
                }
            }
            else
            {
                if (serviceName == "*" && webRequest.WebServerUrl.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TEST;
                }
                if (serviceName == "*" && webRequest.WebServerUrl.EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TRX;
                }
                dataObject.SetContentType(headers);
            }
            return serviceName;
        }

        private static string SetReturnTypeForExtension(IDSFDataObject dataObject, int loc, string originalServiceName, string typeOf)
        {
            if (Enum.TryParse(typeOf.ToUpper(), out EmitionTypes myType))
            {
                dataObject.ReturnType = myType;
            }

            var serviceName = !typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase) ? originalServiceName.Substring(0, loc) : originalServiceName.Substring(0, originalServiceName.Substring(0, loc).LastIndexOf(".", StringComparison.Ordinal));
            if (typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase) || typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase))
            {
                dataObject.IsServiceTestExecution = true;
                var idx = originalServiceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                if (idx > loc)
                {
                    var testName = originalServiceName.Substring(idx + 1).ToUpper();
                    dataObject.TestName = string.IsNullOrEmpty(testName) ? "*" : testName;
                }
                else
                {
                    dataObject.TestName = "*";
                }
                if (typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TEST;
                }
                if (typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataObject.ReturnType = EmitionTypes.TRX;
                }
            }
            if (typeOf.Equals("api", StringComparison.OrdinalIgnoreCase))
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
            if (headers != null)
            {
                var customTransactionId = headers.Get("Warewolf-Custom-Transaction-Id");
                if (!string.IsNullOrEmpty(customTransactionId))
                {
                    dataObject.CustomTransactionID = customTransactionId;
                }
                var executionID = headers.Get("Warewolf-Execution-Id");
                if (!string.IsNullOrEmpty(executionID))
                {
                    dataObject.ExecutionID = Guid.Parse(executionID);
                }
                else
                {
                    dataObject.ExecutionID = Guid.NewGuid();
                }
            }
        }
        public static void SetCustomTransactionID(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            if (headers != null)
            {
                var customTransactionId = headers.Get("CustomTransactionID");
                if (string.IsNullOrEmpty(customTransactionId))
                {
                    customTransactionId = headers.Get("CustomTransactionID");
                }
                if (!string.IsNullOrEmpty(customTransactionId))
                {
                    dataObject.CustomTransactionID = customTransactionId;
                }
            }
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
            if (headers != null)
            {
                var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
                var remoteId = headers.Get(HttpRequestHeader.From.ToString());

                if (isRemote != null && remoteId != null)
                {
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
            }
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
            else
            {
                dataObject.TestsResourceIds = new[] { resource.ResourceID };
            }
        }

        public static void SetupForTestExecution(this IDSFDataObject dataObject, string serviceName, NameValueCollection headers)
        {
            if (IsRunAllTestsRequest(dataObject.ReturnType, serviceName))
            {
                dataObject.IsServiceTestExecution = true;
                dataObject.TestName = "*";
            }
            else
            {
                dataObject.SetContentType(headers);
            }
        }

        static bool IsRunAllTestsRequest(EmitionTypes returnType, string serviceName) => !string.IsNullOrEmpty(serviceName) && (serviceName == "*" || serviceName == ".tests" || serviceName == ".tests.trx") && (returnType == EmitionTypes.TEST || returnType == EmitionTypes.TRX);

        public static void SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IResource resource)
        {
            IResource localResource = null;

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

        static void MapServiceToDataObjects(IDSFDataObject dataObject, IResource localResource)
        {
            dataObject.ServiceName = localResource.ResourceName;
            dataObject.ResourceID = localResource.ResourceID;
            dataObject.SourceResourceID = localResource.ResourceID;
        }

        public static bool CanExecuteCurrentResource(this IDSFDataObject dataObject, IResource resource, IAuthorizationService service)
        {
            if (resource is null)
            {
                return false;
            }
            var canExecute = true;
            if (service != null && dataObject.ReturnType != EmitionTypes.TRX)
            {
                var hasView = service.IsAuthorized(dataObject.ExecutingUser,AuthorizationContext.View, dataObject.ResourceID.ToString());
                var hasExecute = service.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, dataObject.ResourceID.ToString());
                canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource != null && resource.ResourceType == "ReservedService");
            }
            return canExecute;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnJSON(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
                                                                         Dev2JsonSerializer serializer, DataListFormat formatter,
                                                                         IResourceCatalog catalog, ITestCatalog testCatalog,
                                                                         ref string executePayload)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");

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
                                                                        Dev2JsonSerializer serializer, DataListFormat formatter,
                                                                        IResourceCatalog catalog, ITestCatalog testCatalog,
                                                                        ref string executePayload)
        {
            var testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
            formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(dataObject.ServiceName, testResults.Results.SelectMany(o => o.Results).ToList());
            return formatter;
        }

        private class WorkflowTestResults
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
        private class TestResults
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
        static TestResults RunListOfTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog)
        {
            var result = new TestResults();

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
                    }

                    Task.WaitAll(workflowTestTaskList.ToArray());
                    foreach (var task in workflowTestTaskList)
                    {
                        workflowTestResults.Add(task.Result);
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
    }
}
