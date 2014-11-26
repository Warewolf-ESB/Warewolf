
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Clipboard = System.Windows.Clipboard;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses
// ReSharper restore CheckNamespace
{

    using System.Drawing;

    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    public partial class PluginSourceMap
    {
        public void TabToAssemblyPath()
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            Keyboard.SendKeys(wizard, "{TAB}{TAB}");
        }

        public void ClickPluginSourceAssemblyPath()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(423, 406));
        }

        public string GetAssemblyPathText()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[2], new Point(423, 406));
            StudioWindow.GetChildren()[0].GetChildren()[2].WaitForControlReady();
            Keyboard.SendKeys(StudioWindow.GetChildren()[0].GetChildren()[2], "{CTRL}a");
            StudioWindow.GetChildren()[0].GetChildren()[2].WaitForControlReady();
            Keyboard.SendKeys(StudioWindow.GetChildren()[0].GetChildren()[2], "{CTRL}c");
            StudioWindow.GetChildren()[0].GetChildren()[2].WaitForControlReady();
            Keyboard.SendKeys(StudioWindow.GetChildren()[0].GetChildren()[2], "{RIGHT}");
            return Clipboard.GetText();
        }
    }
}
