
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
