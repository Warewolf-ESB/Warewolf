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
