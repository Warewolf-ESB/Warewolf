using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses
{
    using System.Drawing;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

    public partial class DatabaseServiceWizardUIMap
    {
        /// <summary>
        /// ClickScrollActionListUp
        /// </summary>
        public void ClickScrollActionListUp()
        {
            // Click image
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(368, 161));
        }

        public void ClickSecondAction()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(172, 179));
        }

        public string GetActionName()
        {
            var persistClipboard = System.Windows.Clipboard.GetText();
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            wizard.WaitForControlReady();
            Mouse.StartDragging(wizard, new Point(418, 81));
            Mouse.StopDragging(wizard, 108, -1);
            Keyboard.SendKeys(wizard, "{CTRL}c");
            var actionName = System.Windows.Clipboard.GetText();
            System.Windows.Clipboard.SetText(persistClipboard);
            return actionName;
        }

        public void EnterDataIntoMappingTextBox(int textboxNumber, string newMappingText)
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            wizard.WaitForControlReady();
            for(int i = 0; i <= textboxNumber; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
        }

        public void ClickSaveButton(int numberOfTabsToSaveButton)
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            wizard.WaitForControlReady();
            for(int i = 0; i <= numberOfTabsToSaveButton; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        public void CreateDbSource(string sourcePath, string sourceName, string category)
        {
            SendKeys.SendWait("{TAB}");
            Playback.Wait(10);
            SendKeys.SendWait(sourcePath);
            Playback.Wait(10);
            SendKeys.SendWait("{TAB}{RIGHT}{TAB}");
            Playback.Wait(10);
            SendKeys.SendWait("testuser");
            Playback.Wait(10);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(10);
            SendKeys.SendWait("test123");
            Playback.Wait(10);
            SendKeys.SendWait("{TAB}{ENTER}");
            Playback.Wait(1000);
            SendKeys.SendWait("{TAB}{DOWN}{TAB}{ENTER}{ENTER}");
            Playback.Wait(10);
            SendKeys.SendWait(category);
            Playback.Wait(10);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(1000);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(10);
            SendKeys.SendWait(sourceName);
            Playback.Wait(10);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(2500);
        }

        public void CreateDbService(string serviceName, string category)
        {
            ClickFirstAction();
            ClickTestAction();
            KeyboardOK();
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(10);
            SendKeys.SendWait(category);
            Playback.Wait(10);
            SendKeys.SendWait("{ENTER}{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(10);
            SendKeys.SendWait(serviceName);
            Playback.Wait(10);
            SendKeys.SendWait("{ENTER}");
        }
    }
}
