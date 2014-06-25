using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.Hosting;
using System.Linq;
namespace Dev2.Runtime.ESB.WF
{
    public sealed class WfApplicationUtils
    {
        Action<DebugOutputBase, DebugItem> _add;

        public  WfApplicationUtils()
        {
            _add = AddDebugItem;
        }
        public  WfApplicationUtils(Func<IDataListCompiler> getDataListCompiler,Action<DebugOutputBase,DebugItem> add)
        {
            _getDataListCompiler = getDataListCompiler;
            _add = add;
        }
        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, out ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false)
        {
            errors = new ErrorResultTO();
            if(dataObject != null)
            {
                Guid parentInstanceID;
                Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);

                var debugState = new DebugState
                {
                    ID = dataObject.DataListID,
                    ParentID = parentInstanceID,
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
                    Server = string.Empty,
                    Version = string.Empty,
                    SessionID = dataObject.DebugSessionID,
                    EnvironmentID = dataObject.EnvironmentID,
                    ClientID = dataObject.ClientID,
                    Name = GetType().Name,
                    HasError = hasErrors,
                    ErrorMessage = existingErrors,


                };

                if(interrogateInputs)
                {
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                    ErrorResultTO invokeErrors;
                    var com = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    var defs = compiler.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ServiceName), enDev2ColumnArgumentDirection.Input);
                    var inputs = GetDebugValues(defs, com, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    debugState.Inputs.AddRange(inputs);
                }
                if(interrogateOutputs)
                {
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                    ErrorResultTO invokeErrors;
                    var com = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    var defs = compiler.GenerateDefsFromDataListForDebug(FindServiceShape(dataObject.WorkspaceID, dataObject.ServiceName), enDev2ColumnArgumentDirection.Output);
                    var inputs = GetDebugValues(defs, com, out invokeErrors);
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

                if(dataObject.IsDebugMode() || (dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer))
                {
                    if(debugState.StateType == StateType.End)
                    {
                        GetDebugDispatcher().Write(debugState, dataObject.RemoteInvoke, dataObject.RemoteInvokerID,dataObject.ParentInstanceID, dataObject.RemoteDebugItems);
                    }
                    else
                    {
                        GetDebugDispatcher().Write(debugState);
                    }
                }
            }
        }

        public Func<IDebugDispatcher> GetDebugDispatcher = () => DebugDispatcher.Instance;
        private readonly Func<IDataListCompiler> _getDataListCompiler = () => DataListFactory.CreateDataListCompiler();
       
        public List<DebugItem> GetDebugValues(IList<IDev2Definition> values, IBinaryDataList dataList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            IDataListCompiler compiler = _getDataListCompiler();
            var results = new List<DebugItem>();
            var added = new List<string>();
            foreach (IDev2Definition dev2Definition in values)
            {
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, GetVariableName(dev2Definition), false, out errors);
                GetValue(tmpEntry, dev2Definition);
             
               
                var defn = GetVariableName(dev2Definition);
                if (added.Any(a => a == defn))
                    continue;
                
                added.Add(defn);
                DebugItem itemToAdd = new DebugItem();
                _add(new DebugItemVariableParams(GetVariableName(dev2Definition), "", tmpEntry, dataList.UID), itemToAdd);
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }
        private static void GetValue(IBinaryDataListEntry tmpEntry, IDev2Definition defn)
        {

            if(String.IsNullOrEmpty(defn.RecordSetName))
            {
                tmpEntry.FetchScalar(); // ask trav what this side effect means
            }
            else
            {
                string error;
                tmpEntry.MakeRecordsetEvaluateReady(GlobalConstants.AllIndexes, GlobalConstants.AllColumns, out error);
            }
        }

        string GetVariableName(IDev2Definition value)
        {
            return String.IsNullOrEmpty(value.RecordSetName)
                  ? String.Format("[[{0}]]", value.Name)
                  : String.Format("[[{0}(){1}]]", value.RecordSetName, String.IsNullOrEmpty( value.Name)?String.Empty:"."+value.Name);
        }

        void AddDebugItem(DebugOutputBase parameters, DebugItem debugItem)
        {
            var debugItemResults = parameters.GetDebugItemResult();
            debugItem.AddRange(debugItemResults);
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceID, string serviceName)
        {
            var result = "<DataList></DataList>";
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);

            if(resource == null)
            {
                return result;
            }

            result = resource.DataList;



            if(string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="resourceID">The ID of the resource</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceID, Guid resourceID)
        {
            var result = "<DataList></DataList>";
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceID);

            if(resource == null)
            {
                return result;
            }

            result = resource.DataList;



            if(string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }
    }
}