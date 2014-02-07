using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses
{
    using System.Drawing;
    using System.Windows.Input;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;


    public partial class EmailSourceWizardUIMap
    {
        public void OpenWizard()
        {
            var getLocalServer = ExplorerUIMap.GetLocalServer();
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(getLocalServer.BoundingRectangle.X + 25, getLocalServer.BoundingRectangle.Y));
            for(var i = 0; i < 10; i++)
            {
                Keyboard.SendKeys("{DOWN}");
            }

            SendKeys.SendWait("{ENTER}");

            //wait for email source wizard
            WizardsUIMap.WaitForWizard();
        }

        public void CreateEmailSource(string sourceName)
        {
            SendKeys.SendWait("{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("smtp.afrihost.co.za{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("dev2test{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("Password{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("^AThorLocal@norsegods.com{TAB}");
            SendKeys.SendWait("dev2warewolf@gmail.com{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(30000);//wait for test
            ClickSave();

            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait(sourceName);
            Playback.Wait(50);
            SendKeys.SendWait("{TAB}{ENTER}");
        }
    }
}
