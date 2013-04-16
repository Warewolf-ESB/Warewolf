namespace Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Windows.Forms;
    
    
    // Due to the Chromium nature of the control, Points have to be used, as the UI Tester cannot get actual items.
    public partial class NewServerUIMap
    {
        public void EnterServerAddress(string address)
        {
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + 300, theWindow.BoundingRectangle.Top + 125);
            Mouse.Move(p);
            Mouse.Click();
            SendKeys.SendWait(address);
        }

        public void ClearServerAddress()
        {
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + 300, theWindow.BoundingRectangle.Top + 125);
            Mouse.Move(p);
            Mouse.Click();
            SendKeys.SendWait("{END}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("+{HOME}");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{DELETE}");
        }

        public bool IsNewServerWindowOpen()
        {
            try
            {
                WpfWindow theWindow = GetNewServerWindow();
                Point p = new Point();
                return theWindow.TryGetClickablePoint(out p);
            }
            catch
            {
                return false;
            }
        }

        public void CloseWindow()
        {
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + theWindow.BoundingRectangle.Width - 25, theWindow.BoundingRectangle.Top + 5);
            Mouse.Move(p);
            Mouse.Click();
        }

        public void ClickSaveConnection()
        {
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + 300, theWindow.BoundingRectangle.Top + 275);
            Mouse.Move(p);
            Mouse.Click();
        }

        public string SaveWindow_EnterNameText_Return_NameText(string text)
        {
            Clipboard.SetText("asduansldkuansdio7asnd");
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + 300, theWindow.BoundingRectangle.Top + 450);
            Mouse.Move(p);

            // Click the box
            Mouse.Click();

            // Enter the text
            SendKeys.SendWait(text);

            // Copy it (It's not copy friendly...)
            System.Threading.Thread.Sleep(500);
            Mouse.DoubleClick();
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^(C)");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^(c)");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^C");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^c");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^(C)");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^(c)");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^C");
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("^c");
            System.Threading.Thread.Sleep(500);
            string returnText = Clipboard.GetText();
            // And return it
            return returnText;
        }

        public void SaveWindow_ClickCancel()
        {
            WpfWindow theWindow = GetNewServerWindow();
            Point p = new Point(theWindow.BoundingRectangle.Left + 700, theWindow.BoundingRectangle.Top + 525);
            Mouse.Click(p);
        }
    }
}
