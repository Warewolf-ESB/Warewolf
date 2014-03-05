using System;
using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfFindRecordsUiMap : ActivityUiMapBase
    {
        public DsfFindRecordsUiMap(bool createNewtab = true, bool dragFindRecordsOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragFindRecordsOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.Find);
            }
        }

        public void ClickOpenLargeView()
        {
            UITestControl button = AdornersGetButton("Open Large View");
            Mouse.Click(button, new Point(5, 5));
        }

        public void ClickCloseLargeView()
        {
            UITestControl button = AdornersGetButton("Close Large View");
            Mouse.Click(button, new Point(5, 5));
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

        private UITestControl GetView(ViewType viewType)
        {
            if(viewType == ViewType.Large)
            {
                return GetLargeView();
            }
            return GetSmallView();
        }

        UITestControl GetSmallView()
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl firstOrDefault = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "SmallViewContent");
            if(firstOrDefault != null)
            {
                return firstOrDefault;
            }
            throw new Exception("Couldnt find the small view");
        }

        UITestControl GetLargeView()
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl firstOrDefault = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "LargeViewContent");
            if(firstOrDefault != null)
            {
                return firstOrDefault;
            }
            throw new Exception("Couldnt find the large view");
        }
    }

    public enum ViewType
    {
        Large,
        Small
    }
}
