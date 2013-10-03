﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 11.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses
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
    
    
    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public partial class SaveDialogUIMap
    {
        
        /// <summary>
        /// ClickAndTypeInFilterTextBox - Use 'ClickAndTypeInFilterTextBoxParams' to pass parameters into this method.
        /// </summary>
        public void ClickAndTypeInFilterTextBox(string textToType)
        {
            var kids = this.UIBusinessDesignStudioWindow.GetChildren();
            var subKids = kids[0].GetChildren();

            UITestControl uIItemImage = subKids[0];         
            
            //// Click image
            Mouse.Click(uIItemImage, new Point(424, 69));

            //// Type text in 'Wpf' window
            Keyboard.SendKeys(uIItemImage, textToType, ModifierKeys.None);
        }
        
        /// <summary>
        /// ClickAndTypeInNameTextbox - Use 'ClickAndTypeInNameTextboxParams' to pass parameters into this method.
        /// </summary>
        public void ClickAndTypeInNameTextbox(string textToType)
        {
            #region Variable Declarations
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            #endregion

            // Click image
            Mouse.Click(uIItemImage, new Point(145, 364));

            // Type text in 'Wpf' window
            Keyboard.SendKeys(uIItemImage, textToType, ModifierKeys.None);
        }
        
        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            #region Variable Declarations
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            #endregion

            // Click image
            Mouse.Click(uIItemImage, new Point(554, 423));
        }
        
        /// <summary>
        /// ClickCategory
        /// </summary>
        public void ClickCategory()
        {
            #region Variable Declarations
            UITestControl uIItemImage = this.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            #endregion

            // Click image
            Mouse.Click(uIItemImage, new Point(74, 115));
        }
        
        /// <summary>
        /// ClickSave
        /// </summary>
        public void ClickSave()
        {
            //#region Variable Declarations

            //var children = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren();
            //UITestControl uIItemImage = UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            //#endregion
            //// Click image
            //Mouse.Click(uIItemImage, new Point(478, 432));
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}");
        }
        
        #region Properties
        public virtual ClickAndTypeInFilterTextBoxParams ClickAndTypeInFilterTextBoxParams
        {
            get
            {
                if ((this.mClickAndTypeInFilterTextBoxParams == null))
                {
                    this.mClickAndTypeInFilterTextBoxParams = new ClickAndTypeInFilterTextBoxParams();
                }
                return this.mClickAndTypeInFilterTextBoxParams;
            }
        }
        
        public virtual ClickAndTypeInNameTextboxParams ClickAndTypeInNameTextboxParams
        {
            get
            {
                if ((this.mClickAndTypeInNameTextboxParams == null))
                {
                    this.mClickAndTypeInNameTextboxParams = new ClickAndTypeInNameTextboxParams();
                }
                return this.mClickAndTypeInNameTextboxParams;
            }
        }
        
        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        {
            get
            {
                if ((this.mUIBusinessDesignStudioWindow == null))
                {
                    this.mUIBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
                }
                return this.mUIBusinessDesignStudioWindow;
            }
        }
        #endregion
        
        #region Fields
        private ClickAndTypeInFilterTextBoxParams mClickAndTypeInFilterTextBoxParams;
        
        private ClickAndTypeInNameTextboxParams mClickAndTypeInNameTextboxParams;
        
        private UIBusinessDesignStudioWindow mUIBusinessDesignStudioWindow;
        #endregion
    }
    
    /// <summary>
    /// Parameters to be passed into 'ClickAndTypeInFilterTextBox'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public class ClickAndTypeInFilterTextBoxParams
    {
        
        #region Fields
        /// <summary>
        /// Type 'Mo' in 'Wpf' window
        /// </summary>
        public string UIWpfWindowSendKeys = "Mo";
        #endregion
    }
    
    /// <summary>
    /// Parameters to be passed into 'ClickAndTypeInNameTextbox'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public class ClickAndTypeInNameTextboxParams
    {
        
        #region Fields
        /// <summary>
        /// Type 'MyNewWorkflow1' in 'Wpf' window
        /// </summary>
        public string UIWpfWindowSendKeys = "MyNewWorkflow1";
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
    public class UIBusinessDesignStudioWindow : WpfWindow
    {
        
        public UIBusinessDesignStudioWindow()
        {
            #region Search Criteria
            //this.SearchProperties[WpfWindow.PropertyNames.Name] = "Warewolf (DEV2\\massimo.guerrera)";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.Name, "Warewolf", PropertyExpressionOperator.Contains));
            //this.WindowTitles.Add("Warewolf (DEV2\\massimo.guerrera)");
            #endregion
        }
        
        #region Properties
        public WpfImage UIItemImage
        {
            get
            {
                if ((this.mUIItemImage == null))
                {
                    this.mUIItemImage = new WpfImage(this);
                    #region Search Criteria
                    this.mUIItemImage.WindowTitles.Add("Business Design Studio (DEV2\\massimo.guerrera)");
                    #endregion
                }
                return this.mUIItemImage;
            }
        }
        
        public WpfWindow UIWpfWindow
        {
            get
            {
                if ((this.mUIWpfWindow == null))
                {
                    this.mUIWpfWindow = new WpfWindow(this);
                    #region Search Criteria
                    this.mUIWpfWindow.SearchProperties[WpfWindow.PropertyNames.AutomationId] = "WebBrowserWindow";
                    this.mUIWpfWindow.WindowTitles.Add("Business Design Studio (DEV2\\massimo.guerrera)");
                    #endregion
                }
                return this.mUIWpfWindow;
            }
        }
        #endregion
        
        #region Fields
        private WpfImage mUIItemImage;
        
        private WpfWindow mUIWpfWindow;
        #endregion
    }
}
