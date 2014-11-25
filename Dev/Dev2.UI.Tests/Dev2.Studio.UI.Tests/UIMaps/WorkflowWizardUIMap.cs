
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Forms;
    
    
    public partial class WorkflowWizardUIMap
    {
        public bool DoesProcessErrorWindowExist()
        {
            bool processErrorWindowIsOpen = false;
            System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
            foreach (var item in processList)
            {
                if (item.MainWindowTitle == "Process Error")
                {
                    processErrorWindowIsOpen = true;
                    break;
                }
            }
            
            return processErrorWindowIsOpen;
        }
        public void EnterWorkflowName(string name) {
            #region Variable Declarations
            WinWindow uIServiceDetailsWindow1 = StudioWindow.GetChildren()[0] as WinWindow;
            #endregion
            uIServiceDetailsWindow1.Find();
            int l1 = uIServiceDetailsWindow1.BoundingRectangle.Left;
            int t1 = uIServiceDetailsWindow1.BoundingRectangle.Top;

            Point toClick = new Point(l1 + 184, t1 + 77); // Ack :< Required due to lack of Workflow Inspectibility
            
            // Click 'Name' Textbox
            Mouse.Click(toClick);

            // Type 'CodedUITestWorkflow' in 'Workflow Name' textbox
            Keyboard.SendKeys(uIServiceDetailsWindow1, name, ModifierKeys.None);

        }


        public void EnterWorkflowCategory(string categoryName) {
            #region Variable Declarations
            WinWindow uIServiceDetailsWindow1 = StudioWindow.GetChildren()[0] as WinWindow;
            #endregion

            int l1 = uIServiceDetailsWindow1.BoundingRectangle.Left;
            int t1 = uIServiceDetailsWindow1.BoundingRectangle.Top;

            Point toClick = new Point(l1 + 185, t1 + 138); // Ack :< Required due to lack of Workflow Inspectibility
            // Click 'Service Details' window
            Mouse.Click(toClick);

            // Clear any existing Category
            Mouse.DoubleClick();
            SendKeys.SendWait("{DELETE}");

            // Type 'CodedUITestCategory' in 'Service Details' window
            Keyboard.SendKeys(uIServiceDetailsWindow1, categoryName, ModifierKeys.None);
        }


        /// <summary>
        /// Click the "Done" button on the Workflow Service Details Wizard
        /// </summary>
        public void DoneButtonClick()
        {
            #region Variable Declarations
            WinWindow uIServiceDetailsWindow1 = StudioWindow.GetChildren()[0] as WinWindow;
            #endregion

            int l1 = uIServiceDetailsWindow1.BoundingRectangle.Left;
            int t1 = uIServiceDetailsWindow1.BoundingRectangle.Top;

            Point toClick = new Point(l1 + 471, t1 + 190); // Ack :< Required due to lack of Workflow Inspectibility

            // Click 'Workflow Service Details' window
            Mouse.Click(toClick);
        }

        public string GetWorkflowWizardName() {
            #region Variable Declarations
            WinWindow uIWorkflowServiceDetaiWindow1 = StudioWindow.GetChildren()[0] as WinWindow;
            #endregion
            if(uIWorkflowServiceDetaiWindow1.WindowTitles.Count > 1) {
                throw new Exception("More than 1 wizard window opened");
            }
            else {
                return uIWorkflowServiceDetaiWindow1.WindowTitles[0].ToString();
            }
        }
    }
}
