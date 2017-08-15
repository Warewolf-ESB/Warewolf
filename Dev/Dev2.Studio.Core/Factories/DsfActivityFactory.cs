/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities;
using Dev2.Studio.Core.Activities.Interegators;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Studio.Core.Factories

{
    public static class DsfActivityFactory
    {
        public static DsfActivity CreateDsfActivity(IContextualResourceModel resource, DsfActivity activity,
                    bool ifNullCreateNew, IServerRepository serverRepository, bool isDesignerLocalhost)
        {
            var activityToUpdate = activity;
            if(activityToUpdate == null)
            {
                if(ifNullCreateNew)
                {
                    if (resource.ServerResourceType == "WebService")
                    {
                        activityToUpdate = new DsfWebserviceActivity();
                    }
                    else if (resource.ServerResourceType == "PluginService")
                    {
                        activityToUpdate = new DsfPluginActivity();
                    }
                    else if (resource.ServerResourceType == "DbService")
                    {
                        activityToUpdate = new DsfDatabaseActivity();
                    }
                    else
                    {
                        activityToUpdate = new DsfActivity();
                    }
                }
                else
                {
                    return null;
                }
            }

            if(resource != null)
            {
                var activeEnvironment = serverRepository.ActiveServer;
                activityToUpdate.ResourceID = resource.ID;
                SetCorrectEnvironmentId(resource, activityToUpdate, isDesignerLocalhost, activeEnvironment);
                activityToUpdate = SetActivityProperties(resource, activityToUpdate);
            }

            activityToUpdate.ExplicitDataList = null;
            return activityToUpdate;
        }

        static void SetCorrectEnvironmentId(IContextualResourceModel resource, DsfActivity activity, bool isDesignerLocalhost, IServer activeEnvironment)
        {
            if(resource.Environment != null)
            {
                var idToUse = resource.Environment.EnvironmentID;

                //// when we have an active remote environment that we are designing against, set it as local to that environment ;)
                if(activeEnvironment.EnvironmentID == resource.Environment.EnvironmentID && idToUse != Guid.Empty && !isDesignerLocalhost)
                {
                    idToUse = Guid.Empty;
                }

                activity.EnvironmentID = idToUse;
            }
        }

        static DsfActivity SetActivityProperties(IContextualResourceModel resource, DsfActivity activity)
        {
            
            switch(resource.ResourceType)
            {
                case ResourceType.WorkflowService:
                    WorkflowPropertyInterigator.SetActivityProperties(resource, ref activity);
                    break;
                case ResourceType.Service:
                    WorkerServicePropertyInterigator.SetActivityProperties(resource, ref activity,resource.Environment.ResourceRepository);
                    break;
            }
            return activity;
        }
    }
}
