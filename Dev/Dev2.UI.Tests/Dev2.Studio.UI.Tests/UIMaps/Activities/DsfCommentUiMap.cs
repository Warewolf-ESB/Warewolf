using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfCommentUiMap:ToolsUiMapBase
    {
        public DsfCommentUiMap(bool createNewtab = true, bool dragAssignOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragAssignOntoNewTab)
            {                
                DragToolOntoDesigner(ToolType.Comment);
            }
        }

        public void EnterTextIntoComment(string textToEnter)
        {
            VisualTreeWalker vtw = new VisualTreeWalker();
            UITestControl textbox = vtw.GetChildByAutomationIDPath(Activity, "SmallViewContent", "InitialFocusElement");
            textbox.EnterText(textToEnter);
        }
    }
}
