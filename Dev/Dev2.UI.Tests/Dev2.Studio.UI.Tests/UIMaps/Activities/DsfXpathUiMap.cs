using System;
using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfXpathUiMap : ActivityUiMapBase
    {
        public DsfXpathUiMap(bool createNewtab = true, bool dragFindRecordsOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragFindRecordsOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.XPath);
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
            return GetControl("UI__SourceStringtxt_AutoID", GetView(viewType), ControlType.Edit); 
        }

        public UITestControl GetDoneButton()
        {
            return GetControlOnActivity("DoneButton", ControlType.Button);
        }
        
        private UITestControl GetControl(string autoId, UITestControl viewControl, ControlType controlType)
        {
            UITestControlCollection uiTestControlCollection = viewControl.GetChildren();
            UITestControl control = uiTestControlCollection.FirstOrDefault(c => ((WpfControl)c).AutomationId.Equals(autoId));
            if(control != null)
            {
                return control;
            }

            throw new Exception("Couldn't find the " + autoId + " for control type " + controlType);
        }

        private UITestControl GetControlOnActivity(string autoId, ControlType controlType)
        {
            UITestControlCollection uiTestControlCollection = Activity.GetChildren();
            UITestControl control = uiTestControlCollection.FirstOrDefault(c => ((WpfControl)c).AutomationId.Equals(autoId));
            if(control != null)
            {
                return control;
            }

            throw new Exception("Couldn't find the " + autoId + " for control type " + controlType);
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
}
