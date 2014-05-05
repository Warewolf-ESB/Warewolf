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
            KeyboardCommands.SendTab();
            Playback.Wait(50);
            KeyboardCommands.SendKey("localhost");
            KeyboardCommands.SendTab();
            Playback.Wait(50);
            KeyboardCommands.SendKey("test");
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey("test");
            KeyboardCommands.SendTabs(2);
            Playback.Wait(50);
            KeyboardCommands.SendTabs(2);
            Playback.Wait(50);
            KeyboardCommands.SendEnter();
            SendKeys.SendWait("^AThorLocal@norsegods.com{TAB}");
            SendKeys.SendWait("dev2warewolf@gmail.com{TAB}");
            Playback.Wait(50);
            KeyboardCommands.SendEnter();
            Playback.Wait(5000);//wait for test
            ClickSave();

            KeyboardCommands.SendTabs(3);
            Playback.Wait(50);
            KeyboardCommands.SendKey(sourceName);
            Playback.Wait(50);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
        }
    }
}
