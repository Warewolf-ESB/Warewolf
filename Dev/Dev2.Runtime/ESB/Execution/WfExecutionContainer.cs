/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Security;
using Dev2.Utilities;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class WfExecutionContainer : EsbExecutionContainer
    {
        private readonly IWorkflowHelper _workflowHelper;

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace,
            IEsbChannel esbChannel)
            : this(sa, dataObj, theWorkspace, esbChannel, new WorkflowHelper())
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added IWorkflowHelper parameter to facilitate testing
        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace,
            IEsbChannel esbChannel, IWorkflowHelper workflowHelper)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _workflowHelper = workflowHelper;
        }

        /// <summary>
        ///     Executes the specified errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var wfFactor = new WorkflowApplicationFactory();
            Guid result = GlobalConstants.NullDataListID;

            // set current bookmark as per DataObject ;)
            string bookmark = DataObject.CurrentBookmarkName;
            Guid instanceId = DataObject.WorkflowInstanceId;

            // Set Service Name
            DataObject.ServiceName = ServiceAction.ServiceName;

            // Set server ID, only if not set yet - original server;
            if (DataObject.ServerID == Guid.Empty)
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;

            // Set resource ID, only if not set yet - original resource;
            if (DataObject.ResourceID == Guid.Empty && ServiceAction != null && ServiceAction.Service != null)
                DataObject.ResourceID = ServiceAction.Service.ID;

            // Travis : Now set Data List
            DataObject.DataList = ServiceAction.DataListSpecification;
            // Set original instance ID, only if not set yet - original resource;
            if (DataObject.OriginalInstanceID == Guid.Empty)
                DataObject.OriginalInstanceID = DataObject.DataListID;
            Dev2Logger.Log.Info(String.Format("Started Execution for Service Name:{0} Resource Id:{1} Mode:{2}",
                DataObject.ServiceName, DataObject.ResourceID, DataObject.IsDebug ? "Debug" : "Execute"));
            //Set execution origin
            if (!string.IsNullOrWhiteSpace(DataObject.ParentServiceName))
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                DataObject.ExecutionOriginDescription = DataObject.ParentServiceName;
            }
            else if (DataObject.IsDebug)
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            }
            else
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.External;
            }

            PooledServiceActivity activity = ServiceAction.PopActivity();

            try
            {
                var theActivity = activity.Value as DynamicActivity;

                // BUG 9304 - 2013.05.08 - TWR - Added CompileExpressions
                _workflowHelper.CompileExpressions(theActivity);

                IDSFDataObject exeResult = wfFactor.InvokeWorkflow(activity.Value, DataObject,
                    new List<object> {EsbChannel,}, instanceId,
                    TheWorkspace, bookmark, out errors);

                result = exeResult.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Log.Error(iwe);
                string msg = iwe.Message;

                int start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);

                // trap the no start node error so we can replace it with something nicer ;)
                errors.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                errors.AddError(ex.Message);
            }
            finally
            {
                ServiceAction.PushActivity(activity);
            }
            Dev2Logger.Log.Info(String.Format("Completed Execution for Service Name:{0} Resource Id: {1} Mode:{2}",
                DataObject.ServiceName, DataObject.ResourceID, DataObject.IsDebug ? "Debug" : "Execute"));

            return result;
        }

        public List<DebugItem> GetDebugInputs(IList<IDev2Definition> inputs, IBinaryDataList dataList,
            ErrorResultTO errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException("errors");
            }

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var results = new List<DebugItem>();
            foreach (IDev2Definition dev2Definition in inputs)
            {
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User,
                    GetVariableName(dev2Definition), false, out errors);

                IBinaryDataListItem val = tmpEntry.FetchScalar();

                val.TheValue += "";

                var itemToAdd = new DebugItem();
                AddDebugItem(new DebugItemVariableParams(GetVariableName(dev2Definition), "", tmpEntry, dataList.UID),
                    itemToAdd);
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        private string GetVariableName(IDev2Definition value)
        {
            return String.IsNullOrEmpty(value.RecordSetName)
                ? String.Format("[[{0}]]", value.Name)
                : String.Format("[[{0}]]", value.RecordSetName);
        }

        private void AddDebugItem(DebugOutputBase parameters, IDebugItem debugItem)
        {
            List<IDebugItemResult> debugItemResults = parameters.GetDebugItemResult();
            debugItem.AddRange(debugItemResults);
        }
    }
}