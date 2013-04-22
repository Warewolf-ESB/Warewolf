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

        public bool CloseWizard()
        {
            WpfWindow uIPluginServiceDetailsWindow = GetWindow();
            Keyboard.SendKeys(uIPluginServiceDetailsWindow, "%{F4}");
            if (uIPluginServiceDetailsWindow.Exists)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
