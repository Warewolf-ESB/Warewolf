using System;
using Dev2.Studio.Core.Activities.Interegators;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Factories
// ReSharper restore CheckNamespace
{
    public static class DsfActivityFactory
    {
        public static DsfActivity CreateDsfActivity(IContextualResourceModel resource, DsfActivity activity, bool ifNullCreateNew, IEnvironmentRepository environmentRepository, bool isDesignerLocalhost)
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
                var activeEnvironment = environmentRepository.ActiveEnvironment;
                // PBI 9135 - 2013.07.15 - TWR - Added
                activity.ResourceID = resource.ID;
                if(resource.Environment != null)
                {
                    var idToUse = resource.Environment.ID;

                    // when we have an active remote environment that we are designing against, set it as local to that environment ;)
                    if(activeEnvironment.ID == resource.Environment.ID && idToUse != Guid.Empty && !isDesignerLocalhost)
                    {
                        idToUse = Guid.Empty;
                    }

                    activity.EnvironmentID = idToUse;
                }

                if(resource.ResourceType == ResourceType.WorkflowService)
                {
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);
                }
                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                else if(resource.ResourceType == ResourceType.Service)
                {
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity);
                }
            }

            activity.ExplicitDataList = null;
            return activity;
        }
    }
}
