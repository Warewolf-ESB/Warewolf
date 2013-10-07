using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;

namespace Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
    public partial class PluginServiceWizardUIMap
    {
        public string GetWorkflowWizardName()
        {
            #region Variable Declarations
            WpfWindow uIPluginServiceDetailsWindow = GetWindow();
            #endregion
            if (uIPluginServiceDetailsWindow.WindowTitles.Count > 1)
            {
                throw new Exception("More than 1 wizard window opened");
            }
            else
            {
                return uIPluginServiceDetailsWindow.WindowTitles[0].ToString();
            }
        }

        public void ClickCancel()
        {
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];

            // Click image
            Mouse.Click(uIItemImage, new Point(874, 533));
        }

        public void KeyboardSave()
        {
            Playback.Wait(200);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            Keyboard.SendKeys("test string");
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(200);
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }

        public void ClickOK()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }

        /// <summary>
        /// Click Test, Save, Save
        /// </summary>
        public void ClickSave()
        {
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];

            // Click image
            Mouse.Click(uIItemImage, new Point(887, 73));

            // Click image
            Mouse.Click(uIItemImage, new Point(771, 526));

            // Click image
            Mouse.Click(uIItemImage, new Point(612, 474));
        }

        #region Properties
        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        {
            get
            {
                if ((this.mUIBusinessDesignStudioWindow == null))
                {
                    this.mUIBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
                }
                return this.mUIBusinessDesignStudioWindow;
            }
        }
        #endregion

        #region Fields
        private UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;
        #endregion
    }
}
