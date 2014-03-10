using System.Linq;
using Dev2.Studio.UI.Tests.Enums;
using System.Drawing;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfDataMergeUiMap : ToolsUiMapBase
    {
        public DsfDataMergeUiMap(bool createNewtab = true, bool dragOnTab = true, Point dragPoint = new Point())
            : base(createNewtab, 1500)
        {
            if(dragOnTab)
            {
                DragToolOntoDesigner(ToolType.DataMerge, dragPoint);
            }
        }

        public string GetVariable(int index)
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
            return "";
        }
    }
}
