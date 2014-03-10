using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
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
