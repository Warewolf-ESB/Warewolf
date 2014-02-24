using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfMultiAssignUiMap : ActivityUiMapBase
    {
        public DsfMultiAssignUiMap(bool createNewtab = true, bool dragAssignOntoNewTab = true)
            : base(createNewtab)
        {
            if(dragAssignOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.Assign);
            }
        }

        public void ClickOpenLargeView()
        {
            UITestControl button = AdornersGetButton("Open Large View");
            Mouse.Click(button, new Point(5, 5));
        }

        public void ClickCloseLargeView()
        {
            UITestControl button = AdornersGetButton("Close Large View");
            Mouse.Click(button, new Point(5, 5));
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

        public WpfTable GetSmallViewTable()
        {
            UITestControl findContent = null;
            foreach(var child in Activity.GetChildren())
            {
                if(child.FriendlyName == "SmallViewContent")
                {
                    findContent = child;
                    break;
                }
            }
            if(findContent != null)
            {
                UITestControl findTable = null;
                var children = findContent.GetChildren();
                foreach(var child in children)
                {
                    if(child.FriendlyName == "SmallDataGrid")
                    {
                        findTable = child;
                        break;
                    }
                }
                if(findTable != null)
                {
                    WpfTable foundTable = (WpfTable)findTable;
                    return foundTable;
                }
            }
            throw new UITestControlNotFoundException("Cannot find specified control large view content");
        }

        public WpfTable GetLargeViewTable()
        {
            UITestControl findContent = null;
            foreach(var child in Activity.GetChildren())
            {
                if(child.FriendlyName == "LargeViewContent")
                {
                    findContent = child;
                    break;
                }
            }
            if(findContent != null)
            {
                UITestControl findTable = null;
                var children = findContent.GetChildren();
                foreach(var child in children)
                {
                    if(child.FriendlyName == "LargeDataGrid")
                    {
                        findTable = child;
                        break;
                    }
                }
                if(findTable != null)
                {
                    WpfTable foundTable = (WpfTable)findTable;
                    return foundTable;
                }
            }
            return null;
        }

    }
}
