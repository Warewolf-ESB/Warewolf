using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common;
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
        readonly IWorkflowHelper _workflowHelper;

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObj, theWorkspace, esbChannel, new WorkflowHelper())
        {

        }

        // BUG 9304 - 2013.05.08 - TWR - Added IWorkflowHelper parameter to facilitate testing
        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IWorkflowHelper workflowHelper)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _workflowHelper = workflowHelper;
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            WorkflowApplicationFactory wfFactor = new WorkflowApplicationFactory();
            Guid result = GlobalConstants.NullDataListID;

            Guid instanceId = Guid.Empty;
            string bookmark = string.Empty;

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // PBI : 5376 Refactored 
            IBinaryDataListEntry tmp = compiler.Evaluate(DataObject.DataListID,
                                                             enActionType.System,
                                                             enSystemTag.Bookmark.ToString(), false, out errors);
            if(tmp != null)
            {
                bookmark = tmp.FetchScalar().TheValue;
            }

            tmp = compiler.Evaluate(DataObject.DataListID, enActionType.System, enSystemTag.InstanceId.ToString(), false, out errors);
            if(tmp != null)
            {
                Guid.TryParse(tmp.FetchScalar().TheValue, out instanceId);
            }

            // Set Service Name
            DataObject.ServiceName = ServiceAction.ServiceName;

            // Set server ID, only if not set yet - original server;
            if(DataObject.ServerID == Guid.Empty)
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;

            // Set resource ID, only if not set yet - original resource;
            if(DataObject.ResourceID == Guid.Empty && ServiceAction != null && ServiceAction.Service != null)
                DataObject.ResourceID = ServiceAction.Service.ID;

            // Travis : Now set Data List
            DataObject.DataList = ServiceAction.DataListSpecification;

            // Set original instance ID, only if not set yet - original resource;
            if(DataObject.OriginalInstanceID == Guid.Empty)
                DataObject.OriginalInstanceID = DataObject.DataListID;

            //Set execution origing
            if(!string.IsNullOrWhiteSpace(DataObject.ParentServiceName))
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                DataObject.ExecutionOriginDescription = DataObject.ParentServiceName;
            }
            else if(DataObject.IsDebug)
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
                                                                   new List<object> { EsbChannel, }, instanceId,
                                                                   TheWorkspace, bookmark, out errors);

                result = exeResult.DataListID;
            }
            catch(InvalidWorkflowException iwe)
            {
                var msg = iwe.Message;

                int start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);

                // trap the no start node error so we can replace it with something nicer ;)
                if(start > 0)
                {
                    errors.AddError(GlobalConstants.NoStartNodeError);
                }
                else
                {
                    errors.AddError(iwe.Message);
                }
            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }
            finally
            {
                ServiceAction.PushActivity(activity);
            }

            return result;
        }
    }
}
