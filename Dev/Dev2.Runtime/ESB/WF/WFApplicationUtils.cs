#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;

namespace Dev2.Runtime.ESB.WF
{
    public sealed class WfApplicationUtils
    {
        readonly IResourceCatalog _lazyCat;

        public WfApplicationUtils()
            :this(ResourceCatalog.Instance)
        {
        }

        public WfApplicationUtils(IResourceCatalog resourceCatalog)
        {
            _lazyCat = resourceCatalog;
        }

#pragma warning disable S2360 // Optional parameters should not be used, unless they are only used in the same assembly
        internal void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, out ErrorResultTO errors, bool interrogateInputs = false, bool interrogateOutputs = true, bool durationVisible = true)
#pragma warning restore S2360 // Optional parameters should not be used
        {
            errors = new ErrorResultTO();
            if (dataObject != null)
            {
                var debugState = GetDebugState(dataObject, stateType, errors, interrogateInputs, interrogateOutputs, durationVisible);
                TryWriteDebug(dataObject, debugState);
            }
        }

#pragma warning disable S2360 // Optional parameters should not be used, unless they are only used in the same assembly
        internal DebugState GetDebugState(IDSFDataObject dataObject, StateType stateType, ErrorResultTO errors, bool interrogateInputs = false, bool interrogateOutputs = false, bool durationVisible = false)
#pragma warning restore S2360 // Optional parameters should not be used
        {
            var errorMessage = string.Empty;
            if (dataObject.Environment.HasErrors())
            {
                errorMessage = dataObject.Environment.FetchErrors();
            }

            var server = "localhost";
            var hasRemote = Guid.TryParse(dataObject.RemoteInvokerID, out var remoteID);
            if (hasRemote)
            {
                var res = _lazyCat.GetResource(GlobalConstants.ServerWorkspaceID, remoteID);
                if (res != null)
                {
                    server = remoteID != Guid.Empty ? _lazyCat.GetResource(GlobalConstants.ServerWorkspaceID, remoteID).ResourceName : "localhost";
                }
            }

            Guid.TryParse(dataObject.ParentInstanceID, out var parentInstanceId);

            var debugState = new DebugState
            {
                ID = dataObject.OriginalInstanceID,
                ParentID = parentInstanceId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = stateType,
                StartTime = dataObject.StartTime,
                EndTime = DateTime.Now,
                ActivityType = ActivityType.Workflow,
                DisplayName = dataObject.ServiceName,
                IsSimulation = dataObject.IsOnDemandSimulation,
                ServerID = dataObject.ServerID,
                OriginatingResourceID = dataObject.ResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Server = server,
                Version = string.Empty,
                SessionID = dataObject.DebugSessionID,
                EnvironmentID = dataObject.DebugEnvironmentId,
                ClientID = dataObject.ClientID,
                SourceResourceID = dataObject.SourceResourceID,
                Name = stateType.ToString(),
                HasError = dataObject.Environment.HasErrors(),
                ErrorMessage = errorMessage,
                IsDurationVisible = durationVisible
            };

            if (stateType == StateType.End)
            {
                debugState.NumberOfSteps = dataObject.NumberOfSteps;
            }
            if (stateType == StateType.Start)
            {
                debugState.ExecutionOrigin = dataObject.ExecutionOrigin;
                debugState.ExecutionOriginDescription = dataObject.ExecutionOriginDescription;
            }

            if (interrogateInputs)
            {
                var defs = DataListUtil.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID), enDev2ColumnArgumentDirection.Input);
                var inputs = GetDebugValues(defs, dataObject, out var invokeErrors);
                errors.MergeErrors(invokeErrors);
                debugState.Inputs.AddRange(inputs);
            }
            if (interrogateOutputs)
            {
                var defs = DataListUtil.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID), enDev2ColumnArgumentDirection.Output);
                var outputs = GetDebugValues(defs, dataObject, out var invokeErrors);
                errors.MergeErrors(invokeErrors);
                debugState.Outputs.AddRange(outputs);
            }

            return debugState;
        }

        public void TryWriteDebug(IDSFDataObject dataObject, DebugState debugState)
        {
            if (dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
            {
                var debugDispatcher = _getDebugDispatcher();
                if (debugState.StateType == StateType.End)
                {
                    var writeEndArgs = new WriteArgs
                    {
                        debugState = debugState,
                        isTestExecution = dataObject.IsServiceTestExecution,
                        isDebugFromWeb = dataObject.IsDebugFromWeb,
                        testName = dataObject.TestName,
                        isRemoteInvoke = dataObject.RemoteInvoke,
                        remoteInvokerId = dataObject.RemoteInvokerID,
                        parentInstanceId = dataObject.ParentInstanceID,
                        remoteDebugItems = dataObject.RemoteDebugItems
                    };
                    debugDispatcher.Write(writeEndArgs);

                    var dataObjectExecutionId = dataObject.ExecutionID.ToString();
                    try
                    {
                        WriteDebug(dataObject, dataObjectExecutionId);
                    }
                    catch (Exception)
                    {
                        Dev2Logger.Debug("Error getting execution result for :" + dataObject.ResourceID, dataObjectExecutionId);
                    }
                }
                else
                {
                    var writeArgs = new WriteArgs
                    {
                        debugState = debugState,
                        isTestExecution = dataObject.IsServiceTestExecution,
                        isDebugFromWeb = dataObject.IsDebugFromWeb,
                        testName = dataObject.TestName
                    };
                    debugDispatcher.Write(writeArgs);
                }
            }
        }

        private void WriteDebug(IDSFDataObject dataObject, string dataObjectExecutionId)
        {
            var resource = _lazyCat.GetResource(GlobalConstants.ServerWorkspaceID, dataObject.ResourceID);
            var executePayload = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
            var executionLogginResultString = GlobalConstants.ExecutionLoggingResultStartTag + (executePayload ?? "").Replace(Environment.NewLine, string.Empty) + GlobalConstants.ExecutionLoggingResultEndTag;
            if (dataObject.Environment.HasErrors())
            {
                Dev2Logger.Error(executionLogginResultString, dataObjectExecutionId);
            }
            else
            {
                Dev2Logger.Debug(executionLogginResultString, dataObjectExecutionId);
            }
        }

        readonly Func<IDebugDispatcher> _getDebugDispatcher = () => DebugDispatcher.Instance;

        List<DebugItem> GetDebugValues(IList<IDev2Definition> values, IDSFDataObject dataObject, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var results = new List<DebugItem>();
            var added = new List<string>();
            foreach (var dev2Definition in values)
            {
                var defn = GetVariableName(dev2Definition);
                if (added.Any(a => a == defn))
                {
                    continue;
                }

                added.Add(defn);
                var itemToAdd = new DebugItem();

                AddDebugItem(new DebugEvalResult(DataListUtil.ReplaceRecordBlankWithStar(defn), "", dataObject.Environment, 0), itemToAdd); // TODO: confirm why 0 is hardcoded for the execution update state?
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        static string GetVariableName(IDev2Definition value)
        {
            string variableName;
            if (value.IsJsonArray && value.Name.StartsWith("@", StringComparison.Ordinal))
            {
                variableName = $"[[{value.Name}()]]";
            }
            else if (string.IsNullOrEmpty(value.RecordSetName))
            {
                variableName = $"[[{value.Name}]]";
            }
            else
            {
                variableName = $"[[{value.RecordSetName}(){(string.IsNullOrEmpty(value.Name) ? string.Empty : "." + value.Name)}]]";
            }
            return variableName;
        }

        static void AddDebugItem(DebugOutputBase parameters, IDebugItem debugItem)
        {
            var debugItemResults = parameters.GetDebugItemResult();
            debugItem.AddRange(debugItemResults);
        }

        string FindServiceShape(Guid workspaceId, Guid resourceId)
        {
            const string EmptyDataList = "<DataList></DataList>";
            var resource = _lazyCat.GetResource(workspaceId, resourceId);

            if (resource == null)
            {
                return EmptyDataList;
            }

            var serviceShape = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").ToString();
            serviceShape = serviceShape.Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            return string.IsNullOrEmpty(serviceShape) ? EmptyDataList : serviceShape;
        }
    }
}
