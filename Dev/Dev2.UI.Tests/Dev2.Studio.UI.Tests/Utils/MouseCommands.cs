using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class MouseCommands
    {

        public static void WaitAndClick(int waitAmt)
        {
            Playback.Wait(waitAmt);
            Mouse.Click();
        }

        public static void ClickAndWait(int waitAmt)
        {
            Mouse.Click();
            Playback.Wait(waitAmt);
        }

        public static void ClickPoint(Point p, int waitAmt = 0)
        {
            Mouse.Click(p);
            Playback.Wait(waitAmt);
        }

        public static void MoveAndClick(Point p, int waitAmt = 0)
        {
            Mouse.Move(p);
            Mouse.Click();
            Playback.Wait(waitAmt);
        }

        public static void ClickControl(UITestControl control)
        {
            Mouse.Click(control);
        }

        public static void ClickControlAtPoint(UITestControl control, Point p)
        {
            Mouse.Click(control, p);
        }
    }
}
