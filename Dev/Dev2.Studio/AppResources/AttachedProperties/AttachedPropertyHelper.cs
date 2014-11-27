
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Views.Configuration;
using Dev2.Studio.Views.Deploy;
using Unlimited.Applications.BusinessDesignStudio;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class AttachedPropertyHelper
    {
        public static void SetAttachedProperties(WorkflowDesignerWindow workflowDesignerWindow,
                                                  IContextualResourceModel resource, string iconPath)
        {
            UIElementTitleProperty.SetTitle(workflowDesignerWindow, resource.ResourceName);
            UIElementTabActionContext.SetTabActionContext(workflowDesignerWindow, WorkSurfaceContext.Workflow);
            UIElementImageProperty.SetImage(workflowDesignerWindow, iconPath);
        }

        public static void SetAttachedProperties(DeployView view, string tabName)
        {
            UIElementTabActionContext.SetTabActionContext(view, WorkSurfaceContext.DeployResources);
            UIElementTitleProperty.SetTitle(view, tabName);
            UIElementImageProperty.SetImage(view, "/images/database_save.png");
        }

        public static void SetAttachedProperties(RuntimeConfigurationView view, string tabName)
        {
            UIElementTabActionContext.SetTabActionContext(view, WorkSurfaceContext.Settings);
            UIElementTitleProperty.SetTitle(view, tabName);
            UIElementImageProperty.SetImage(view, "/images/database_save.png");
        }
    }
}
