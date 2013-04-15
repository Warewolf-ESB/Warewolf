using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using System;
using Dev2.Workspaces;
using Dev2.Common;

namespace Dev2.Runtime.ESB.Execution
{
    public class WfExecutionContainer : EsbExecutionContainer
    {

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            
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
                                                             DataList.Contract.enActionType.System,
                                                             enSystemTag.Bookmark.ToString(), false, out errors);
            if (tmp != null)
            {
                bookmark = tmp.FetchScalar().TheValue;
            }

            tmp = compiler.Evaluate(DataObject.DataListID, DataList.Contract.enActionType.System,
                                        enSystemTag.InstanceId.ToString(), false, out errors);
            if (tmp != null)
            {
                Guid.TryParse(tmp.FetchScalar().TheValue, out instanceId);
            }       

            // Set Service Name
            DataObject.ServiceName = ServiceAction.ServiceName;

            // Travis : Now set Data List
            DataObject.DataList = ServiceAction.DataListSpecification;

            PooledServiceActivity activity = ServiceAction.PopActivity();

            try
            {
                IDSFDataObject exeResult = wfFactor.InvokeWorkflow(activity.Value, DataObject, new List<object> { EsbChannel, }, instanceId, TheWorkspace, bookmark, out errors);

                result = exeResult.DataListID;
            }
            catch (Exception ex)
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
