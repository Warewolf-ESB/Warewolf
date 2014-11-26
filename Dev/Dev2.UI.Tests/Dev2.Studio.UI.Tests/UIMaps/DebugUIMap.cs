
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.UI.Tests.Extensions;

namespace Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses
{
    using System;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Windows.Forms;


    public partial class DebugUIMap : UIMapBase
    {
        public void ClickItem(int row)
        {
            WpfRow theRow = GetRow(row);
            WpfEdit requiredEdit = (WpfEdit)theRow.GetChildren()[2].GetChildren()[0];
            Mouse.Click(requiredEdit, new Point(5, 5));

        }

        public int CountRows()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControlCollection rowList = debugWindow.GetChildren()[1].GetChildren()[0].GetChildren()[1].GetChildren();
            return rowList.Count;
        }



        public void CloseDebugWindow_ByCancel()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControl theControl = new UITestControl(debugWindow);
            theControl.SearchProperties.Add("AutomationId", "UI_Cancelbtn_AutoID");
            theControl.Find();
            Mouse.Click(theControl, new Point(5, 5));
        }

        public void CloseDebugWindow()
        {
            SendKeys.SendWait("{ESCAPE}");
        }

        public void ClickExecute(int waitAmt = 0)
        {
            var debugWindow = GetDebugWindow();
            int counter = 0;
            while(debugWindow == null && counter <= 5)
            {
                Playback.Wait(500);
                debugWindow = GetDebugWindow();
                counter++;
            }
            if(debugWindow == null)
            {
                throw new Exception("The debug popup couldnt be found");
            }
            foreach(var child in debugWindow.GetChildren())
            {
                if(child.GetProperty("AutomationId").ToString() == "UI_Executebtn_AutoID")
                {
                    Mouse.Click(child);
                    break;
                }
            }

            Playback.Wait(waitAmt);
        }

        public bool DebugWindowExists()
        {
            WpfWindow uIDebugWindow = UIDebugWindow;
            Point p;
            if(uIDebugWindow.TryGetClickablePoint(out p))
            {
                return true;
            }
            return false;
        }

        public void ClickXMLTab()
        {
            WpfWindow uIDebugWindow = UIDebugWindow;
            WpfTabPage XMLTabPage = (WpfTabPage)uIDebugWindow.GetChildren()[1].GetChildren()[1];
            Mouse.Click(XMLTabPage, new Point(5, 5));
            Playback.Wait(200);
        }

        public void ClickInputDataTab()
        {
            WpfWindow uIDebugWindow = UIDebugWindow;
            WpfTabPage TabPage = (WpfTabPage)uIDebugWindow.GetChildren()[1].GetChildren()[0];
            Mouse.Click(TabPage, new Point(5, 5));
            Playback.Wait(200);
        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public bool WaitForDebugWindow(int timeOut)
        {
            const int waitLength = 100;
            Type type = null;
            var timeNow = 0;
            while(type != typeof(WpfWindow) & timeNow < timeOut)
            {
                timeNow = timeNow + waitLength;
                Playback.Wait(waitLength);
                var tryGetDialog = StudioWindow.GetChildren()[0];
                type = tryGetDialog.GetType();
                if(timeNow > timeOut)
                {
                    break;
                }
            }
            return type == typeof(WpfWindow);
        }

        public void EnterTextIntoRow(int rowIndex, string stringToEnter)
        {
            WpfRow wpfRow = GetRow(rowIndex);

            WpfEdit requiredEdit = (WpfEdit)wpfRow.GetChildren()[2].GetChildren()[0];
            requiredEdit.EnterText(stringToEnter);
        }

        public string GetTextFromRow(int rowIndex)
        {
            WpfEdit textFromRow = DebugUIMap.GetRow(rowIndex).Cells[1].GetChildren()[0] as WpfEdit;
            if(textFromRow != null)
            {
                return textFromRow.Text;
            }
            return null;
        }
    }
}
