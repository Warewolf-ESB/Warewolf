namespace Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses
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
    
    
    public partial class PluginSourceMap
    {
        public void TabToAssemblyPath()
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            Keyboard.SendKeys(wizard, "{TAB}{TAB}");
        }

        public void ClickPluginSourceAssemblyPath()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(423, 406));
        }
    }
}
