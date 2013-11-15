namespace Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Dev2.Studio.UI.Tests;

    public partial class WorkflowWizardUIMap : UIMapBase
    {
        public WpfWindow GetWindow()
        {
            #region Variable Declarations
            WpfWindow theWindow = StudioWindow.GetChildren()[0] as WpfWindow;
            #endregion

            return theWindow;
        }
    }
}
