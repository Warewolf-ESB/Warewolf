using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using Dev2.Activities.Debug;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System.Linq;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.ESB.WF
{
    public sealed class WfApplicationUtils
    {

       
        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false)
        {
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
                    HasError = errors.HasErrors(),
                    ErrorMessage = errors.MakeDisplayReady(),
                   
                    
                };

                if (interrogateInputs)
                {
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                    var com = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
                    var defs = compiler.GenerateDefsFromDataList(FindServiceShape(dataObject.WorkspaceID, dataObject.ServiceName), enDev2ColumnArgumentDirection.Input);
                    var inputs = GetDebugInputs(defs, com, errors);
                    debugState.Inputs.AddRange(inputs);
                }
                if (interrogateOutputs)
                {
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                    var com = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
                    var defs = compiler.GenerateDefsFromDataList(FindServiceShape(dataObject.WorkspaceID,dataObject.ServiceName), enDev2ColumnArgumentDirection.Output);
                    var inputs = GetDebugInputs(defs, com, errors);
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

                if(dataObject.IsDebugMode())
                {
                    if (debugState.StateType == StateType.End)
                    {
                        DebugDispatcher.Instance.Write(debugState, dataObject.RemoteInvoke, dataObject.RemoteInvokerID);
                    }
                    else
                    {
                        DebugDispatcher.Instance.Write(debugState);
                    }
                }
            }
        }

        public List<DebugItem> GetDebugInputs(IList<IDev2Definition> inputs, IBinaryDataList dataList, ErrorResultTO errors)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var results = new List<DebugItem>();
            foreach (IDev2Definition dev2Definition in inputs)
            {

                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, GetVariableName(dev2Definition), false, out errors);

                GetValue(tmpEntry,dev2Definition);

                DebugItem itemToAdd = new DebugItem();
                AddDebugItem(new DebugItemVariableParams(GetVariableName(dev2Definition), "", tmpEntry, dataList.UID), itemToAdd);
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        private static void GetValue(IBinaryDataListEntry tmpEntry,IDev2Definition defn)
        {
         
            if (String.IsNullOrEmpty(defn.RecordSetName))
            {
                tmpEntry.FetchScalar(); // ask trav what this side effect means
            }
            else
            {
               var a =  tmpEntry.IsRecordset;
            }

        }

        string GetVariableName(IDev2Definition value)
        {
            return String.IsNullOrEmpty(value.RecordSetName)
                  ? String.Format("[[{0}]]", value.Name)
                  : String.Format("[[{0}()]]", value.RecordSetName);
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

            if (resource == null)
            {
                return result;
            }

            result = resource.DataList;



            if (string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }
    }
}