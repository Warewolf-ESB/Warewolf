using System;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class WorkflowResume : WorkflowManagementEndpointAbstract
    {
        public override string HandlesType() => nameof(WorkflowResume);

        protected override CompressedExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId)
        {
            throw new NotImplementedException("WorkflowResume Service Not implemented");
        }

    }
}