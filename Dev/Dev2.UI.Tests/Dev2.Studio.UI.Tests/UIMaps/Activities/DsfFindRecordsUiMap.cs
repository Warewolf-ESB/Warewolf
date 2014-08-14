using System.Collections.Generic;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Linq;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfFindRecordsUiMap : ToolsUiMapBase
    {
        public DsfFindRecordsUiMap(bool createNewtab = true, bool dragFindRecordsOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragFindRecordsOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.Find);
            }
        }

        public void EnterTextIntoFieldsToSearch(string stringToEnter, ViewType viewType)
        {
            GetFieldsToSearchTextBoxControl(viewType).EnterText(stringToEnter);
        }

        public string GetTextFromFieldsToSearch(ViewType viewType)
        {
            return GetFieldsToSearchTextBoxControl(viewType).GetText();
        }

        public UITestControl GetFieldsToSearchTextBoxControl(ViewType viewType)
        {
            return GetTextBox("UI__FieldsToSearchtxt_AutoID", GetView(viewType));
        }

        public UITestControl GetResultTextBoxControl(ViewType viewType)
        {
            return GetTextBox("UI__Result_AutoID", GetView(viewType));
        }

        public void SetFocusToConditionDropDown(int rowNumber, ViewType viewType)
        {
            List<UITestControl> dataGridRowChildList = GetDataGridRowChildList(rowNumber - 1, viewType);
            dataGridRowChildList[2].SetFocus();
        }

        public bool IsMatchTextBoxEnabled(int rowNumber, ViewType viewType)
        {
            List<UITestControl> dataGridRowChildList = GetDataGridRowChildList(rowNumber, viewType);
            UITestControl firstOrDefault = dataGridRowChildList[3].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(firstOrDefault == null)
            {
                throw new Exception("Could not find the matches text box.");
            }
            return firstOrDefault.IsEnabled();
        }

        private List<UITestControl> GetDataGridRowChildList(int rowNumber, ViewType viewType)
        {
            UITestControl childByAutomationIDPath = VisualTreeWalker.GetChildByAutomationIdPath(GetView(viewType), "Table");
            List<UITestControl> uiTestControlCollection = childByAutomationIDPath.GetChildren().Where(c => c.ControlType == ControlType.Row).ToList();
            return uiTestControlCollection[rowNumber].GetChildren().ToList();
        }

        private UITestControl GetTextBox(string autoId, UITestControl viewControl)
        {
            UITestControlCollection uiTestControlCollection = viewControl.GetChildren();
            UITestControl textbox = uiTestControlCollection.FirstOrDefault(c => c.ControlType == ControlType.Edit && c.FriendlyName == autoId);
            if(textbox != null)
            {
                return textbox;
            }

            throw new Exception("Couldn't find the" + autoId + " textbox.");
        }

    }

}
