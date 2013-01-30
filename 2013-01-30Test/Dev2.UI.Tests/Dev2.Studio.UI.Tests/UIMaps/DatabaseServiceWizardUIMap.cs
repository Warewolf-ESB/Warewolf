namespace Dev2.CodedUI.Tests.UIMaps.DatabaseServiceUIMapClasses
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
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    
    
    public partial class DatabaseServiceWizardUIMap
    {
        public void InputDatabaseServiceName(string serviceName) {
            EnterDatabaseServiceName(serviceName);
        }

        public void InputDatabaseCategoryName(string categoryName) {
            EnterCategory(categoryName);
        }

        public void ClickNextButton() {
            NextButtonClick();
        }

        public bool CloseWizard() {
            #region Variable Declarations
            WinWindow dbServiceWindow = this.UIDatabaseServiceDetaiWindow1;
            #endregion
            Keyboard.SendKeys(dbServiceWindow, "%{F4}");
            if(dbServiceWindow.Exists) {
                return false;
            }
            else {
                return true;
            }
        }

        public string GetWorkflowWizardName() {
            #region Variable Declarations
            WinWindow uIDatabaseServiceDetaiWindow1 = this.UIDatabaseServiceDetaiWindow1;
            #endregion
            if(uIDatabaseServiceDetaiWindow1.WindowTitles.Count > 1) {
                throw new Exception("More than 1 wizard window opened");
            }
            else {
                return uIDatabaseServiceDetaiWindow1.WindowTitles[0].ToString();
            }
        }
    }
}
