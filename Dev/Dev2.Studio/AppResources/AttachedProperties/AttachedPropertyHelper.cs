using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
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
    }
}
