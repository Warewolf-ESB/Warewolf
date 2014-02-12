using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfActivityUiMap : ActivityUiMapBase
    {
        public DsfActivityUiMap(bool createNewtab = true)
            : base(createNewtab)
        {

        }


        #region Public Methods

        public void ClickEdit()
        {
            UITestControl button = AdornersGetButton("Edit");
            Mouse.Move(button.GetParent(), new Point(5, 5));
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
            if(fixErrorsButton == null)
            {
                return false;
            }
            return true;
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
            List<UITestControl> buttons = Activity.GetChildren().Where(c => c.ControlType == ControlType.Button).ToList();
            if(buttons.Any())
            {
                return buttons[0];
            }
            return null;
        }

    }
}
