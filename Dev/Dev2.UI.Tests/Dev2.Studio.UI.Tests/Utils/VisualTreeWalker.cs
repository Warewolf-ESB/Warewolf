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
        public static UITestControl GetChildByAutomationIDPath(UITestControl parent, int bookmark, params string[] automationIDs)
        {
            var children = parent.GetChildren();
            var firstChildFound = children.FirstOrDefault(child =>
                {
                    var childAutoID = child.GetProperty("AutomationID").ToString();
                    return childAutoID.Contains(automationIDs[bookmark]) || 
                        child.FriendlyName.Contains(automationIDs[bookmark]) || 
                        child.ControlType.Name.Contains(automationIDs[bookmark]) ||
                        child.ClassName.Contains(automationIDs[bookmark]);
                });
            if (firstChildFound == null)
            {
                throw new UITestControlNotFoundException("Cannot find " + automationIDs[bookmark] + " control within " + parent.GetProperty("AutomationID"));
            }
            if (bookmark == automationIDs.Count() - 1)
            {
                return firstChildFound;
            }
            return GetChildByAutomationIDPath(firstChildFound, ++bookmark, automationIDs);
        }

        public static UITestControl GetControl(params string[] automationIDs)
        {
            var studioWindow = new UIBusinessDesignStudioWindow();
            var control = GetChildByAutomationIDPath(studioWindow, 0, automationIDs);
            if (control == null)
            {
                throw new UITestControlNotFoundException();
            }
            return control;
        }
    }
}
