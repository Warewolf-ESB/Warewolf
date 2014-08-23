using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Linq;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfMultiAssignUiMap : ToolsUiMapBase
    {
        public DsfMultiAssignUiMap(bool createNewtab = true, bool dragAssignOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragAssignOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.Assign);
            }
        }

        public void DragToolOntoDesigner()
        {
            Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, TheTab);
        }

        public void EnterTextIntoVariable(int index, string stringToEnter)
        {
            WpfTable table = GetLargeViewTable();
            if(table == null)
            {
                table = GetSmallViewTable();
            }
            UITestControl row = table.Rows[index];
            UITestControl cell = row.GetChildren()[2];
            UITestControlCollection uiTestControlCollection = cell.GetChildren();
            UITestControl textbox = uiTestControlCollection.FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textbox != null)
            {
                textbox.EnterText(stringToEnter);
            }
        }



        public void EnterTextIntoValue(int index, string stringToEnter)
        {
            WpfTable table = GetLargeViewTable();
            if(table == null)
            {
                table = GetSmallViewTable();
            }
            UITestControl row = table.Rows[index];
            UITestControl cell = row.GetChildren()[4];
            UITestControlCollection uiTestControlCollection = cell.GetChildren();
            UITestControl textbox = uiTestControlCollection.FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textbox != null)
            {
                textbox.EnterText(stringToEnter);
            }
        }

        public string GetTextFromVariable(int index)
        {
            WpfTable table = GetLargeViewTable();
            if(table == null)
            {
                table = GetSmallViewTable();
            }
            UITestControl row = table.Rows[index];
            UITestControl cell = row.GetChildren()[2];
            UITestControlCollection uiTestControlCollection = cell.GetChildren();
            UITestControl textbox = uiTestControlCollection.FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textbox != null)
            {
                return textbox.GetText();
            }
            return null;
        }

        public string GetTextFromValue(int index)
        {
            WpfTable table = GetLargeViewTable();
            if(table == null)
            {
                table = GetSmallViewTable();
            }
            UITestControl row = table.Rows[index];
            UITestControl cell = row.GetChildren()[4];
            UITestControlCollection uiTestControlCollection = cell.GetChildren();
            UITestControl textbox = uiTestControlCollection.FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textbox != null)
            {
                return textbox.GetText();
            }
            return null;
        }
    }
}
