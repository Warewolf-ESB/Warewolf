using System.Threading;
using System.Windows.Forms;
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
            Mouse.DoubleClick(uIItemImage, new Point(760, 484));
        }

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void HitDoneWithKeyboard()
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
        public void GetFirstIntellisense(string startWith, bool deleteText = false, Point mousePoint = default(Point))
        {
            SendKeys.SendWait(startWith);
            Thread.Sleep(250);

            if (mousePoint != default(Point) )
            {
                // nasty fixed sizing, but no other real choice to test what  I need to ;(
                Mouse.Click(new Point(UIBusinessDesignStudioWindow.Left + mousePoint.X, UIBusinessDesignStudioWindow.Top + mousePoint.Y));
            }
            else
            {
                SendKeys.SendWait("{DOWN}");
                Thread.Sleep(250);
                SendKeys.SendWait("{ENTER}");     
            }

            if (deleteText)
            {
                Thread.Sleep(250);
                SendKeys.SendWait("^A");
                Thread.Sleep(250);
                SendKeys.SendWait("^C");

                Thread.Sleep(250);
                SendKeys.SendWait("{DELETE}");
            }
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
