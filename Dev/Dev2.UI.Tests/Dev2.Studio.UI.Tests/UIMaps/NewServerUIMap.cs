
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Drawing;
    using System.Windows.Forms;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    // Due to the Chromium nature of the control, Points have to be used, as the UI Tester cannot get actual items.
    public partial class NewServerUIMap
    {
        public void EnterServerAddress(string address)
        {
            WpfWindow theWindow = GetNewServerWindow();
            if(theWindow != null)
            {
                SendKeys.SendWait("{TAB}");
                System.Threading.Thread.Sleep(500);
                SendKeys.SendWait(address);
            }
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
            //            WpfWindow theWindow = GetNewServerWindow();
            //            Point p = new Point(theWindow.BoundingRectangle.Left + theWindow.BoundingRectangle.Width - 25, theWindow.BoundingRectangle.Top + 5);
            //            Mouse.Move(p);
            //            Mouse.Click();

            #region Variable Declarations
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            #endregion

            // Click image
            Mouse.Click(uIItemImage, new Point(654, 455));
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

        public void SelectAuthenticationType(string p0)
        {
            WpfWindow theWindow = GetNewServerWindow();
            if(theWindow != null)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(1000);
                switch(p0)
                {
                    case "Windows":
                        {
                            Keyboard.SendKeys("{LEFT}{ENTER}");
                            break;
                        }
                    case "User":
                        {
                            Keyboard.SendKeys("{RIGHT}{ENTER}");
                            break;
                        }
                    case "Public":
                        {
                            Keyboard.SendKeys("{RIGHT}{RIGHT}{ENTER}");
                            break;
                        }
                }
                Playback.Wait(1000);
            }
        }

        public void ClickTestConnection()
        {
            SendKeys.SendWait("{TAB}");
            Playback.Wait(1000);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(1000);
        }

        public void ClickSave()
        {
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        public void ClickCancel()
        {
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        public void SaveNameInDialog(string serverName)
        {
            SendKeys.SendWait("{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait(serverName);
        }

        public void EnterUserName(string userName)
        {
            WpfWindow theWindow = GetNewServerWindow();
            if(theWindow != null)
            {
                SendKeys.SendWait("{TAB}");
                System.Threading.Thread.Sleep(500);
                SendKeys.SendWait("{ENTER}");
                SendKeys.SendWait(userName);
            }
        }

        public void EnterPassword(string password)
        {
            WpfWindow theWindow = GetNewServerWindow();
            if(theWindow != null)
            {
                SendKeys.SendWait("{TAB}");
                System.Threading.Thread.Sleep(500);
                SendKeys.SendWait("{ENTER}");
                SendKeys.SendWait(password);
            }
        }
    }
}
