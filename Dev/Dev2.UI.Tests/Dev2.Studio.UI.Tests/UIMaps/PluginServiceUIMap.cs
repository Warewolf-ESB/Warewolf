
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
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Clipboard = System.Windows.Clipboard;

// ReSharper disable CheckNamespace
namespace Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses
// ReSharper restore CheckNamespace
{
    using System;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    // ReSharper disable InconsistentNaming
    public partial class PluginServiceWizardUIMap : UIMapBase
    // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// ClickFirstAction
        /// </summary>
        public void ClickMappingTab(int x = 280)
        {
            UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[2];
            Playback.Wait(500);
            Mouse.Click(uIItemImage, new Point(x, 25));
        }

        /// <summary>
        /// Selects all window contents.
        /// </summary>
        /// <returns></returns>
        public string GetWindowContents()
        {
            KeyboardCommands.SelectAndCopy();
            return Clipboard.GetText();
        }

        /// <summary>
        /// Edits the source.
        /// </summary>
        public void EditSource()
        {
            KeyboardCommands.SendTabs(2);
            KeyboardCommands.SendEnter();
        }

        /// <summary>
        /// Cancels the entire operation.
        /// </summary>
        public void CancelEntireOperation()
        {
            KeyboardCommands.SendEsc();
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
            for(int i = 0; i <= numberOfTabsToSaveButton; i++)
            {
                SendKeys.SendWait("{TAB}");
                Playback.Wait(50);
            }
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        public string GetWorkflowWizardName()
        {
            #region Variable Declarations
            WpfWindow uIPluginServiceDetailsWindow = GetWindow();
            #endregion
            if(uIPluginServiceDetailsWindow.WindowTitles.Count > 1)
            {
                throw new Exception("More than 1 wizard window opened");
            }
            return uIPluginServiceDetailsWindow.WindowTitles[0];
        }

        public void ClickCancel()
        {
            UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.Click(uIItemImage, new Point(874, 533));
        }

        public UITestControl GetWizardWindow()
        {
            Playback.Wait(150);
            return UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
        }

        public void HitTestAndOkWithKeyboard()
        {
            Playback.Wait(200);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            SendKeys.SendWait("test string");
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(200);
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }

        public void ClickTest()
        {
            UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.Click(uIItemImage, new Point(892, 79));
            Playback.Wait(7000);
        }

        // ReSharper disable InconsistentNaming
        public void ClickOK()
        // ReSharper restore InconsistentNaming
        {
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }

        #region Properties
        // ReSharper disable InconsistentNaming
        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        // ReSharper restore InconsistentNaming
        {
            get
            {
                if((mUIBusinessDesignStudioWindow == null))
                {
                    mUIBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
                }
                return mUIBusinessDesignStudioWindow;
            }
        }
        #endregion

        #region Fields
        // ReSharper disable InconsistentNaming
        private UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;
        // ReSharper restore InconsistentNaming
        #endregion

        public void ClickActionAtIndex(int i)
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(172, (164 + (25 * i))));
        }

        public string GetActionName()
        {
            var persistClipboard = Clipboard.GetText();
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            Mouse.StartDragging(wizard, new Point(398, 83));
            Mouse.StopDragging(wizard, 125, 0);
            Keyboard.SendKeys(wizard, "{CTRL}c");
            var actionName = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            return actionName;
        }
    }
}
