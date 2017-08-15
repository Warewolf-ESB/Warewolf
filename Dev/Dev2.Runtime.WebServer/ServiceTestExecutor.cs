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
                    ErrorResultTO errs;
                    esbEndpointClone.ExecuteRequest(dataObjectToUse, interTestRequest, workspaceGuid, out errs);
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
            await lastTask;
        }

        public static string SetupForTestExecution(Dev2JsonSerializer serializer, EsbExecuteRequest esbExecuteRequest,IDSFDataObject dataObject)
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
            if (dataObject.TestsResourceIds?.Any() ?? false && serviceName == "*")
            {
                if (dataObject.ReturnType == Web.EmitionTypes.TEST)
                {
                    formatter = dataObject.RunMultipleTestBatchesAndReturnJSON(userPrinciple, workspaceGuid, serializer, formatter,
                    resourceCatalog, testCatalog, ref executePayload);
                }
                if (dataObject.ReturnType == Web.EmitionTypes.TRX)
                {
                    formatter = dataObject.RunMultipleTestBatchesAndReturnTRX(userPrinciple, workspaceGuid, serializer, formatter,
                    resourceCatalog, testCatalog, ref executePayload);
                }
                dataObject.ResourceID = Guid.Empty;
            }
            else
            {
                if (dataObject.ReturnType == Web.EmitionTypes.TEST)
                {
                    formatter = dataObject.RunSingleTestBatchAndReturnJSON(userPrinciple, workspaceGuid, serializer, formatter,
                        serviceName, testCatalog, ref executePayload);
                }
                if (dataObject.ReturnType == Web.EmitionTypes.TRX)
                {
                    formatter = dataObject.RunSingleTestBatchAndReturnTRX(userPrinciple, workspaceGuid, serializer, formatter,
                        serviceName, testCatalog, ref executePayload);
                }

            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return formatter;
        }
    }
}
