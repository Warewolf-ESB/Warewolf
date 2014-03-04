using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses
{
    using System.Drawing;
    using System.Windows.Input;
    using Keyboard = Keyboard;
    using Mouse = Mouse;
    using MouseButtons = MouseButtons;


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
            SendKeys.SendWait("localhost{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("test{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("test{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("^AThorLocal@norsegods.com{TAB}");
            SendKeys.SendWait("dev2warewolf@gmail.com{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(2000);//wait for test
            ClickSave();

            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(50);
            SendKeys.SendWait(sourceName);
            Playback.Wait(50);
            SendKeys.SendWait("{TAB}{ENTER}");
        }
    }
}
