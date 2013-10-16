using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    class VisualTreeWalker
    {
        private static UITestControl GetChildByAutomationID(UITestControl parent, int bookmark, params string[] automationIDs)
        {
            var children = parent.GetChildren();
            var firstChildFound = children.FirstOrDefault(child =>
                {
                    var childAutoID = child.GetProperty("AutomationID").ToString();
                    return childAutoID == automationIDs[bookmark];
                });
            if (firstChildFound == null)
            {
                throw new UITestControlNotFoundException("Cannot find " + automationIDs[bookmark] + " control within " + parent.GetProperty("AutomationID"));
            }
            if (bookmark == automationIDs.Count() - 1)
            {
                return firstChildFound;
            }
            return GetChildByAutomationID(firstChildFound, ++bookmark, automationIDs);
        }

        public static UITestControl GetControl(params string[] automationIDs)
        {
            var studioWindow = new UIBusinessDesignStudioWindow();
            var control = GetChildByAutomationID(studioWindow, 0, automationIDs);
            if (control == null)
            {
                throw new UITestControlNotFoundException();
            }
            return control;
        }
    }
}
