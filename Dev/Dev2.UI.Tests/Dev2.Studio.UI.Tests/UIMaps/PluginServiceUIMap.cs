using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Clipboard = System.Windows.Clipboard;

namespace Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses
{
    using System;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


    public partial class PluginServiceWizardUIMap : UIMapBase
    {
        /// <summary>
        /// ClickFirstAction
        /// </summary>
        public void ClickMappingTab(int x = 280)
        {
            UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Playback.Wait(500);
            Mouse.Click(uIItemImage, new Point(x, 25));
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

        public void ClickOK()
        {
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }

        #region Properties
        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
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
        private UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;
        #endregion

        public void ClickActionAtIndex(int i)
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(172, (164 + (30 * i))));
        }

        public string GetActionName()
        {
            var persistClipboard = Clipboard.GetText();
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.StartDragging(wizard, new Point(398, 83));
            Mouse.StopDragging(wizard, 55, 0);
            Keyboard.SendKeys(wizard, "{CTRL}c");
            var actionName = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            return actionName;
        }
    }
}
