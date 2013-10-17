using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses
{
    using System;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    public partial class DecisionWizardUIMap
    {
        private UIWarewolfWindow mUIWarewolfWindow;

        #region Properties

        public UIWarewolfWindow UIWarewolfWindow
        {
            get
            {
                if((mUIWarewolfWindow == null))
                {
                    mUIWarewolfWindow = new UIWarewolfWindow();
                }
                return mUIWarewolfWindow;
            }
        }

        #endregion

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            WpfImage uIItemImage = this.UIWarewolfWindow.UIItemImage;
            Mouse.DoubleClick(uIItemImage, new Point(760, 484));
        }

        /// <summary>
        /// ClickOK
        /// </summary>
        public void ClickDone()
        {
            var window = new UIBusinessDesignStudioWindow();
            var wizard = window.GetChildren()[0].GetChildren()[0];
            Mouse.Click(wizard, new Point(650, 484));
        }

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void HitDoneWithKeyboard()
        {
            UITestControl decisionDialog = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            // Click middle of the image to set focus
            Mouse.Click(decisionDialog, new Point(decisionDialog.BoundingRectangle.X + decisionDialog.Width / 2, decisionDialog.BoundingRectangle.Y + decisionDialog.Height / 2));
            SendKeys.SendWait("{TAB}");
            Playback.Wait(700);
            SendKeys.SendWait("{ENTER}");
        }

        /// <summary>
        /// Select nth menu item
        /// </summary>
        public void SelectMenuItem(int n)
        {
            for(int i = 0; i < n; i++)
            {
                SendKeys.SendWait("{DOWN}");
            }
        }

        /// <summary>
        /// Send n tabs
        /// </summary>
        public void SendTabs(int n)
        {
            for(int i = 0; i < n; i++)
            {
                Playback.Wait(700);
                SendKeys.SendWait("{TAB}");
            }
        }

        /// <summary>
        /// Gets the first intellisense result
        /// </summary>
        public void GetFirstIntellisense(string startWith, bool deleteText = false, Point relativeToWizard = default(Point))
        {
            var wizard = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];

            //prompt intellisense
            SendKeys.SendWait(startWith);

            //wait for intellisense to drop down
            Playback.Wait(1000);

            if (relativeToWizard != default(Point) )
            {
                // nasty fixed sizing, but no other real choice to test what  I need to ;(
                Mouse.Click(new Point(wizard.Left + relativeToWizard.X, wizard.Top + relativeToWizard.Y));
            }
            else
            {
                Keyboard.SendKeys(wizard, "{DOWN}");
                Playback.Wait(250);
                Keyboard.SendKeys(wizard, "{ENTER}");     
            }

            if (deleteText)
            {
                SendKeys.SendWait("^a");
                Playback.Wait(250);
                SendKeys.SendWait("^x");
                Playback.Wait(250);
            }
        }
    }
}
