namespace Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses
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
    
    
    public partial class DocManagerUIMap
    {
        /// <summary>
        /// Clicks open one of the DocManager tabs
        /// </summary>
        /// <param name="tabName">The name of the tab (EG: Explorer, Toolbox, Variables, Output, etc)</param>
        public void ClickOpenTabPage(string tabName)
        {
            WpfTabPage theTab = FindTabPage(tabName);
            Mouse.Click(theTab, new Point(5, 5));
        }

        public bool DoesTabExist(string tabName)
        {
            WpfTabPage theTab = FindTabPage(tabName);
            Point p = new Point();
            return (theTab.TryGetClickablePoint(out p));
        }
    }
}
