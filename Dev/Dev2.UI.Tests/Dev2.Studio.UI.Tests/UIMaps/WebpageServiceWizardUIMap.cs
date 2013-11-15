using System.Threading;

namespace Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses
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
    
    
    public partial class WebpageServiceWizardUIMap
    {
        public string GetWorkflowWizardName()
        {
            #region Variable Declarations
            WpfWindow uIWebpageServiceDetailWindow = GetWindow();
            #endregion
            if (uIWebpageServiceDetailWindow.WindowTitles.Count > 1)
            {
                throw new Exception("More than 1 wizard window opened");
            }
            else
            {
                return uIWebpageServiceDetailWindow.WindowTitles[0].ToString();
            }
        }

        public bool CloseWizard()
        {
            WpfWindow uIWebpageServiceDetailWindow = GetWindow();
            Keyboard.SendKeys(uIWebpageServiceDetailWindow, "{ESC}");
            if (uIWebpageServiceDetailWindow.Exists)
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
