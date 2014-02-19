using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class WebSourceWizardUIMap : UIMapBase
    {
        public void ClickSave()
        {
            //var control = StudioWindow.GetChildren()[0].GetChildren()[0];
            //control.WaitForControlEnabled();
            //Mouse.Click(control, new Point(648, 501));
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}");
            Playback.Wait(1000);
        }

        public void EnterDefaultQuery(string textToEnter)
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}" + textToEnter);
            Playback.Wait(100);
        }


        public void DefaultQuerySetFocus()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}");
        }

        public void EnterTextForDefaultQueryIfFocusIsSet(string textToEnter)
        {
            Keyboard.SendKeys(textToEnter);
        }
    }
}