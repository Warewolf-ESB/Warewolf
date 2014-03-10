using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Linq;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public abstract class ToolsUiMapBase : ActivityUiMapBase
    {
        protected ToolsUiMapBase(bool createNewtab = true, int wait = 1500)
            : base(createNewtab, wait)
        {

        }

        public UITestControl GetDoneButton()
        {
            return GetControlOnActivity("DoneButton", ControlType.Button);
        }

        public void ClickOpenLargeView()
        {
            ClickButton("Open Large View");
        }

        public void ClickCloseLargeView()
        {
            ClickButton("Close Large View");
        }

        public void ClickOpenQuickVariableInput()
        {
            ClickButton("Open Quick Variable Input");
        }

        public void ClickCloseQuickVariableInput()
        {
            ClickButton("Close Quick Variable Input");
        }

        public void EnterVariables(string variables)
        {
            GetQuickVariableInputView().FindByAutomationId("QviVariableListBox").EnterText(variables);
        }

        public void SelectSplitOn(int index)
        {
            WpfComboBox comboBox = GetQuickVariableInputView().FindByAutomationId("QviSplitOnCombobox") as WpfComboBox;
            if(comboBox != null)
            {
                comboBox.SelectedIndex = index;
            }
        }

        public void EnterSplitCharacter(string splitChar)
        {
            GetQuickVariableInputView().FindByAutomationId("QviSplitOnCharacter").EnterText(splitChar);
        }

        public void SelectReplaceOption()
        {
            GetQuickVariableInputView().FindByAutomationId("ReplaceOption").Click();
        }

        public void SelectAppendOption()
        {
            GetQuickVariableInputView().FindByAutomationId("AppendOption").Click();
        }

        public void ClickAddButton()
        {
            GetControlOnActivity("AddButton", ControlType.Button).Click();
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

        protected UITestControl GetSmallView()
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl firstOrDefault = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "SmallViewContent");
            if(firstOrDefault != null)
            {
                return firstOrDefault;
            }
            throw new Exception("Couldnt find the small view");
        }

        protected UITestControl GetLargeView()
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl firstOrDefault = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "LargeViewContent");
            if(firstOrDefault != null)
            {
                return firstOrDefault;
            }
            throw new Exception("Couldnt find the large view");
        }

        protected UITestControl GetQuickVariableInputView()
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl firstOrDefault = uiTestControlCollection.FirstOrDefault(c => ((WpfControl)c).AutomationId == "QuickVariableInputContent");
            if(firstOrDefault != null)
            {
                return firstOrDefault;
            }
            throw new Exception("Couldnt find the quick variable input view");
        }

        protected UITestControl GetControlOnActivity(string autoId, ControlType controlType)
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl control = uiTestControlCollection.FirstOrDefault(c => ((WpfControl)c).AutomationId.Equals(autoId));
            if(control != null)
            {
                return control;
            }

            throw new Exception("Couldn't find the " + autoId + " for control type " + controlType);
        }

        protected UITestControl GetView(ViewType viewType)
        {
            if(viewType == ViewType.Large)
            {
                return GetLargeView();
            }
            return GetSmallView();
        }

        protected UITestControl GetControl(string autoId, UITestControl viewControl, ControlType controlType)
        {
            UITestControlCollection uiTestControlCollection = viewControl.GetChildren();
            UITestControl control = uiTestControlCollection.FirstOrDefault(c => ((WpfControl)c).AutomationId.Equals(autoId));
            if(control != null)
            {
                return control;
            }

            throw new Exception("Couldn't find the " + autoId + " for control type " + controlType);
        }

        public enum ViewType
        {
            Large,
            Small
        }
    }
}