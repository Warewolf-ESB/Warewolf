using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
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
