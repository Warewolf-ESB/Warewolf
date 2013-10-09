namespace Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses
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
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using System.Windows.Forms;


    public partial class ExternalUIMap
    {
        public bool NotepadTextContains(string text)
        {
            return uiMapNotepadTextContains(text);
        }

        public string GetIEBodyText()
        {
            UITestControl ie = GetIE();
            UITestControl subMap = new UITestControl(ie);
            subMap.SearchProperties[UITestControl.PropertyNames.ClassName] = "Internet Explorer_Server";
            subMap.Find();
            UITestControl bodyText = subMap.GetChildren()[0];
            
            string body = bodyText.GetProperty("InnerText").ToString();
            return body;
        }

        public void SendIERefresh()
        {
            UITestControl ie = GetIE();

            ie.SetFocus();

            SendKeys.SendWait("{F5}");
        }

        public void CloseAllInstancesOfIE()
        {
            Process[] processList = Process.GetProcessesByName("iexplore");
            foreach (Process p in processList)
            {
                p.Kill();
            }
        }

        public bool Outlook_HasOpened()
        {
            WinWindow outlookWindow = GetOutlookAfterFeedbackWindow();
            Point p = new Point();
            return outlookWindow.TryGetClickablePoint(out p);
        }

        public void CloseAllInstancesOfOutlook()
        {
            Process[] processList = Process.GetProcessesByName("OUTLOOK");
            foreach(Process p in processList)
            {
                p.Kill();
            }
        }
    }
}
