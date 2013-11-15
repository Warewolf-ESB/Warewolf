using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests.UIMaps
{
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
            var browsers = new string[]{"iexplore", "chrome"};

            foreach (var browser in browsers)
            {
                Process[] processList = Process.GetProcessesByName(browser);
                foreach (Process p in processList)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception e)
                    {
                    }
                }    
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
