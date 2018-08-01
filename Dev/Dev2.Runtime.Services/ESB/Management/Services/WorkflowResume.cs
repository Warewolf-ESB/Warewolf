using System;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class WorkflowResume : WorkflowManagementEndpointAbstract
    {
        public override string HandlesType() => nameof(WorkflowResume);

        protected override ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId)
        {
            if (!Resumable(resourceId))
            {
                return new ExecuteMessage { HasError = true, Message = new System.Text.StringBuilder("this workflow is  not resumable") };
            }
            return new ExecuteMessage { HasError = false, Message = new System.Text.StringBuilder("workflow resume is not implemented") };
        }

        /// <summary>
        /// TODO:
        /// return false if the resource does not have a failed state in the audit logs
        /// return false if the resource is not found
        /// return false if the resource does not contain the
        ///      tool id from the previous execution.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>

        bool Resumable(Guid resourceId)
        {
            if (resourceId != null)
            {
                return true;
            }
            //var logEntriesJson = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
            // || a.WorkflowName.Equals("LogExecuteCompleteState")
            // || a.ExecutionID.Equals("")
            // || a.AuditType.Equals("")))
            return false;
        }

    }
}
