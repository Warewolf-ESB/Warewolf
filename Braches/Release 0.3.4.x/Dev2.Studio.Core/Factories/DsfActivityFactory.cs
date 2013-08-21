using Dev2.Studio.Core.Activities.Interegators;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Factories
{
    public static class DsfActivityFactory
    {
        public static DsfActivity CreateDsfActivity(IContextualResourceModel resource, DsfActivity activity, bool ifNullCreateNew)
        {
            if(activity == null)
            {
                if(ifNullCreateNew)
                {
                    activity = new DsfActivity();
                }
                else
                {
                    return null;
                }
            }

            if(resource != null)
            {
                // PBI 9135 - 2013.07.15 - TWR - Added
                activity.ResourceID = resource.ID;
                if(resource.Environment != null)
                {
                    activity.EnvironmentID = resource.Environment.ID;
                }

                if(resource.ResourceType == ResourceType.WorkflowService)
                {
                    activity.IsWorkflow = true;
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    activity.FriendlySourceName = resource.Environment.Name;
                    activity = WorkflowPropertyInterigator.SetActivityProperties(resource.ServiceDefinition, activity);
                }
                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                else if(resource.ResourceType == ResourceType.Service)
                {
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    activity.IsWorkflow = false;

                    activity = WorkerServicePropertyInterigator.SetActivityProperties(resource.ServiceDefinition, activity);
                }
            }

            activity.ExplicitDataList = null;
            return activity;
        }
    }
}
