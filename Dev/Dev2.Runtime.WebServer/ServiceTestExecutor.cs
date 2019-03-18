#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
    static class ServiceTestExecutor
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
                if (result == null && interTestRequest.ExecuteResult != null)
                {
                    var r = serializer.Deserialize<TestRunResult>(interTestRequest.ExecuteResult.ToString()) ?? new TestRunResult { TestName = dataObjectToUse.TestName };
                    result = new ServiceTestModelTO { Result = r, TestName = r.TestName };
                }

                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObjectToUse.DataListID);
                dataObjectToUse.Environment = null;
                testResults.Add(result);
            });
            await lastTask.ConfigureAwait(true);
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
            if (dataObject.TestsResourceIds?.Any() ?? false)
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
                if (dataObject.ReturnType == EmitionTypes.TEST)
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
