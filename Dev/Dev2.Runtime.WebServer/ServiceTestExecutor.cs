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
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
 using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Interfaces;
 using Dev2.Web;
 using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestExecutor
    {
        public static Task<IServiceTestModelTO> ExecuteTestAsync(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObjectClone)
        {
            var lastTask = Task<IServiceTestModelTO>.Factory.StartNew(() =>
            {
                var interTestRequest = new EsbExecuteRequest { ServiceName = serviceName };
                var dataObjectToUse = dataObjectClone;
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                    var esbEndpointClone = new EsbServicesEndpoint();
                    esbEndpointClone.ExecuteRequest(dataObjectToUse, interTestRequest, workspaceGuid, out _);
                });
                var result = serializer.Deserialize<ServiceTestModelTO>(interTestRequest.ExecuteResult);
                if (result == null && interTestRequest.ExecuteResult != null)
                {
                    var r = serializer.Deserialize<TestRunResult>(interTestRequest.ExecuteResult.ToString()) ?? new TestRunResult { TestName = dataObjectToUse.TestName };
                    result = new ServiceTestModelTO { Result = r, TestName = r.TestName };
                }

                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObjectToUse.DataListID);
                dataObjectToUse.Environment = null;
                return result;
            });
            lastTask.ConfigureAwait(true);
            return lastTask;
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

        public static DataListFormat ExecuteTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, IResourceCatalog resourceCatalog, out string executePayload)
        {
            executePayload = null;

            DataListFormat formatter = null;
            if (dataObject.TestsResourceIds?.Any() ?? false)
            {
                if (dataObject.ReturnType == EmitionTypes.TEST)
                {
                    formatter = dataObject.RunMultipleTestBatchesAndReturnJSON(userPrinciple, workspaceGuid, serializer, resourceCatalog, testCatalog, out executePayload);
                }
                if (dataObject.ReturnType == EmitionTypes.TRX)
                {
                    formatter = dataObject.RunMultipleTestBatchesAndReturnTRX(userPrinciple, workspaceGuid, serializer, resourceCatalog, testCatalog, out executePayload);
                }
                dataObject.ResourceID = Guid.Empty;
            }
            else
            {
                throw new Exception("do not expect this to be executed any longer");
            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return formatter ?? DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
        }
    }
}
