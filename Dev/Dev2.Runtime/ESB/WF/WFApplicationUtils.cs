/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Binary_Objects;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;

// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Dev2.Runtime.ESB.WF
{
    public sealed class WfApplicationUtils
    {
        readonly Action<DebugOutputBase, DebugItem> _add;

        public WfApplicationUtils()
        {
            _add = AddDebugItem;
        }

        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, out ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false, bool durationVisible=true)
        {
            errors = new ErrorResultTO();
            if(dataObject != null)
            {
                var debugState = GetDebugState(dataObject, stateType, hasErrors, existingErrors, errors, workflowStartTime, interrogateInputs, interrogateOutputs, durationVisible);
                WriteDebug(dataObject, debugState);
            }
        }

        public DebugState GetDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, ErrorResultTO errors, DateTime? workflowStartTime, bool interrogateInputs, bool interrogateOutputs, bool durationVisible)
        {
            Guid parentInstanceId;
            Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceId);
            var hasError = dataObject.Environment.HasErrors();
            var errorMessage = string.Empty;
            if (hasError)
            {
                errorMessage = dataObject.Environment.FetchErrors();
            }
            if (string.IsNullOrEmpty(existingErrors))
            {
                existingErrors = errorMessage;
            }
            else if (!existingErrors.Contains(errorMessage))
            {
                existingErrors += Environment.NewLine + errorMessage;
            }
            var name = "localhost";
            Guid remoteID;
            var hasRemote = Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);
            if (hasRemote)
            {
                var res = _lazyCat.GetResource(GlobalConstants.ServerWorkspaceID, remoteID);
                if (res != null)
                    name = remoteID != Guid.Empty ? _lazyCat.GetResource(GlobalConstants.ServerWorkspaceID, remoteID).ResourceName : "localhost";
            }
            var debugState = BuildDebugState(dataObject, stateType, hasErrors, existingErrors, workflowStartTime, durationVisible, parentInstanceId, name, hasError);

            
            if (stateType == StateType.End)
            {
                debugState.StartTime = dataObject.StartTime;
                debugState.EndTime = DateTime.Now;
            }
            if(interrogateInputs)
            {
                ErrorResultTO invokeErrors;
                var defs = DataListUtil.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID), enDev2ColumnArgumentDirection.Input);
                var inputs = GetDebugValues(defs, dataObject, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                debugState.Inputs.AddRange(inputs);
            }
            if(interrogateOutputs)
            {
                ErrorResultTO invokeErrors;

                var defs = DataListUtil.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID), enDev2ColumnArgumentDirection.Output);
                var inputs = GetDebugValues(defs, dataObject, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                debugState.Outputs.AddRange(inputs);
            }
            if(stateType == StateType.End)
            {
                debugState.NumberOfSteps = dataObject.NumberOfSteps;
            }

            if(stateType == StateType.Start)
            {
                debugState.ExecutionOrigin = dataObject.ExecutionOrigin;
                debugState.ExecutionOriginDescription = dataObject.ExecutionOriginDescription;
            }
            return debugState;
        }

        public void WriteDebug(IDSFDataObject dataObject, DebugState debugState)
        {
            if(dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
            {
                var debugDispatcher = _getDebugDispatcher();
                if(debugState.StateType == StateType.End)
                {
                    debugDispatcher.Write(debugState, dataObject.IsServiceTestExecution, dataObject.TestName, dataObject.RemoteInvoke, dataObject.RemoteInvokerID, dataObject.ParentInstanceID, dataObject.RemoteDebugItems);
                }
                else
                {
                    debugDispatcher.Write(debugState, dataObject.IsServiceTestExecution, dataObject.TestName);
                }
            }
        }

        private static DebugState BuildDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, DateTime? workflowStartTime, bool durationVisible, Guid parentInstanceId, string name, bool hasError)
        {
            var debugState = new DebugState
            {
                ID = dataObject.OriginalInstanceID,
                ParentID = parentInstanceId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = stateType,
                StartTime = workflowStartTime ?? DateTime.Now,
                EndTime = DateTime.Now,
                ActivityType = ActivityType.Workflow,
                DisplayName = dataObject.ServiceName,
                IsSimulation = dataObject.IsOnDemandSimulation,
                ServerID = dataObject.ServerID,
                OriginatingResourceID = dataObject.ResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Server = name,
                Version = string.Empty,
                SessionID = dataObject.DebugSessionID,
                EnvironmentID = dataObject.DebugEnvironmentId,
                ClientID = dataObject.ClientID,
                SourceResourceID = dataObject.SourceResourceID,
                Name = stateType.ToString(),
                HasError = hasErrors || hasError,
                ErrorMessage = existingErrors,
                IsDurationVisible = durationVisible
            };
            return debugState;
        }

        private readonly Func<IDebugDispatcher> _getDebugDispatcher = () => DebugDispatcher.Instance;

        private List<DebugItem> GetDebugValues(IList<IDev2Definition> values, IDSFDataObject dataObject, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var results = new List<DebugItem>();
            var added = new List<string>();
            foreach(var dev2Definition in values)
            {
                var defn = GetVariableName(dev2Definition);
                if(added.Any(a => a == defn))
                    continue;

                added.Add(defn);
                var itemToAdd = new DebugItem();
                _add(new DebugEvalResult(DataListUtil.ReplaceRecordBlankWithStar(defn), "", dataObject.Environment, 0), itemToAdd); //todo:confirm 0
                results.Add(itemToAdd);
            }

            foreach(IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        private string GetVariableName(IDev2Definition value)
        {
            return string.IsNullOrEmpty(value.RecordSetName)
                  ? $"[[{value.Name}]]"
                : $"[[{value.RecordSetName}(){(string.IsNullOrEmpty(value.Name) ? string.Empty : "." + value.Name)}]]";
        }

        private void AddDebugItem(DebugOutputBase parameters, IDebugItem debugItem)
        {
            var debugItemResults = parameters.GetDebugItemResult();
            debugItem.AddRange(debugItemResults);
        }

        private readonly IResourceCatalog  _lazyCat =  ResourceCatalog.Instance;
        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceId">The ID of the resource</param>
        /// <returns></returns>
        private string FindServiceShape(Guid workspaceId, Guid resourceId)
        {
            const string EmptyDataList = "<DataList></DataList>";
            var resource = _lazyCat.GetResource(workspaceId, resourceId);

            if(resource == null)
            {
                return EmptyDataList;
            }

            var serviceShape = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote,"\"").ToString();
            serviceShape = serviceShape.Replace(GlobalConstants.SerializableResourceSingleQuote,"\'");
            return string.IsNullOrEmpty(serviceShape) ? EmptyDataList : serviceShape;
        }
    }
}
