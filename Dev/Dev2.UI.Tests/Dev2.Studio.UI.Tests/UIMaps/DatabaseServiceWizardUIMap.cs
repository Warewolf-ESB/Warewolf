using System.Windows;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses
{
    using System.Drawing;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    
    public partial class DatabaseServiceWizardUIMap
    {
        /// <summary>
        /// ClickScrollActionListUp
        /// </summary>
        public void ClickScrollActionListUp()
        {
            // Click image
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(368, 161));
        }

        public void ClickSecondAction()
        {
            Mouse.Click(StudioWindow.GetChildren()[0].GetChildren()[0], new Point(172, 179));
        }

        public string GetActionName()
        {
            var persistClipboard = Clipboard.GetText();
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.StartDragging(wizard, new Point(418, 81));
            Mouse.StopDragging(wizard, 108, -1);
            Keyboard.SendKeys(wizard, "{CTRL}c");
            var actionName = Clipboard.GetText();
            Clipboard.SetText(persistClipboard);
            return actionName;
        }
    }
}
