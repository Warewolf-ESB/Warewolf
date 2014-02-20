using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class WebSourceWizardUIMap : UIMapBase
    {
        public void ClickSave()
        {
            var control = StudioWindow.GetChildren()[0].GetChildren()[0];
            control.WaitForControlEnabled();
            Mouse.Click(control, new Point(648, 501));
        }

        public void EnterTextForDefaultQueryIfFocusIsSet(string textToEnter)
        {
            Keyboard.SendKeys(textToEnter);
        }
    }
}