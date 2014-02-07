using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses
{
    using System.Drawing;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    public partial class WebServiceWizardUIMap
    {
        public void ClickNewWebSource()
        {
            var control = StudioWindow.GetChildren()[0].GetChildren()[0];
            control.WaitForControlEnabled();
            Mouse.Click(control, new Point(410, 79));
        }

        public void CreateWebSource(string sourceUrl, string sourceName)
        {
            //Web Source Details
            SendKeys.SendWait(sourceUrl);
            SendKeys.SendWait("{TAB}{TAB}{TAB}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(10000);
            WebSourceWizardUIMap.ClickSave();
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}" + sourceName + "{TAB}{ENTER}");
            Playback.Wait(1000);
        }

        public void SaveWebService(string serviceName)
        {
            //Web Service Details
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(30000);//wait for test
            SendKeys.SendWait("{TAB}{ENTER}");
            Playback.Wait(1000);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}" + serviceName + "{TAB}{ENTER}");
        }
    }
}
