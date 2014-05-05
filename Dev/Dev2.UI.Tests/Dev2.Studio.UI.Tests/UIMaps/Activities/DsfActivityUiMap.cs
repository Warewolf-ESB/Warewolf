using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfActivityUiMap : ActivityUiMapBase
    {
        public DsfActivityUiMap(bool createNewtab = true, int waitAmt = 1000)
            : base(createNewtab, waitAmt)
        {

        }


        #region Public Methods

        public void ClickEdit()
        {
            Mouse.Move(Activity, new Point(5, 5));
            UITestControl button = AdornersGetButton("Edit");
            Mouse.Click(button, new Point(5, 5));
            Playback.Wait(2000);
        }

        public void ClickOpenMapping()
        {
            Activity.DoubleClick();
        }

        public void ClickCloseMapping(int waitAmt = 0)
        {
            Activity.DoubleClick();
        }

        public bool IsFixErrorButtonShowing()
        {
            UITestControl fixErrorsButton = GetFixErrorsButton();
            Point buttonPoint;
            if(fixErrorsButton != null && fixErrorsButton.TryGetClickablePoint(out buttonPoint))
            {
                return true;
            }
            return false;
        }

        public void ClickFixErrors()
        {
            UITestControl fixErrorsButton = GetFixErrorsButton();
            if(fixErrorsButton != null)
            {
                Mouse.Click(fixErrorsButton, new Point(5, 5));
            }
        }

        public string GetDoneButtonDisplayName()
        {
            return GetDoneButton().DisplayText;
        }

        public void ClickDoneButton()
        {
            Mouse.Click(GetDoneButton());
        }

        #endregion

        WpfButton GetDoneButton()
        {
            UITestControl doneButton = Activity.FindByAutomationId("DoneButton");
            return doneButton as WpfButton;
        }

        UITestControl GetFixErrorsButton()
        {
            UITestControlCollection activityChildren = Activity.GetChildren();
            UITestControl smallView = activityChildren.FirstOrDefault(c => c.FriendlyName.Contains("SmallView"));
            if(smallView != null)
            {
                List<UITestControl> buttons = smallView.GetChildren().Where(c => c.ControlType == ControlType.Button).ToList();
                if(buttons.Any())
                {
                    return buttons[0];
                }
            }

            return null;
        }

        public string GetInputMappingToServiceValue(int rowNumber)
        {
            var rows = GetInputMappingRows();
            if(rows.Count >= rowNumber)
            {
                UITestControlCollection rowChildren = rows[rowNumber - 1].GetChildren();
                List<UITestControl> cells = rowChildren.Where(c => c.ControlType == ControlType.Cell).ToList();
                UITestControlCollection cellChildren = cells[1].GetChildren();
                UITestControl firstOrDefault = cellChildren.FirstOrDefault(c => c.ControlType == ControlType.Text && c.FriendlyName != "*");
                if(firstOrDefault != null)
                {
                    WpfText wpfText = firstOrDefault as WpfText;
                    if(wpfText != null)
                    {
                        return wpfText.DisplayText;
                    }
                }
            }
            else
            {
                throw new Exception("The row could not be found.");
            }
            return null;
        }

        public List<UITestControl> GetInputMappingRows()
        {
            VisualTreeWalker visualTreeWalker = new VisualTreeWalker();
            UITestControl table = visualTreeWalker.GetChildByAutomationIDPath(Activity, "LargeView", "Table");
            UITestControlCollection tableChildren = table.GetChildren();
            List<UITestControl> rows = tableChildren.Where(c => c.ControlType == ControlType.Row).ToList();
            return rows;
        }
    }
}
