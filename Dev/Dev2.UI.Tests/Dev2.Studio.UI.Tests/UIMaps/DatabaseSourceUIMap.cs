using System.Windows;
using Microsoft.VisualStudio.TestTools.UITesting;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses
// ReSharper restore CheckNamespace
{
    using System.Drawing;
    using Mouse = Mouse;

    // ReSharper disable InconsistentNaming
    public partial class DatabaseSourceUIMap : WizardsUIMap
    {
        public string GetUserName()
        {
            KeyboardCommands.SendTabs(3, 250);
            KeyboardCommands.SelectAll();
            KeyboardCommands.SendKey(KeyboardCommands.CopyCommand);
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            var userName = Clipboard.GetText();
            //Clipboard.SetText(persistClipboard);
            Mouse.Click(wizard, new Point(584, 160));
            return userName;
        }

        public void ChangeAuthenticationTypeToUserFromWindows()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{RIGHT}");
            Playback.Wait(100);
        }

        public void ChangeAuthenticationTypeToWindowsFromUser()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{LEFT}");
            Playback.Wait(100);
        }

        public void EnterUsernameAndPassword(string user, string pass)
        {
            KeyboardCommands.SendTabs(3, 200);
            KeyboardCommands.SendKey(user);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey(pass);
            Playback.Wait(100);
        }

        public void TestConnection()
        {
            Keyboard.SendKeys("{TAB}{ENTER}");
            Playback.Wait(100);
        }

        public void ClickSaveDbConnectionFromTestConnection()
        {
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            Playback.Wait(100);
        }

        public void ClickSaveDbConnectionFromWindowsRadioButton()
        {
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            Playback.Wait(100);
        }
    }
}
