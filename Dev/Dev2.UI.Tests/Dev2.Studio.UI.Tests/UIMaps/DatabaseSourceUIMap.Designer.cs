
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
