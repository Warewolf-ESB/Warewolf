
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

namespace Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.CodeDom.Compiler;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

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
            Playback.Wait(1150);
            SendKeys.SendWait("{ESC}");
            Playback.Wait(50);

        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public bool WaitForDialog(int timeOut)
        {
            Playback.Wait(1500);
            return true;
        }

        #region Properties
        public UIInputsOutputsChangedWindow UIInputsOutputsChangedWindow
        {
            get
            {
                if((this.mUIInputsOutputsChangedWindow == null))
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
                if((this.mUIViewDependenciesButton == null))
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
                if((this.mUICancelButton == null))
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
