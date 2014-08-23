using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Linq;

namespace Dev2.Studio.UI.Tests.Utils
{
    public class VisualTreeWalker
    {
        static UIMapBase.UIStudioWindow _studioWindow;

        private UITestControl GetChildByAutomationIDPathImpl(UITestControl parent, int bookmark, params string[] automationIDs)
        {
            //Find all children
            var children = parent.GetChildren();
            if(children == null)
            {
                try
                {
                    throw new UITestControlNotFoundException("Cannot find children of " + parent.GetProperty("AutomationID") +
                        " and friendly name: " + parent.FriendlyName +
                        " and control type: " + parent.ControlType +
                        " and class name: " + parent.ClassName + ".");
                }
                catch(NullReferenceException)
                {
                    throw new UITestControlNotFoundException("Cannot find children of " + parent.GetProperty("AutomationID") + ".");
                }
            }

            //Find some child
            var firstChildFound = children.FirstOrDefault(child =>
                {
                    var childId = child.GetProperty("AutomationID");
                    if(childId != null)
                    {
                        var childAutoId = childId.ToString();
                        return childAutoId.Contains(automationIDs[bookmark])
                            || child.FriendlyName.Contains(automationIDs[bookmark])
                            || child.ControlType.Name.Contains(automationIDs[bookmark])
                            || child.ClassName.Contains(automationIDs[bookmark]);
                    }
                    return false;
                });
            if(firstChildFound == null)
            {
                throw new UITestControlNotFoundException("Cannot find " + automationIDs[bookmark] +
                    " control within parent" +
                    " with automation ID: " + parent.GetProperty("AutomationID") +
                    " and friendly name: " + parent.FriendlyName +
                    " and control type: " + parent.ControlType +
                    " and class name: " + parent.ClassName + ".");
            }

            //Return child or some child of child...
            return bookmark == automationIDs.Count() - 1 ? firstChildFound : GetChildByAutomationIDPathImpl(firstChildFound, ++bookmark, automationIDs);
        }

        public static UITestControl GetChildByAutomationIdPath(UITestControl parent, params string[] automationIDs)
        {
            return new VisualTreeWalker().GetChildByAutomationIDPathImpl(parent, 0, automationIDs);
        }

        public static UITestControl GetControl(params string[] automationIDs)
        {
            if(_studioWindow == null)
            {
                _studioWindow = new UIMapBase.UIStudioWindow();
            }
            var control = GetChildByAutomationIdPath(_studioWindow, automationIDs);
            if(control == null)
            {
                throw new UITestControlNotFoundException();
            }
            return control;
        }

        /// <summary>
        /// Gets the control from the Window.
        /// Used to search pinned panes
        /// </summary>
        /// <param name="singleSearch">if set to <c>true</c> [single search].</param>
        /// <param name="splitPaneIndex">Index of the split pane.</param>
        /// <param name="startControl"></param>
        /// <param name="returnNullIfNotFound"></param>
        /// <param name="automationIDs">The automation attribute ds.</param>
        /// <returns></returns>
        public static UITestControl GetControlFromRoot(bool singleSearch, int splitPaneIndex, UITestControl startControl, bool returnNullIfNotFound, params string[] automationIDs)
        {
            if(_studioWindow == null)
            {
                _studioWindow = new UIMapBase.UIStudioWindow();
                _studioWindow.Find();
            }

            if(automationIDs == null || automationIDs.Length == 0)
            {
                return _studioWindow;
            }

            if(automationIDs.Length > 0)
            {
                UITestControl theControl = null;
                if(singleSearch)
                {
                    var automationCounter = 0;
                    while(automationCounter <= automationIDs.Length - 1)
                    {
                        var wpfControl = startControl ?? _studioWindow;
                        var automationId = automationIDs[automationCounter];
                        theControl = automationId.EndsWith("*") ?  wpfControl.FindByFriendlyName(automationId) :
                                                                   wpfControl.FindByAutomationId(automationId, returnNullIfNotFound); ;
                        if(theControl == null && returnNullIfNotFound)
                        {
                            return null;
                        }
                        startControl = theControl;
                        automationCounter++;
                    }
                    return theControl;
                }
            }
            return null;
        }

        public static UITestControl GetControlFromRoot(bool singleSearch, int splitPaneIndex, UITestControl startControl, params string[] automationIDs)
        {
            return GetControlFromRoot(singleSearch, splitPaneIndex,startControl , false , automationIDs);
        }
    }
}
