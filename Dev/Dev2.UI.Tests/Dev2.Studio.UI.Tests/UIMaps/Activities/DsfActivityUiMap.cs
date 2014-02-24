using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;

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
            UITestControl button = AdornersGetButton("Open Mapping");
            Mouse.Click(button, new Point(5, 5));
        }

        public void ClickCloseMapping()
        {
            UITestControl button = AdornersGetButton("Close Mapping");
            Mouse.Click(button, new Point(5, 5));
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

        #endregion

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

    }
}
