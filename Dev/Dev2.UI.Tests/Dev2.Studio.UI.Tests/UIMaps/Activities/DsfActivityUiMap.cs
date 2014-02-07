using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Studio.UI.Tests.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfActivityUiMap : ActivityUiMapBase
    {

        #region Public Methods
        public void DragToolOntoDesigner(ToolType toolType, Point pointToDragTo = new Point())
        {
            if(toolType == ToolType.Workflow || toolType == ToolType.Service)
            {
                ToolboxUIMap.DragControlToWorkflowDesigner(toolType.GetDescription(), TheTab, new Point(), false);
                PopupDialogUIMap.WaitForDialog();
            }
            else
            {
                Activity = ToolboxUIMap.DragControlToWorkflowDesigner(toolType.GetDescription(), TheTab);
            }
        }

        public void DragWorkflowOntoDesigner(string workflowName, string categoryName, string serverName = "localhost", Point pointToDragTo = new Point())
        {
            ExplorerUIMap.EnterExplorerSearchText(workflowName);
            Activity = ExplorerUIMap.DragResourceOntoWorkflowDesigner(TheTab, workflowName, categoryName, ServiceType.Workflows, serverName, pointToDragTo);
        }

        public void DragServiceOntoDesigner(string serviceName, string categoryName, string serverName = "localhost", Point pointToDragTo = new Point())
        {
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Activity = ExplorerUIMap.DragResourceOntoWorkflowDesigner(TheTab, serviceName, categoryName, ServiceType.Services, serverName, pointToDragTo);
        }

        public void ClickEdit()
        {
            UITestControl button = AdornersGetButton("Edit");
            Mouse.Click(button, new Point(5, 5));
            Playback.Wait(2000);
        }

        public void ClickOpenMapping()
        {
            UITestControl button = AdornersGetButton("Open Mapping");
            Mouse.Click(button, new Point(5, 5));
        }

        public void ClickCloseMapping()
        {
            UITestControl button = AdornersGetButton("Close Mapping");
            Mouse.Click(button, new Point(5, 5));
        }

        public bool IsFixErrorButtonShowing()
        {
            UITestControl fixErrorsButton = GetFixErrorsButton();
            if(fixErrorsButton == null)
            {
                return false;
            }
            return true;
        }

        public void ClickFixErrors()
        {
            UITestControl fixErrorsButton = GetFixErrorsButton();
            if(fixErrorsButton != null)
            {
                Mouse.Click(fixErrorsButton, new Point(5, 5));
            }
        }

        #endregion

        UITestControl GetFixErrorsButton()
        {
            List<UITestControl> buttons = Activity.GetChildren().Where(c => c.ControlType == ControlType.Button).ToList();
            if(buttons.Any())
            {
                return buttons[0];
            }
            return null;
        }

    }
}
