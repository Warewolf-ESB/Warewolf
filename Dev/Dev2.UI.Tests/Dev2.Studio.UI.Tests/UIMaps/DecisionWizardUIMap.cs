using System.Threading;
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
            #region Variable Declarations
            WpfImage uIItemImage = this.UIWarewolfWindow.UIItemImage;
            #endregion

            // Click image
            Mouse.Click(uIItemImage, new Point(760, 484));
        }

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void KeyboardDone()
        {
            UITestControl decisionDialog = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            // Click image
            Mouse.Click(decisionDialog, new Point(decisionDialog.BoundingRectangle.X + decisionDialog.Width / 2, decisionDialog.BoundingRectangle.Y + decisionDialog.Height / 2));
            Keyboard.SendKeys(decisionDialog, "{TAB}");
            Playback.Wait(700);
            Keyboard.SendKeys(decisionDialog, "{ENTER}");
        }

        /// <summary>
        /// Select nth menu item
        /// </summary>
        public void SelectMenuItem(int n)
        {
            for(int i = 0; i < n; i++)
            {
                Keyboard.SendKeys("{DOWN}");
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
                Keyboard.SendKeys("{TAB}");
            }
        }

        /// <summary>
        /// Gets the first intellisense result
        /// </summary>
        public void GetFirstIntellisense(string startWith)
        {
            Keyboard.SendKeys("[[V");
            Thread.Sleep(250);
            Keyboard.SendKeys("{DOWN}");
            Thread.Sleep(250);
            Keyboard.SendKeys("{ENTER}");
            Thread.Sleep(250);
            Keyboard.SendKeys("^A");
            Thread.Sleep(250);
            Keyboard.SendKeys("^C");
            Thread.Sleep(250);
            Keyboard.SendKeys("{DELETE}");
        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public bool WaitForDialog(int timeOut)
        {
            Type type = null;
            var timeNow = 0;
            while (type != typeof(WpfImage))
            {
                timeNow = timeNow + 100;
                Playback.Wait(100);
                type = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0].GetType();
                if (timeNow > timeOut)
                {
                    break;
                }
            }
            return type == typeof(WpfImage);
        }
    }
}
