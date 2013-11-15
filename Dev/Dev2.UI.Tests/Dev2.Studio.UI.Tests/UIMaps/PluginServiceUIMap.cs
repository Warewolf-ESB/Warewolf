using System.Windows.Forms;
using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests.UIMaps
{
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
        public DatabaseServiceWizardUIMapClasses.UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        {
            get
            {
                if ((this.mUIBusinessDesignStudioWindow == null))
                {
                    this.mUIBusinessDesignStudioWindow = new DatabaseServiceWizardUIMapClasses.UIBusinessDesignStudioWindow();
                }
                return this.mUIBusinessDesignStudioWindow;
            }
        }
        #endregion

        #region Fields
        private DatabaseServiceWizardUIMapClasses.UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;
        #endregion
    }
}
