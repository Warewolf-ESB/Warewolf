using System.Activities;
using System.Activities.Hosting;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Hosting
{
    public class WorkflowInstanceInfo : IWorkflowInstanceExtension
    {
        public IEnumerable<object> GetAdditionalExtensions()
        {
            yield break;
        }

        public void SetInstance(WorkflowInstanceProxy instance)
        {
            Proxy = instance;
        }

        public WorkflowInstanceProxy Proxy { get; private set; }

        public string ProxyName
        {
            get
            {
                var dynamicActivity = Proxy.WorkflowDefinition as DynamicActivity;
                return dynamicActivity != null ? dynamicActivity.Name : Proxy.WorkflowDefinition.DisplayName;
            }
        }
    }
}