using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Interfaces;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using Dev2.Web;
using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestExecutor
    {
        public static async Task GetTaskForTestExecution(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ICollection<IServiceTestModelTO> testResults, IDSFDataObject dataObjectClone)
        {
            var lastTask = Task.Run(() =>
            {
                var interTestRequest = new EsbExecuteRequest { ServiceName = serviceName };
                var dataObjectToUse = dataObjectClone;
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                    var esbEndpointClone = new EsbServicesEndpoint();
                    esbEndpointClone.ExecuteRequest(dataObjectToUse, interTestRequest, workspaceGuid, out ErrorResultTO errs);
                });
                var result = serializer.Deserialize<ServiceTestModelTO>(interTestRequest.ExecuteResult);
                if (result == null)
                {
                    if (interTestRequest.ExecuteResult != null)
                    {
                        var r = serializer.Deserialize<TestRunResult>(interTestRequest.ExecuteResult.ToString()) ?? new TestRunResult { TestName = dataObjectToUse.TestName };
                        result = new ServiceTestModelTO { Result = r, TestName = r.TestName };
                    }
                }
                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObjectToUse.DataListID);
                dataObjectToUse.Environment = null;
                testResults.Add(result);
            });
            await lastTask.ConfigureAwait(false);
        }

        public static string SetpForTestExecution(Dev2JsonSerializer serializer, EsbExecuteRequest esbExecuteRequest,IDSFDataObject dataObject)
        {
            var result = serializer.Deserialize<ServiceTestModelTO>(esbExecuteRequest.ExecuteResult);
            string executePayload;
            if (result != null)
            {
                var resObj = result.BuildTestResultJSONForWebRequest();
                executePayload = serializer.Serialize(resObj);
                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
                dataObject.Environment = null;
            }
            else
            {
                executePayload = serializer.Serialize(new JObject());
            }
            return executePayload;
        }

        public static DataListFormat ExecuteTests(string serviceName, IDSFDataObject dataObject, DataListFormat formatter,
            IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, IResourceCatalog resourceCatalog, ref string executePayload)
        {
            if (dataObject.TestsResourceIds?.Any() ?? false)
            {
                formatter = RunMultipleTestBatchesAndReturnJSON(dataObject, userPrinciple, workspaceGuid, serializer, formatter,
                    resourceCatalog, testCatalog, ref executePayload);
                dataObject.ResourceID = Guid.Empty;
            }
            else
            {
                List<IServiceTestModelTO> testResults = RunSingleTestBatch(dataObject, serviceName, userPrinciple, workspaceGuid, serializer, testCatalog);
                const string TrxExtension = ".tests.trx";
                if (!serviceName.EndsWith(TrxExtension, StringComparison.CurrentCultureIgnoreCase))
                {
                    var objArray = SerializeTestResultsToJSON(serviceName, testResults, ref formatter);
                    executePayload = serializer.Serialize(objArray);
                }
                else
                {
                    executePayload = SerializeTestResultsToTRX(serviceName.Replace(TrxExtension, string.Empty), testResults, ref formatter);
                }

            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return formatter;
        }

        public static List<IServiceTestModelTO> RunSingleTestBatch(IDSFDataObject dataObject, string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, string testsResourceId = null)
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

                var lastTask = GetTaskForTestExecution(serviceName, userPrinciple, workspaceGuid,
                    serializer, testResults, dataObjectClone);
                taskList.Add(lastTask);
            }
            Task.WaitAll(taskList.ToArray());
            return testResults;
        }

        public static List<IServiceTestModelTO> RunMultipleTestBatches(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog, Guid testsResourceId)
        {
            var allTests = testCatalog.Fetch(testsResourceId);
            var taskList = new List<Task>();
            var testResults = new List<IServiceTestModelTO>();
            foreach (var test in allTests)
            {
                var dataObjectClone = dataObject.Clone();
                dataObjectClone.Environment = new ExecutionEnvironment();
                dataObjectClone.TestName = test.TestName;
                var res = catalog.GetResource(GlobalConstants.ServerWorkspaceID, testsResourceId);
                dataObjectClone.ServiceName = res.ResourceName;
                var resourcePath = res.GetResourcePath(GlobalConstants.ServerWorkspaceID).Replace("\\", "/");
                var lastTask = GetTaskForTestExecution(resourcePath, userPrinciple, workspaceGuid,
                    serializer, testResults, dataObjectClone);
                taskList.Add(lastTask);
            }
            Task.WaitAll(taskList.ToArray());
            return testResults;
        }

        public static DataListFormat RunMultipleTestBatchesAndReturnJSON(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid,
                                                                         Dev2JsonSerializer serializer, DataListFormat formatter,
                                                                         IResourceCatalog catalog, ITestCatalog testCatalog,
                                                                         ref string executePayload)
        {
            foreach (var testsResourceId in dataObject.TestsResourceIds)
            {
                List<IServiceTestModelTO> testResults = RunMultipleTestBatches(dataObject, userPrinciple, workspaceGuid, serializer, catalog, testCatalog, testsResourceId);
                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                var objArray = (from testRunResult in testResults
                                where testRunResult != null
                                select testRunResult.BuildTestResultJSONForWebRequest()
                                ).ToList();
                if (objArray.Count > 0)
                {
                    executePayload = (executePayload == string.Empty ? "[" : executePayload.TrimEnd("\r\n]".ToCharArray()) + ",") + serializer.Serialize(objArray).TrimStart('[');
                }
            }
            return formatter;
        }

        public static IEnumerable<JObject> SerializeTestResultsToJSON(string serviceName, List<IServiceTestModelTO> testResults, ref DataListFormat formatter)
        {
            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            return (from testRunResult in testResults
                    where testRunResult != null
                    select testRunResult.BuildTestResultJSONForWebRequest()
                    ).ToList();
        }

        public static string SerializeTestResultsToTRX(string serviceName, List<IServiceTestModelTO> testResults, ref DataListFormat formatter)
        {
            formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            return ServiceTestModelTRXResultBuilder.BuildTestResultTRX(serviceName, testResults);
        }
    }
}
