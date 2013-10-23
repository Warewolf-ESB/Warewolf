using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    class VisualTreeWalker
    {
        static UIMapBase.UIStudioWindow _studioWindow;

        private static UITestControl GetChildByAutomationIDPathImpl(UITestControl parent, int bookmark, params string[] automationIDs)
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
                throw new UITestControlNotFoundException("Cannot find " + automationIDs[bookmark] +
                    " control within parent" +
                    " with automation ID: " + parent.GetProperty("AutomationID") +
                    " and friendly name: " + parent.FriendlyName +
                    " and control type: " + parent.ControlType +
                    " and class name: " + parent.ClassName + ".");
            } 
            if (bookmark == automationIDs.Count() - 1)
            {
                return firstChildFound;
            }
            return GetChildByAutomationIDPathImpl(firstChildFound, ++bookmark, automationIDs);
        }

        public static UITestControl GetChildByAutomationIDPath(UITestControl parent, params string[] automationIDs)
        {
            return GetChildByAutomationIDPathImpl(parent, 0, automationIDs);
        }

        public static UITestControl GetControl(params string[] automationIDs)
        {
            if (_studioWindow == null)
            {
                _studioWindow = new UIMapBase.UIStudioWindow();
            }
            var control = GetChildByAutomationIDPath(_studioWindow, automationIDs);
            if (control == null)
            {
                throw new UITestControlNotFoundException();
            }
            return control;
        }
    }
}
