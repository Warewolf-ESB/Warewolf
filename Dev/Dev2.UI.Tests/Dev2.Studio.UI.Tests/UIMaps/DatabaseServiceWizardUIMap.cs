
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses
// ReSharper restore CheckNamespace
{
    using System.Drawing;
    using Mouse = Mouse;

    public partial class DatabaseServiceWizardUIMap
    {
        /// <summary>
        /// ClickScrollActionListUp
        /// </summary>
        public void ClickScrollActionListUp()
        {
            // Click image
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(368, 161));
        }

        public void ClickSecondAction()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(172, 179));
        }

        public void ClickFourthAction()
        {
            var kids = StudioWindow.GetChildren();
            var grandKids = kids[0].GetChildren();
            Mouse.Click(grandKids[2], new Point(172, 229));
        }

        public void ClickThirdAction()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(172, 199));
        }

        public string GetActionName()
        {
            var persistClipboard = System.Windows.Clipboard.GetText();
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
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
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            wizard.WaitForControlReady();
            for(int i = 0; i <= textboxNumber; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            Keyboard.SendKeys(newMappingText);
        }

        public void ClickSaveButton(int numberOfTabsToSaveButton)
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            wizard.WaitForControlReady();
            KeyboardCommands.SendTabs(numberOfTabsToSaveButton, 200);
            KeyboardCommands.SendEnter();
            Playback.Wait(500);
        }

        public void CreateDbSource(string sourcePath, string sourceName)
        {
            //KeyboardCommands.SendTab();
            KeyboardCommands.SendKey(sourcePath);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendRightArrows(1);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey(@"testuser");
            KeyboardCommands.SendTab();
            KeyboardCommands.SendKey("test123");
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
            Playback.Wait(2500);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendDownArrows(1);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
            KeyboardCommands.SendTabs(3);
            KeyboardCommands.SendKey(sourceName);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
            Playback.Wait(1500);
        }

        public void CreateDbService(string serviceName)
        {
            ClickFirstAction();
            ClickTestAction();
            KeyboardCommands.SendTabs(5);
            KeyboardCommands.SendEnter();
            KeyboardCommands.SendTabs(3);
            SendKeys.SendWait(serviceName);
            Playback.Wait(10);
            KeyboardCommands.SendEnter();
        }
    }
}
