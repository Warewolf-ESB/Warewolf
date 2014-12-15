
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

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
            WorkflowDesignerUIMap.OpenCloseLargeView(Activity);
        }

        public void ClickCloseLargeView()
        {
            WorkflowDesignerUIMap.OpenCloseLargeView(Activity);
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
            GetQuickVariableInputView().FindByAutomationId("QviVariableListBox", false).EnterText(variables);
        }

        public void SelectSplitOn(int index)
        {
            WpfComboBox comboBox = GetQuickVariableInputView().FindByAutomationId("QviSplitOnCombobox", false) as WpfComboBox;
            if(comboBox != null)
            {
                comboBox.SelectedIndex = index;
            }
        }

        public void EnterSplitCharacter(string splitChar)
        {
            GetQuickVariableInputView().FindByAutomationId("QviSplitOnCharacter", false).EnterText(splitChar);
        }

        public void SelectReplaceOption()
        {
            GetQuickVariableInputView().FindByAutomationId("ReplaceOption", false).Click();
        }

        public void SelectAppendOption()
        {
            GetQuickVariableInputView().FindByAutomationId("AppendOption", false).Click();
        }

        public void ClickAddButton()
        {
            GetControlOnActivity("AddButton", ControlType.Button).Click();
        }

        public WpfTable GetSmallViewTable()
        {
            UITestControl findContent = null;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var child in Activity.GetChildren())
            // ReSharper restore LoopCanBeConvertedToQuery
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

        protected UITestControl GetView(ViewType viewType)
        {
            if(viewType == ViewType.Large)
            {
                return GetLargeView();
            }
            return GetSmallView();
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

        public enum ViewType
        {
            Large,
            Small
        }
    }
}
