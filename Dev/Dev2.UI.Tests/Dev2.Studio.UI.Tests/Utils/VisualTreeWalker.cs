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
            //Find all children
            var children = parent.GetChildren();
            if(children == null)
            {
                throw new UITestControlNotFoundException("Cannot find children of" + parent.GetProperty("AutomationID") +
                    " and friendly name: " + parent.FriendlyName +
                    " and control type: " + parent.ControlType +
                    " and class name: " + parent.ClassName + ".");
            }

            //Find some child
            var firstChildFound = children.Where(child =>
                {
                    var childID = child.GetProperty("AutomationID");
                    if(childID != null)
                    {
                        var childAutoID = childID.ToString();
                        return childAutoID.Contains(automationIDs[bookmark])
                            || child.FriendlyName.Contains(automationIDs[bookmark])
                            || child.ControlType.Name.Contains(automationIDs[bookmark])
                            || child.ClassName.Contains(automationIDs[bookmark]);
                    }
                    return false;
                }).FirstOrDefault();
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

        public static UITestControl GetChildByAutomationIDPath(UITestControl parent, params string[] automationIDs)
        {
            return GetChildByAutomationIDPathImpl(parent, 0, automationIDs);
        }

        public static UITestControl GetControl(params string[] automationIDs)
        {
            if(_studioWindow == null)
            {
                _studioWindow = new UIMapBase.UIStudioWindow();
            }
            var control = GetChildByAutomationIDPath(_studioWindow, automationIDs);
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
        /// <param name="depth">The depth.</param>
        /// <param name="singleSearch">if set to <c>true</c> [single search].</param>
        /// <param name="splitPaneIndex">Index of the split pane.</param>
        /// <param name="automationIDs">The automation attribute ds.</param>
        /// <returns></returns>
        public static UITestControl GetControlFromRoot(int depth, bool singleSearch, int splitPaneIndex, params string[] automationIDs)
        {
            if(_studioWindow == null)
            {
                _studioWindow = new UIMapBase.UIStudioWindow();
                _studioWindow.Find();
            }

            if(automationIDs != null && automationIDs.Length > 0)
            {
                UITestControl theControl = new UITestControl(_studioWindow);

                // handle all other pinned panes ;)
                if(singleSearch)
                {
                    theControl.SearchProperties[UITestControl.PropertyNames.ClassName] = automationIDs[0];
                    theControl.Find();

                    if(automationIDs.Length > 1)
                    {
                        return GetChildByAutomationIDPathImpl(theControl, depth, automationIDs);
                    }

                    return theControl;
                }

                // handle the explorer ;)
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if(!singleSearch && automationIDs.Length > 1 && splitPaneIndex >= 0)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    theControl.SearchProperties[UITestControl.PropertyNames.ClassName] = automationIDs[0];

                    // 1 is the explorer pinned to top left ;)
                    var tmp = theControl.FindMatchingControls();

                    if(tmp != null)
                    {
                        var parent = tmp[splitPaneIndex];

                        for(int i = 1; i < automationIDs.Length; i++)
                        {
                            if(parent != null)
                            {
                                var children = parent.GetChildren();

                                UITestControl canidate = null;
                                foreach(var child in children)
                                {
                                    var childAutoID = child.GetProperty("AutomationID").ToString();

                                    if(childAutoID == automationIDs[i] ||
                                        childAutoID.Contains(automationIDs[i]) ||
                                        child.FriendlyName.Contains(automationIDs[i]) ||
                                        child.ControlType.Name.Contains(automationIDs[i]) ||
                                        child.ClassName.Contains(automationIDs[i]))
                                    {
                                        canidate = child;
                                    }
                                }

                                // all done, tag it ;)
                                parent = canidate;
                            }
                        }
                        return parent;
                    }
                    throw new UITestControlNotFoundException("Child with " + UITestControl.PropertyNames.ClassName + ": " + automationIDs[0] + " not found within parent with automation ID: " + theControl.GetProperty("AutomationID").ToString() + " and FriendlyName: " + theControl.FriendlyName + " and ControlType: " + theControl.ControlType.Name + " and ClassName: " + theControl.ClassName);
                }
            }

            return null;

        }
    }
}
