
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Forms;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses
// ReSharper restore CheckNamespace
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    // ReSharper disable InconsistentNaming
    public partial class DecisionWizardUIMap : UIMapBase
    // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// ClickCancel
        /// </summary>
        public void CancelWizard(int waitAmt = 2500)
        {
            Playback.Wait(waitAmt);
            KeyboardCommands.SendEsc(250);
        }

        /// <summary>
        /// ClickOK
        /// </summary>
        public void ClickDone(int waitAmt = 0)
        {
            Playback.Wait(waitAmt);
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];
            Mouse.Click(wizard, new Point(650, 450));
        }

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void HitDoneWithKeyboard()
        {
            UITestControl decisionDialog = StudioWindow.GetChildren()[0].GetChildren()[2];
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
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[2];

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
