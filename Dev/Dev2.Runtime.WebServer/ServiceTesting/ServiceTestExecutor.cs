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
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Interfaces;
using Dev2.Web;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Execution;
using Warewolf.Storage;

namespace Dev2.Runtime.WebServer
{
    internal class ServiceTestExecutor : IServiceTestExecutor
    {
        static string _serviceName;
        static IPrincipal _userPrinciple;
        static Guid _workspaceGuid;
        static Dev2JsonSerializer _serializer;
        static IDSFDataObject _dataObject;

        public ServiceTestExecutor(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObject)
        {
            _serviceName = serviceName;
            _userPrinciple = userPrinciple;
            _workspaceGuid = workspaceGuid;
            _serializer = serializer;
            _dataObject = dataObject;
        }
        
#pragma warning disable CC0074
        public Func<IServiceTestModelTO> ExecuteTestAsync = () =>
#pragma warning restore CC0074
        {
            var interTestRequest = new EsbExecuteRequest { ServiceName = _serviceName };
            var dataObjectToUse = _dataObject;
            Common.Utilities.PerformActionInsideImpersonatedContext(_userPrinciple, () =>
            {
                var esbEndpointClone = new EsbServicesEndpoint();
                esbEndpointClone.ExecuteRequest(dataObjectToUse, interTestRequest, _workspaceGuid, out _);
            });
            var result = _serializer.Deserialize<ServiceTestModelTO>(interTestRequest.ExecuteResult);
            if (result == null && interTestRequest.ExecuteResult != null)
            {
                var r = _serializer.Deserialize<TestRunResult>(interTestRequest.ExecuteResult.ToString()) ?? new TestRunResult { TestName = dataObjectToUse.TestName };
                result = new ServiceTestModelTO { Result = r, TestName = r.TestName };
            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObjectToUse.DataListID);
            dataObjectToUse.Environment = null;
            return result;
        };

        public Task<IServiceTestModelTO> ExecuteTestAsyncTask(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObjectClone)
        {
            var lastTask = Task<IServiceTestModelTO>.Factory.StartNew(ExecuteTestAsync);
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

        public DataListFormat ExecuteTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, IResourceCatalog resourceCatalog, out string executePayload, ITestCoverageCatalog testCoverageCatalog)
        {
            executePayload = null;

            DataListFormat formatter = null;
            if (dataObject.TestsResourceIds?.Any() ?? false)
            {
                if (dataObject.ReturnType == EmitionTypes.TEST)
                {
                    formatter = RunMultipleTestBatchesAndReturnJSON(resourceCatalog, testCatalog, out executePayload, testCoverageCatalog);
                }
                if (dataObject.ReturnType == EmitionTypes.TRX)
                {
                    formatter = RunMultipleTestBatchesAndReturnTRX(resourceCatalog, testCatalog, out executePayload, testCoverageCatalog);
                }
                dataObject.ResourceID = Guid.Empty;
            }
            else
            {
                Dev2.Common.Dev2Logger.Warn("No tests found to execute for requested resource", Dev2.Common.GlobalConstants.WarewolfWarn);
            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return formatter ?? DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
        }

        public DataListFormat RunMultipleTestBatchesAndReturnJSON(IResourceCatalog catalog, ITestCatalog testCatalog,
            out string executePayload, ITestCoverageCatalog testCoverageCatalog)
        {
            var testResults = RunListOfTests(_userPrinciple, _workspaceGuid, _serializer, catalog, testCatalog, testCoverageCatalog);
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

            executePayload = _serializer.Serialize(obj);
            return formatter;
        }

        public DataListFormat RunMultipleTestBatchesAndReturnTRX(IResourceCatalog catalog, ITestCatalog testCatalog,
            out string executePayload, ITestCoverageCatalog testCoverageCatalog)
        {
            var testResults = RunListOfTests(_userPrinciple, _workspaceGuid, _serializer, catalog, testCatalog, testCoverageCatalog);
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            executePayload = ServiceTestModelTRXResultBuilder.BuildTestResultTRX(_dataObject.ServiceName, testResults.Results.SelectMany(o => o.Results).ToList());
            return formatter;
        }

        TestResults RunListOfTests(IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog)
        {
            var result = new TestResults();

            var selectedResources = catalog.GetResources(workspaceGuid)
                ?.Where(resource => _dataObject.TestsResourceIds.Contains(resource.ResourceID)).ToArray();

            if (selectedResources != null)
            {
                var workflowTaskList = new List<Task<WorkflowTestResults>>();
                foreach (var testsResourceId in _dataObject.TestsResourceIds)
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
                                                                    let dataObjectClone = _dataObject.Clone()
                                                                    select (test, dataObjectClone))
                            {
                                dataObjectClone.Environment = new ExecutionEnvironment();
                                dataObjectClone.TestName = test.TestName;
                                dataObjectClone.ServiceName = res.ResourceName;
                                dataObjectClone.ResourceID = res.ResourceID;
                                var lastTask = ExecuteTestAsyncTask(resourcePath, userPrinciple, workspaceGuid,
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
    }
}
