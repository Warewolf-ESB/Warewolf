using System.Threading;
using System.Windows.Forms;
using Dev2.CodedUI.Tests;

namespace Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    public partial class DatabaseSourceUIMap
    {
        
        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            Thread.Sleep(150);
            // Click image
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}");
        }
        
          
        /// <summary>
        /// ClickSave
        /// </summary>
        public void ClickSaveConnection()
        {
            Thread.Sleep(150);
            // Click image
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }
    }
}
