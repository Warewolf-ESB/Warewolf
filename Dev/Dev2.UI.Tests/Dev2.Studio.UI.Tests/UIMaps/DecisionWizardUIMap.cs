using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    public partial class DecisionWizardUIMap : UIMapBase
    {
        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            Playback.Wait(3500);
            var win = StudioWindow.GetChildren()[0].GetChildren()[0];
            win.WaitForControlEnabled();

            Mouse.Click(win, new Point(760, 484));
        }

        /// <summary>
        /// ClickOK
        /// </summary>
        public void ClickDone(int waitAmt = 0)
        {
            Playback.Wait(waitAmt);
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            Mouse.Click(wizard, new Point(650, 484));
        }

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void HitDoneWithKeyboard()
        {
            UITestControl decisionDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
            // Click middle of the image to set focus
            Mouse.Click(decisionDialog, new Point(decisionDialog.BoundingRectangle.X + decisionDialog.Width / 2, decisionDialog.BoundingRectangle.Y + decisionDialog.Height / 2));
            SendKeys.SendWait("{TAB}");
            Playback.Wait(200);
            SendKeys.SendWait("{ENTER}");
        }

        /// <summary>
        /// Select nth menu item
        /// </summary>
        public void SelectMenuItem(int n, int waitAmt = 0)
        {
            for(int i = 0; i < n; i++)
            {
                Playback.Wait(50);
                SendKeys.SendWait("{DOWN}");
            }

            Playback.Wait(waitAmt);
        }

        /// <summary>
        /// Send n tabs
        /// </summary>
        public void SendTabs(int n, int waitAmt = 0)
        {
            for(int i = 0; i < n; i++)
            {
                Playback.Wait(50);
                SendKeys.SendWait("{TAB}");
            }

            Playback.Wait(waitAmt);
        }

        /// <summary>
        /// Gets the first intellisense result
        /// </summary>
        public void GetFirstIntellisense(string startWith, bool deleteText = false, Point relativeToWizard = default(Point))
        {
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];

            //prompt intellisense
            SendKeys.SendWait(startWith);

            //wait for intellisense to drop down
            Playback.Wait(500);

            if(relativeToWizard != default(Point))
            {
                Playback.Wait(1000);
                // nasty fixed sizing, but no other real choice to test what  I need to ;(
                Mouse.Click(new Point(wizard.Left + relativeToWizard.X, wizard.Top + relativeToWizard.Y));
            }
            else
            {
                Keyboard.SendKeys(wizard, "{DOWN}");
                Playback.Wait(250);
                Keyboard.SendKeys(wizard, "{ENTER}");
            }

            if(deleteText)
            {
                SendKeys.SendWait("^a");
                Playback.Wait(150);
                SendKeys.SendWait("^x");
                Playback.Wait(150);
            }
        }
    }
}
