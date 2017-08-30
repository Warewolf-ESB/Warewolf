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

namespace Dev2.Runtime.WebServer
{
    internal static class DataObjectExtentions
    {
        public static string SetEmitionType(this IDSFDataObject dataObject, WebRequestTO webRequest, string serviceName, NameValueCollection headers)
        {
            int loc;
            if (!string.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;

                if (loc > 0)
                {
                    var typeOf = serviceName.Substring(loc + 1);
                    EmitionTypes myType;
                    if (Enum.TryParse(typeOf.ToUpper(), out myType))
                    {
                        dataObject.ReturnType = myType;
                    }

                    serviceName = !typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase) ? serviceName.Substring(0, loc) : serviceName.Substring(0, serviceName.Substring(0, loc).LastIndexOf(".", StringComparison.Ordinal));
                    if (typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase) || typeOf.StartsWith("trx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dataObject.IsServiceTestExecution = true;
                        var idx = serviceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                        if (idx > loc)
                        {
                            var testName = serviceName.Substring(idx + 1).ToUpper();
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

        public static void SetTestResourceIds(this IDSFDataObject dataObject, IResourceCatalog catalog, WebRequestTO webRequest, string serviceName)
        {
            if (IsRunAllTestsRequest(dataObject.ReturnType, serviceName))
            {
                var pathOfAllResources = webRequest.GetPathForAllResources();
                dataObject.ResourceID = Guid.Empty;
                if (string.IsNullOrEmpty(pathOfAllResources))
                {
                    var resources = catalog.GetResources(GlobalConstants.ServerWorkspaceID);
                    dataObject.TestsResourceIds = resources.Select(p => p.ResourceID).ToList();
                }
                else
                {
                    var resources = catalog.GetResources(GlobalConstants.ServerWorkspaceID);
                    var resourcesToRunTestsFor = resources?.Where(a => a.GetResourcePath(GlobalConstants.ServerWorkspaceID)
                                                   .StartsWith(pathOfAllResources, StringComparison.InvariantCultureIgnoreCase));
                    dataObject.TestsResourceIds = resourcesToRunTestsFor?.Select(p => p.ResourceID).ToList();
                }
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

        private static bool IsRunAllTestsRequest(EmitionTypes returnType, string serviceName)
        {
            return !string.IsNullOrEmpty(serviceName) && (serviceName == "*" || serviceName == ".tests" || serviceName == ".tests.trx") && (returnType == EmitionTypes.TEST || returnType == EmitionTypes.TRX);
        }

        public static void SetResourceNameAndId(this IDSFDataObject dataObject, IResourceCatalog catalog, string serviceName, out IResource resource)
        {
            IResource localResource = null;
            Guid resourceID;

            if (Guid.TryParse(serviceName, out resourceID))
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
                var stringDynaResourceId = serviceName.Replace(".xml", "").Replace(".json", "");
                if (Guid.TryParse(stringDynaResourceId, out resourceID))
                {
                    localResource = catalog.GetResource(dataObject.WorkspaceID, resourceID);
                    if (localResource != null)
                    {
                        MapServiceToDataObjects(dataObject, localResource);
                    }
                }

            }
            resource = localResource;
        }

        private static void MapServiceToDataObjects(IDSFDataObject dataObject, IResource localResource)
        {
            dataObject.ServiceName = localResource.ResourceName;
            dataObject.ResourceID = localResource.ResourceID;
            dataObject.SourceResourceID = localResource.ResourceID;
        }

        public static bool CanExecuteCurrentResource(this IDSFDataObject dataObject, IResource resource, IAuthorizationService service)
        {
            var canExecute = true;
            if (service != null && dataObject.ReturnType != EmitionTypes.TEST && dataObject.ReturnType != EmitionTypes.TRX)
            {
                var hasView = service.IsAuthorized(AuthorizationContext.View, dataObject.ResourceID.ToString());
                var hasExecute = service.IsAuthorized(AuthorizationContext.Execute, dataObject.ResourceID.ToString());
                canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource != null && resource.ResourceType == "ReservedService");
            }
            return canExecute;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnJSON(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
                                                                         Dev2JsonSerializer serializer, DataListFormat formatter,
                                                                         IResourceCatalog catalog, ITestCatalog testCatalog,
                                                                         ref string executePayload)
        {
            List<IServiceTestModelTO> testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            var objArray = (from testRunResult in testResults
                            where testRunResult != null
                            select testRunResult.BuildTestResultJSONForWebRequest()
                            ).ToList();
            if (objArray.Count > 0)
            {
                executePayload = (executePayload == string.Empty ? "[" : executePayload.TrimEnd("\r\n]".ToCharArray()) + ",") + serializer.Serialize(objArray).TrimStart('[');
            }
            return formatter;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnTRX(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
                                                                        Dev2JsonSerializer serializer, DataListFormat formatter,
                                                                        IResourceCatalog catalog, ITestCatalog testCatalog,
                                                                        ref string executePayload)
        {
            List<IServiceTestModelTO> testResults = RunListOfTests(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog);
            formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(dataObject.ServiceName, testResults);
            return formatter;
        }

        public static DataListFormat RunSingleTestBatchAndReturnJSON(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, DataListFormat formatter, string serviceName, ITestCatalog testCatalog, ref string executePayload)
        {
            List<IServiceTestModelTO> testResults = RunAllTestsForWorkflow(dataObject, serviceName, userPrinciple, workspaceGuid, serializer, testCatalog);

            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            var objArray = (from testRunResult in testResults
                            where testRunResult != null
                            select testRunResult.BuildTestResultJSONForWebRequest()
                            ).ToList();
            executePayload = serializer.Serialize(objArray);
            return formatter;
        }

        public static DataListFormat RunSingleTestBatchAndReturnTRX(this IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, DataListFormat formatter, string serviceName, ITestCatalog testCatalog, ref string executePayload)
        {
            List<IServiceTestModelTO> testResults = RunAllTestsForWorkflow(dataObject, serviceName, userPrinciple, workspaceGuid, serializer, testCatalog);

            formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(serviceName, testResults);
            return formatter;
        }

        private static List<IServiceTestModelTO> RunAllTestsForWorkflow(IDSFDataObject dataObject, string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, string testsResourceId=null)
        {
            var allTests = testCatalog.Fetch(dataObject.ResourceID) ?? new List<IServiceTestModelTO>();
            var taskList = new List<Task>();
            var testResults = new List<IServiceTestModelTO>();
            foreach (var test in allTests.Where(to => to.Enabled))
            {
                dataObject.ResourceID = test.ResourceId;
                var dataObjectClone = dataObject.Clone();
                dataObjectClone.Environment = new ExecutionEnvironment();
                dataObjectClone.TestName = test.TestName;
                dataObjectClone.ServiceName = serviceName;
                var lastTask = ServiceTestExecutor.GetTaskForTestExecution(serviceName, userPrinciple, workspaceGuid,
                    serializer, testResults, dataObjectClone);
                taskList.Add(lastTask);
            }
            Task.WaitAll(taskList.ToArray());
            return testResults;
        }

        private static List<IServiceTestModelTO> RunListOfTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog)
        {
            var testResults = new List<IServiceTestModelTO>();
            var taskList = new List<Task>();
            foreach (var testsResourceId in dataObject.TestsResourceIds)
            {
                var allTests = testCatalog.Fetch(testsResourceId);
                foreach (var test in allTests)
                {
                    var dataObjectClone = dataObject.Clone();
                    dataObjectClone.Environment = new ExecutionEnvironment();
                    dataObjectClone.TestName = test.TestName;
                    var res = catalog.GetResource(GlobalConstants.ServerWorkspaceID, testsResourceId);
                    dataObjectClone.ServiceName = res.ResourceName;
                    var resourcePath = res.GetResourcePath(GlobalConstants.ServerWorkspaceID).Replace("\\", "/");
                    var lastTask = ServiceTestExecutor.GetTaskForTestExecution(resourcePath, userPrinciple, workspaceGuid,
                        serializer, testResults, dataObjectClone);
                    taskList.Add(lastTask);
                }
            }
            Task.WaitAll(taskList.ToArray());
            return testResults;
        }
    }
}
