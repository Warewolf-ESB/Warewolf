using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.Utils
{
    public class VisualTreeWalker
    {
        static UIMapBase.UIStudioWindow _studioWindow;
        static Dictionary<string, UITestControl> _controlCache = new Dictionary<string, UITestControl>();

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
        /// <param name="automationIDs">The automation attribute ds.</param>
        /// <returns></returns>
        public static UITestControl GetControlFromRoot(bool singleSearch, int splitPaneIndex, UITestControl startControl, params string[] automationIDs)
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
                if (startControl != null)
                {
                    var list = automationIDs.ToList();
                    list.RemoveRange(0, automationIDs.Length - 1);
                    automationIDs = list.ToArray();
                }

                UITestControl theControl = null;
                // handle all other pinned panes ;)
                if(singleSearch)
                {
                    var automationCounter = 0;
                    while(automationCounter <= automationIDs.Length - 1)
                    {
                        theControl = new UITestControl(startControl ?? _studioWindow);
                        var automationId = automationIDs[automationCounter];
                            UITestControl foundControl;
                            if(_controlCache.TryGetValue(automationId, out foundControl))
                            {
                                theControl = foundControl;
                                try
                                {
                                    //children = theControl.GetChildren();
                                }
                                catch(UITestControlNotFoundException)
                                {
                                    theControl.SearchProperties[WpfControl.PropertyNames.AutomationId] = automationId;
                                    theControl.Find();
                                }
                            }
                            else
                            {
                                theControl.SearchProperties[WpfControl.PropertyNames.AutomationId] = automationId;
                                theControl.Find();
                                startControl = theControl;
                            }
                        automationCounter++;
                        if(automationCounter != automationIDs.Length)
                        {
                            //children = theControl.GetChildren();
                        }
                        if(!_controlCache.ContainsKey(automationId))
                        {

                            //_controlCache.Add(automationId, theControl);
                        }
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

                        for(var i = 1; i < automationIDs.Length; i++)
                        {
                            if(parent == null)
                            {
                                continue;
                            }
                            var children = parent.GetChildren();

                            UITestControl canidate = null;
                            foreach(var child in children)
                            {
                                var childAutoId = child.GetProperty("AutomationID").ToString();

                                if(childAutoId == automationIDs[i] ||
                                   childAutoId.Contains(automationIDs[i]) ||
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
                        return parent;
                    }
                    if(theControl.ControlType != null)
                    {
                        throw new UITestControlNotFoundException("Child with " + UITestControl.PropertyNames.ClassName + ": " + automationIDs[0] + " not found within parent with automation ID: " + theControl.GetProperty("AutomationID") + " and FriendlyName: " + theControl.FriendlyName + " and ControlType: " + theControl.ControlType.Name + " and ClassName: " + theControl.ClassName);
                    }
                }
            }
            return null;
        }

        public static void ClearControlCache()
        {
            _controlCache = new Dictionary<string, UITestControl>();
        }
    }
}
