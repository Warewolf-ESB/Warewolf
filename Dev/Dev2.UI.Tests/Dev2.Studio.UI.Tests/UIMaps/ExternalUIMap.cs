namespace Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;


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
            var browsers = new[] { "iexplore", "chrome" };

            foreach(var browser in browsers)
            {
                Process[] processList = Process.GetProcessesByName(browser);
                foreach(Process p in processList)
                {
                    try
                    {
                        p.Kill();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                }
            }

        }

        public bool Outlook_HasOpened()
        {
            WinWindow outlookWindow = GetOutlookAfterFeedbackWindow();
            Point p;
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
