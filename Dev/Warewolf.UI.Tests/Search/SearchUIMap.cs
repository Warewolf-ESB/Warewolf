namespace Warewolf.UI.Tests.Search.SearchUIMapClasses
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
    
    
    public partial class SearchUIMap
    {
        
        private RecordedMethod1Params mRecordedMethod1Params;
        
        public virtual RecordedMethod1Params RecordedMethod1Params
        {
            get
            {
                if ((this.mRecordedMethod1Params == null))
                {
                    this.mRecordedMethod1Params = new RecordedMethod1Params();
                }
                return this.mRecordedMethod1Params;
            }
        }
        
        /// <summary>
        /// RecordedMethod1 - Use 'RecordedMethod1Params' to pass parameters into this method.
        /// </summary>
        public void RecordedMethod1()
        {
            #region Variable Declarations
            WpfCheckBox uIAllCheckBox = this.UIWarewolfDEV2SANELEMTWindow.UISearchViewUserControCustom.UISearchOptionsExpander.UIAllCheckBox;
            #endregion

            // Select 'All' check box
            uIAllCheckBox.Checked = this.RecordedMethod1Params.UIAllCheckBoxChecked;
        }
    }
    
    /// <summary>
    /// Parameters to be passed into 'RecordedMethod1'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class RecordedMethod1Params
    {
        
        #region Fields
        /// <summary>
        /// Select 'All' check box
        /// </summary>
        public bool UIAllCheckBoxChecked = true;
        #endregion
    }
}
