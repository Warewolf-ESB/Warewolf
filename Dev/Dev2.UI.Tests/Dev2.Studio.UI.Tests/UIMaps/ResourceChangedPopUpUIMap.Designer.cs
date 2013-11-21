using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    public partial class ResourceChangedPopUpUIMap : UIMapBase
    {
        
        /// <summary>
        /// ClickViewDependancies
        /// </summary>
        public void ClickViewDependancies()
        {
            #region Variable Declarations
            WpfButton uIViewDependenciesButton = this.UIInputsOutputsChangedWindow.UIViewDependenciesButton;
            #endregion

            // Click 'View Dependencies' button
            Mouse.Click(uIViewDependenciesButton);
        }
        
        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            //#region Variable Declarations
            //WpfButton uICancelButton = this.UIInputsOutputsChangedWindow.UICancelButton;
            //#endregion

            //// Click 'Cancel' button
            //Mouse.Click(uICancelButton);

            Playback.Wait(150);
            SendKeys.SendWait("{ESC}");
            Playback.Wait(50);

        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public bool WaitForDialog(int timeOut)
        {
            Type type = null;
            var timeNow = 0;
            while(type != typeof(WpfWindow))
            {
                timeNow = timeNow + 100;
                Playback.Wait(100);
                var tryGetDialog = StudioWindow.GetChildren()[0];
                type = tryGetDialog.GetType();
                if(timeNow > timeOut)
                {
                    break;
                }
            }
            return type == typeof(WpfWindow);
        }
        
        #region Properties
        public UIInputsOutputsChangedWindow UIInputsOutputsChangedWindow
        {
            get
            {
                if ((this.mUIInputsOutputsChangedWindow == null))
                {
                    this.mUIInputsOutputsChangedWindow = new UIInputsOutputsChangedWindow();
                }
                return this.mUIInputsOutputsChangedWindow;
            }
        }
        #endregion
        
        #region Fields
        private UIInputsOutputsChangedWindow mUIInputsOutputsChangedWindow;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public class UIInputsOutputsChangedWindow : WpfWindow
    {
        
        public UIInputsOutputsChangedWindow()
        {
            #region Search Criteria
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "Inputs / Outputs Changed";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Inputs / Outputs Changed");
            #endregion
        }
        
        #region Properties
        public WpfButton UIViewDependenciesButton
        {
            get
            {
                if ((this.mUIViewDependenciesButton == null))
                {
                    this.mUIViewDependenciesButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIViewDependenciesButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "UI_DeleteResourceShowDependenciesBtn_AutoID";
                    this.mUIViewDependenciesButton.WindowTitles.Add("Inputs / Outputs Changed");
                    #endregion
                }
                return this.mUIViewDependenciesButton;
            }
        }
        
        public WpfButton UICancelButton
        {
            get
            {
                if ((this.mUICancelButton == null))
                {
                    this.mUICancelButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUICancelButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "UI_DeleteResourceNoBtn_AutoID";
                    this.mUICancelButton.WindowTitles.Add("Inputs / Outputs Changed");
                    #endregion
                }
                return this.mUICancelButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIViewDependenciesButton;
        
        private WpfButton mUICancelButton;
        #endregion
    }
}
