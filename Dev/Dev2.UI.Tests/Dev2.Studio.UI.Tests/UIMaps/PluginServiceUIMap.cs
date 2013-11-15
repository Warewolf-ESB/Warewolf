using System.Windows.Forms;
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
            UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.Click(uIItemImage, new Point(874, 533));
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
